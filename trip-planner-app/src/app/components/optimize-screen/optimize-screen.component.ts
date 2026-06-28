import { Component, OnDestroy, OnInit } from '@angular/core';

import { CommonModule } from '@angular/common';

import { ActivatedRoute, Router, RouterModule } from '@angular/router';

import { Subscription, interval, switchMap, takeWhile, catchError, of } from 'rxjs';

import { TripService } from '../../services/trip.service';

import { TripStateService } from '../../services/trip-state.service';

import { OptimizationStepTrace, OptimizationProgress, OptimizeRequest, ScoreTableCellTrace, Trip } from '../../models/models';
import { ScoreTableGridComponent } from '../score-table-grid/score-table-grid.component';



@Component({

  selector: 'app-optimize-screen',

  standalone: true,

  imports: [CommonModule, RouterModule, ScoreTableGridComponent],

  template: `

    <div class="container">

      <h2>תוצאת המערכת</h2>



      <div *ngIf="trip" class="summary-card">

        <h3>{{ trip.tripName }}</h3>

        <p>התחלה: {{ trip.addressStart || '—' }}</p>

        <p>שעות: {{ request.tripStartTime }} – {{ request.tripEndTime }}</p>

      </div>



      <button (click)="optimize()" [disabled]="loading" class="btn-primary">

        {{ loading ? 'מחשב מסלול...' : 'חשב מסלול אופטימלי' }}

      </button>



      <div *ngIf="loading || pipelineSteps.length" class="pipeline-card">

        <h3>מעקב שלבים</h3>

        <ul class="pipeline-list">

          <li *ngFor="let step of pipelineSteps"

              [class.running]="step.status === 'Running'"

              [class.done]="step.status === 'Completed'"

              [class.failed]="step.status === 'Failed'">

            <span class="icon">{{ stepIcon(step) }}</span>

            <div class="step-body">

              <strong>{{ step.label }}</strong>

              <span class="status">{{ stepStatusLabel(step) }}</span>

              <p *ngIf="step.detail" class="detail">{{ step.detail }}</p>

              <p *ngIf="step.durationMs != null" class="duration">{{ step.durationMs }} ms</p>

            </div>

          </li>

        </ul>

      </div>

      <app-score-table-grid
        *ngIf="showScoreTable"
        [cells]="scoreTableCells"
        [httpCompleted]="scoreTableCellsBuilt"
        [httpEstimated]="scoreTableCellsTotal || scoreTableCellsBuilt"
        [waiting]="scoreTableStepActive && scoreTableCells.length === 0"
        title="בניית טבלת ציונים תלת-ממדית">
      </app-score-table-grid>

      <div *ngIf="error" class="error">{{ error }}</div>

      <div class="btn-row">
        <a routerLink="/plan" class="btn-secondary">חזור לתכנון</a>
      </div>

    </div>

  `,

  styles: [`
    .btn-primary { margin-bottom: var(--tl-space-md); }
  `]

})

export class OptimizeScreenComponent implements OnInit, OnDestroy {

  private readonly cellByKey = new Map<string, ScoreTableCellTrace>();

  tripId = 0;

  trip: Trip | null = null;

  request: OptimizeRequest = {

    tripId: 0,

    tripStartTime: '08:00',

    tripEndTime: '18:00',

    maxTravelTime: 480,

    returnTravelTime: 60,

    minTransitEfficiency: 0.1

  };

  loading = false;

  error = '';

  pipelineSteps: OptimizationStepTrace[] = [];
  scoreTableCells: ScoreTableCellTrace[] = [];
  scoreTableCellsBuilt = 0;
  scoreTableCellsTotal = 0;
  private pollSub?: Subscription;

  get scoreTableStepActive(): boolean {
    return this.pipelineSteps.some(s => s.stepNumber === 2 && s.status === 'Running');
  }

  get showScoreTable(): boolean {
    return this.scoreTableStepActive || this.scoreTableCells.length > 0 || this.loading;
  }



  constructor(

    private route: ActivatedRoute,

    private router: Router,

    private tripService: TripService,

    private tripState: TripStateService

  ) {}



  ngOnInit(): void {

    this.tripId = Number(this.route.snapshot.paramMap.get('tripId'));

    this.request.tripId = this.tripId;



    const qp = this.route.snapshot.queryParamMap;

    this.request.tripStartTime       = qp.get('startTime') || '08:00';

    this.request.tripEndTime         = qp.get('endTime') || '18:00';

    this.request.maxTravelTime       = Number(qp.get('maxTravelTime') || 480);

    this.request.returnTravelTime    = Number(qp.get('returnTravelTime') || 60);

    this.tripService.getById(this.tripId).subscribe({

      next: t => this.trip = t,

      error: () => this.error = 'לא נמצא טיול'

    });

  }



  ngOnDestroy(): void {

    this.pollSub?.unsubscribe();

  }



  optimize(): void {
    if (this.loading) return;

    this.loading = true;
    this.error = '';
    this.tripState.clear(this.tripId);

    this.pipelineSteps = [];
    this.cellByKey.clear();
    this.scoreTableCells = [];
    this.scoreTableCellsBuilt = 0;
    this.scoreTableCellsTotal = 0;



    const traceId = crypto.randomUUID().replace(/-/g, '');

    const apiRequest = { ...this.buildApiRequest(), traceId };



    this.startProgressPolling(traceId);



    this.tripService.optimize(apiRequest).subscribe({
      next: result => this.handleOptimizeResult(result),
      error: err => {
        this.loading = false;
        this.pollSub?.unsubscribe();
        this.error = err.error?.error || err.error?.title || 'שגיאה בהפעלת חישוב המסלול';
      }
    });
  }

  private handleOptimizeResult(result: import('../../models/models').OptimizeResult): void {
    const legs = result.legs ?? [];
    const route = result.optimalRoute ?? [];

    if (result.scoreTableCellTrace?.length) {
      this.setScoreTableCells(result.scoreTableCellTrace);
    }

    if (result.pipelineTrace?.length) {
      this.pipelineSteps = result.pipelineTrace;
    }

    if (!route.length || !legs.length) {
      this.loading = false;
      this.pollSub?.unsubscribe();
      const validCells = result.scoreTableStats?.validCells ?? 0;
      const failedStep = result.pipelineTrace?.find(s => s.status === 'Failed');
      if (failedStep?.detail) {
        this.error = failedStep.detail;
      } else if (validCells === 0) {
        this.error = 'לא נמצאו תאים תקפים בטבלת הציונים. נסי להוריד את סף יעילות התחבורה או להרחיב את חלון הזמן.';
      } else {
        this.error = 'לא נמצא מסלול תקף מתוך היעדים שנבחרו. נסי להוריד את סף יעילות התחבורה או להאריך את שעות הטיול.';
      }
      return;
    }

    this.appendClientStep(9, 'SAVE_ROUTE', 'שמירת מסלול בבסיס הנתונים', 'Running');

    const destIds = route.map(d => d.desId);
    this.tripService.saveRoute(this.tripId, destIds).subscribe({
      next: () => this.finishOptimize(result),
      error: () => this.finishOptimize(result)
    });
  }

  private startProgressPolling(traceId: string): void {

    this.pollSub?.unsubscribe();

    this.pollSub = interval(800).pipe(

      switchMap(() => this.tripService.getOptimizeProgress(traceId).pipe(
        catchError(() => of<OptimizationProgress>({
          traceId,
          isComplete: false,
          hasError: false,
          steps: [],
          scoreTableCells: []
        }))
      )),

      takeWhile(p => !p.isComplete, true)

    ).subscribe({

      next: progress => {

        if (progress.steps?.length) {
          this.pipelineSteps = progress.steps;
        }

        if (progress.scoreTableCellsTotal != null) {
          this.scoreTableCellsTotal = progress.scoreTableCellsTotal;
        }
        if (progress.scoreTableCellsBuilt != null) {
          this.scoreTableCellsBuilt = progress.scoreTableCellsBuilt;
        }
        if (progress.scoreTableCells?.length) {
          this.mergeScoreTableCells(progress.scoreTableCells);
        }

        if (progress.hasError && progress.errorMessage) {
          this.loading = false;
          this.pollSub?.unsubscribe();
          this.error = progress.errorMessage;
          return;
        }

        if (progress.isComplete && !progress.hasError) {
          const ran = progress.steps?.some(s => s.status === 'Completed' || s.status === 'Running');
          if (!ran) {
            return;
          }
          this.pollSub?.unsubscribe();
        }

      },

      error: () => { /* polling continues via catchError above */ }

    });

  }



  private mergeScoreTableCells(incoming: ScoreTableCellTrace[]): void {
    for (const cell of incoming) {
      this.cellByKey.set(`${cell.i}-${cell.j}-${cell.h}`, cell);
    }
    this.scoreTableCells = Array.from(this.cellByKey.values()).sort(
      (a, b) => a.i - b.i || a.j - b.j || a.h - b.h
    );
    this.scoreTableCellsBuilt = this.scoreTableCells.length;
  }

  private setScoreTableCells(cells: ScoreTableCellTrace[]): void {
    this.cellByKey.clear();
    this.mergeScoreTableCells(cells);
    this.scoreTableCellsTotal = cells.length;
  }

  private finishOptimize(result: import('../../models/models').OptimizeResult): void {

    this.appendClientStep(9, 'SAVE_ROUTE', 'שמירת מסלול בבסיס הנתונים', 'Completed', 'המסלול נשמר');

    result.pipelineTrace = [...this.pipelineSteps];

    this.pollSub?.unsubscribe();

    this.tripState.setOptimizeResult(result);

    this.loading = false;

    this.router.navigate(['/trips', this.tripId, 'result']);

  }



  private appendClientStep(

    stepNumber: number,

    stepName: string,

    label: string,

    status: OptimizationStepTrace['status'],

    detail?: string

  ): void {

    const existing = this.pipelineSteps.findIndex(s => s.stepNumber === stepNumber && s.stepName === stepName);

    const step: OptimizationStepTrace = {

      stepNumber,

      stepName,

      label,

      status,

      detail

    };

    if (existing >= 0) {

      this.pipelineSteps[existing] = step;

    } else {

      this.pipelineSteps = [...this.pipelineSteps, step];

    }

  }



  stepIcon(step: OptimizationStepTrace): string {

    switch (step.status) {

      case 'Running': return '⟳';

      case 'Completed': return '✓';

      case 'Failed': return '✕';

      default: return '○';

    }

  }



  stepStatusLabel(step: OptimizationStepTrace): string {

    switch (step.status) {

      case 'Running': return 'בתהליך...';

      case 'Completed': return 'הושלם';

      case 'Failed': return 'נכשל';

      default: return 'ממתין';

    }

  }



  private buildApiRequest(): OptimizeRequest {

    const date = this.resolveTripDate(this.trip?.tripDate);

    return {

      tripId: this.request.tripId,

      tripStartTime: this.combineDateAndTime(date, this.request.tripStartTime),

      tripEndTime: this.combineDateAndTime(date, this.request.tripEndTime),

      maxTravelTime: this.request.maxTravelTime,

      returnTravelTime: this.request.returnTravelTime,

      minTransitEfficiency: this.request.minTransitEfficiency ?? 0.1

    };

  }



  private combineDateAndTime(date: string, time: string): string {

    const normalized = time.length === 5 ? `${time}:00` : time;

    return `${date}T${normalized}`;

  }

  private resolveTripDate(raw?: string): string {
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    const min = new Date(today);
    min.setDate(min.getDate() + 1);

    const max = new Date(today);
    max.setDate(max.getDate() + 14);

    let parsed = raw ? new Date(raw) : new Date(min);
    parsed.setHours(0, 0, 0, 0);

    if (isNaN(parsed.getTime()) || parsed < min) {
      parsed = min;
    } else if (parsed > max) {
      parsed = new Date(today);
      parsed.setDate(parsed.getDate() + 7);
    }

    return parsed.toISOString().split('T')[0];
  }

}


