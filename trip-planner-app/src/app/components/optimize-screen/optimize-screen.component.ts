import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';

import { CommonModule } from '@angular/common';

import { ActivatedRoute, Router, RouterModule } from '@angular/router';

import { Subscription, interval, switchMap, takeWhile } from 'rxjs';

import { TripService } from '../../services/trip.service';

import { TripStateService } from '../../services/trip-state.service';

import { OptimizationStepTrace, OptimizeRequest, ScoreTableCellTrace, Trip } from '../../models/models';



@Component({

  selector: 'app-optimize-screen',

  standalone: true,

  imports: [CommonModule, RouterModule],

  template: `

    <div class="container">

      <h2>חישוב מסלול אופטימלי</h2>



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

      <div *ngIf="scoreTableCellsTotal > 0" class="cells-card">
        <h3>בניית טבלה תלת-ממדית — תא {{ scoreTableCellsBuilt }} / {{ scoreTableCellsTotal }}</h3>
        <div class="cells-log" #cellsLog>
          <div *ngFor="let cell of scoreTableCells"
               class="cell-row"
               [class.invalid]="!cell.isValid">
            <span class="coords">[{{ cell.i }},{{ cell.j }},{{ cell.h }}]</span>
            <span class="route">{{ cell.fromLabel }} → {{ cell.toLabel }}</span>
            <span class="time">{{ cell.departureTime }}</span>
            <span class="score" [class.bad]="!cell.isValid">ציון {{ cell.transitionScore }}</span>
            <span class="meta">אוטובוס {{ cell.busTransitHours }}ש | הליכה {{ cell.walkingHours }}ש</span>
          </div>
        </div>
      </div>

      <div *ngIf="error" class="error">{{ error }}</div>

      <a routerLink="/plan" class="btn-secondary">חזור לתכנון</a>

    </div>

  `,

  styles: [`

    .container { padding: 24px; direction: rtl; max-width: 960px; margin: 0 auto; }

    .summary-card { background: #f0f7ff; padding: 16px; border-radius: 8px; margin-bottom: 20px; }

    .pipeline-card { background: #fafafa; border: 1px solid #e0e0e0; border-radius: 8px; padding: 16px; margin: 20px 0; }

    .pipeline-list { list-style: none; padding: 0; margin: 0; }

    .pipeline-list li { display: flex; gap: 12px; padding: 10px 0; border-bottom: 1px solid #eee; }

    .pipeline-list li:last-child { border-bottom: none; }

    .pipeline-list li.running { background: #e3f2fd; border-radius: 6px; padding: 10px 8px; }

    .pipeline-list li.done .icon { color: #2e7d32; }

    .pipeline-list li.failed { background: #ffebee; border-radius: 6px; }

    .icon { font-size: 1.2rem; width: 28px; text-align: center; }

    .step-body { flex: 1; }

    .status { color: #666; font-size: 0.85rem; margin-right: 8px; }

    .detail { margin: 4px 0 0; color: #444; font-size: 0.9rem; }

    .duration { margin: 2px 0 0; color: #888; font-size: 0.8rem; }
    .cells-card { background: #fffde7; border: 1px solid #fff176; border-radius: 8px; padding: 16px; margin: 20px 0; }
    .cells-card h3 { margin: 0 0 12px; color: #f57f17; font-size: 1rem; }
    .cells-log { max-height: 320px; overflow-y: auto; font-family: Consolas, monospace; font-size: 0.8rem; background: #fff; border: 1px solid #eee; border-radius: 6px; padding: 8px; }
    .cell-row { display: grid; grid-template-columns: 72px 1fr 48px 72px 1fr; gap: 8px; padding: 4px 0; border-bottom: 1px solid #f5f5f5; }
    .cell-row.invalid { color: #c62828; background: #fff5f5; }
    .cell-row .coords { color: #666; }
    .cell-row .score.bad { color: #c62828; }
    .cell-row .meta { color: #888; font-size: 0.75rem; }
    .btn-primary { background: #1976d2; color: white; padding: 12px 24px; border: none; border-radius: 8px; cursor: pointer; margin-left: 12px; }

    .btn-primary:disabled { background: #90caf9; }

    .btn-secondary { color: #1976d2; text-decoration: none; }

    .error { margin-top: 16px; color: #d32f2f; background: #ffebee; padding: 12px; border-radius: 6px; }

  `]

})

export class OptimizeScreenComponent implements OnInit, OnDestroy {

  tripId = 0;

  trip: Trip | null = null;

  request: OptimizeRequest = {

    tripId: 0,

    tripStartTime: '08:00',

    tripEndTime: '18:00',

    maxTravelTime: 480,

    returnTravelTime: 60,

    minTransitEfficiency: 0.5

  };

  loading = false;

  error = '';

  pipelineSteps: OptimizationStepTrace[] = [];
  scoreTableCells: ScoreTableCellTrace[] = [];
  scoreTableCellsBuilt = 0;
  scoreTableCellsTotal = 0;
  @ViewChild('cellsLog') cellsLog?: ElementRef<HTMLDivElement>;
  private pollSub?: Subscription;



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

    this.request.minTransitEfficiency = Number(qp.get('minTransitEfficiency') || 0.5);



    this.tripService.getById(this.tripId).subscribe({

      next: t => this.trip = t,

      error: () => this.error = 'לא נמצא טיול'

    });

  }



  ngOnDestroy(): void {

    this.pollSub?.unsubscribe();

  }



  optimize(): void {

    this.loading = true;

    this.error = '';

    this.pipelineSteps = [];
    this.scoreTableCells = [];
    this.scoreTableCellsBuilt = 0;
    this.scoreTableCellsTotal = 0;



    const traceId = crypto.randomUUID().replace(/-/g, '');

    const apiRequest = { ...this.buildApiRequest(), traceId };



    this.startProgressPolling(traceId);



    this.tripService.optimize(apiRequest).subscribe({

      next: result => {

        if (result.scoreTableCellTrace?.length) {
          this.scoreTableCells = result.scoreTableCellTrace;
          this.scoreTableCellsTotal = result.scoreTableCellTrace.length;
        }

        if (result.pipelineTrace?.length) {

          this.pipelineSteps = result.pipelineTrace;

        }

        this.appendClientStep(9, 'SAVE_ROUTE', 'שמירת מסלול בבסיס הנתונים', 'Running');



        const destIds = result.optimalRoute?.map(d => d.desId) ?? [];

        this.tripService.saveRoute(this.tripId, destIds).subscribe({

          next: () => this.finishOptimize(result),

          error: () => this.finishOptimize(result)

        });

      },

      error: err => {

        this.loading = false;

        this.pollSub?.unsubscribe();

        this.error = err.error?.error || err.error?.title || 'שגיאה בחישוב המסלול';

      }

    });

  }



  private startProgressPolling(traceId: string): void {

    this.pollSub?.unsubscribe();

    this.pollSub = interval(400).pipe(

      switchMap(() => this.tripService.getOptimizeProgress(traceId)),

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
          this.scoreTableCells = progress.scoreTableCells;
          setTimeout(() => this.scrollCellsToBottom(), 0);
        }

        if (progress.hasError && progress.errorMessage) {

          this.error = progress.errorMessage;

        }

      },

      error: () => {}

    });

  }



  private scrollCellsToBottom(): void {
    const el = this.cellsLog?.nativeElement;
    if (el) el.scrollTop = el.scrollHeight;
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

    const date = (this.trip?.tripDate || new Date().toISOString()).split('T')[0];

    return {

      tripId: this.request.tripId,

      tripStartTime: this.combineDateAndTime(date, this.request.tripStartTime),

      tripEndTime: this.combineDateAndTime(date, this.request.tripEndTime),

      maxTravelTime: this.request.maxTravelTime,

      returnTravelTime: this.request.returnTravelTime / 60,

      minTransitEfficiency: this.request.minTransitEfficiency

    };

  }



  private combineDateAndTime(date: string, time: string): string {

    const normalized = time.length === 5 ? `${time}:00` : time;

    return `${date}T${normalized}`;

  }

}


