import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TripService } from '../../services/trip.service';
import { OptimizeRequest, OptimizeResult } from '../../models/models';

interface RouteStop {
  order: number;
  destinationName: string;
  region: string;
  arrivalTime: string;
  departureTime: string;
  stayDuration: string;
  boardingStation: string;
  alightingStation: string;
  busLines: BusLeg[];
}

interface BusLeg {
  busNumber: string;
  boardingStation: string;
  alightingStation: string;
  departureTime: string;
  arrivalTime: string;
}

@Component({
  selector: 'app-optimizer',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="container">
      <h2>אופטימיזציית מסלול</h2>

      <div class="form-group">
        <label>מזהה טיול</label>
        <input type="number" [(ngModel)]="request.tripId" />
      </div>
      <div class="form-group">
        <label>שעת התחלה</label>
        <input type="time" [(ngModel)]="request.tripStartTime" />
      </div>
      <div class="form-group">
        <label>שעת סיום</label>
        <input type="time" [(ngModel)]="request.tripEndTime" />
      </div>
      <div class="form-group">
        <label>זמן נסיעה מקסימלי (דקות)</label>
        <input type="number" [(ngModel)]="request.maxTravelTime" />
      </div>
      <div class="form-group">
        <label>זמן חזרה (דקות)</label>
        <input type="number" [(ngModel)]="request.returnTravelTime" />
      </div>
      <div class="form-group">
        <label>יעילות תחבורה מינימלית (0-1)</label>
        <input type="number" step="0.01" min="0" max="1" [(ngModel)]="request.minTransitEfficiency" />
      </div>

      <button (click)="optimize()" [disabled]="loading" class="btn-primary">
        {{ loading ? 'מחשב...' : 'חשב מסלול אופטימלי' }}
      </button>

      <div *ngIf="result" class="result">
        <div class="summary">
          <h3>סיכום מסלול</h3>
          <div class="summary-grid">
            <div class="summary-item"><span class="label">מספר יעדים</span><span class="value">{{ result.destinationCount }}</span></div>
            <div class="summary-item"><span class="label">ציון כולל</span><span class="value">{{ result.totalScore | number:'1.2-2' }}</span></div>
            <div class="summary-item"><span class="label">זמן שנוצל</span><span class="value">{{ formatHours(result.timeUsed) }}</span></div>
            <div class="summary-item"><span class="label">זמן זמין</span><span class="value">{{ formatHours(result.timeAvailable) }}</span></div>
            <div class="summary-item"><span class="label">יעילות תחבורה</span><span class="value">{{ result.transitEfficiency | percent:'1.1-1' }}</span></div>
          </div>
        </div>

        <h3>מסלול מפורט</h3>
        <div class="route-table-wrapper">
          <table class="route-table">
            <thead>
              <tr>
                <th>#</th>
                <th>יעד</th>
                <th>אזור</th>
                <th>הגעה</th>
                <th>עזיבה</th>
                <th>משך שהייה</th>
                <th>פרטי נסיעה</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let stop of routeStops">
                <td class="center">{{ stop.order }}</td>
                <td class="dest-name">{{ stop.destinationName }}</td>
                <td class="center">{{ stop.region }}</td>
                <td class="center time">{{ stop.arrivalTime }}</td>
                <td class="center time">{{ stop.departureTime }}</td>
                <td class="center">{{ stop.stayDuration }}</td>
                <td class="transit-cell">
                  <div *ngFor="let leg of stop.busLines; let i = index" class="bus-leg">
                    <span *ngIf="stop.busLines.length > 1" class="leg-num">החלפה {{ i + 1 }}</span>
                    <div class="leg-details">
                      <span class="bus-badge">🚌 קו {{ leg.busNumber }}</span>
                      <span class="station">עלייה: {{ leg.boardingStation }}</span>
                      <span class="time-badge">{{ leg.departureTime }}</span>
                      <span class="arrow">←</span>
                      <span class="station">ירידה: {{ leg.alightingStation }}</span>
                      <span class="time-badge">{{ leg.arrivalTime }}</span>
                    </div>
                  </div>
                </td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div *ngIf="error" class="error">{{ error }}</div>
    </div>
  `,
  styles: [`
    .container { padding: 20px; direction: rtl; max-width: 1000px; }
    .form-group { margin-bottom: 12px; display: flex; flex-direction: column; }
    label { font-weight: bold; margin-bottom: 4px; }
    input { padding: 6px; border: 1px solid #ccc; border-radius: 4px; }
    .btn-primary { background: #1976d2; color: white; padding: 10px 20px; border: none; border-radius: 4px; cursor: pointer; margin-top: 8px; }
    .btn-primary:disabled { background: #aaa; }

    .result { margin-top: 24px; }

    .summary { background: #f0f7ff; padding: 16px; border-radius: 8px; margin-bottom: 24px; }
    .summary-grid { display: flex; flex-wrap: wrap; gap: 16px; margin-top: 12px; }
    .summary-item { display: flex; flex-direction: column; background: white; padding: 10px 16px; border-radius: 6px; min-width: 140px; }
    .summary-item .label { font-size: 12px; color: #666; }
    .summary-item .value { font-size: 20px; font-weight: bold; color: #1976d2; }

    .route-table-wrapper { overflow-x: auto; }
    .route-table { width: 100%; border-collapse: collapse; font-size: 14px; }
    .route-table th { background: #1976d2; color: white; padding: 10px 12px; text-align: right; }
    .route-table td { padding: 10px 12px; border-bottom: 1px solid #e0e0e0; vertical-align: top; }
    .route-table tr:nth-child(even) { background: #f9f9f9; }
    .route-table tr:hover { background: #e3f2fd; }

    .center { text-align: center; }
    .dest-name { font-weight: bold; }
    .time { font-family: monospace; font-size: 15px; }

    .transit-cell { min-width: 320px; }
    .bus-leg { margin-bottom: 8px; padding: 6px; background: #f5f5f5; border-radius: 4px; }
    .bus-leg:last-child { margin-bottom: 0; }
    .leg-num { font-size: 11px; color: #888; display: block; margin-bottom: 4px; }
    .leg-details { display: flex; flex-wrap: wrap; align-items: center; gap: 6px; }
    .bus-badge { background: #1976d2; color: white; padding: 2px 8px; border-radius: 12px; font-size: 13px; }
    .station { font-size: 13px; }
    .time-badge { background: #e8f5e9; color: #2e7d32; padding: 2px 6px; border-radius: 4px; font-family: monospace; font-size: 13px; }
    .arrow { color: #999; }

    .error { margin-top: 16px; color: #d32f2f; }
  `]
})
export class OptimizerComponent {
  request: OptimizeRequest = {
    tripId: 0,
    tripStartTime: '08:00',
    tripEndTime: '18:00',
    maxTravelTime: 480,
    returnTravelTime: 60,
    minTransitEfficiency: 0.5
  };

  result: OptimizeResult | null = null;
  routeStops: RouteStop[] = [];
  loading = false;
  error = '';

  constructor(private tripService: TripService) {}

  optimize(): void {
    this.loading = true;
    this.error = '';
    this.result = null;
    this.routeStops = [];

    this.tripService.optimize(this.request).subscribe({
      next: res => {
        this.result = res;
        this.routeStops = this.buildRouteStops(res);
        this.loading = false;
      },
      error: err => {
        this.error = 'שגיאה בחישוב המסלול: ' + (err.error?.error || err.message);
        this.loading = false;
      }
    });
  }

  private buildRouteStops(result: OptimizeResult): RouteStop[] {
    const stops: RouteStop[] = [];
    let currentTime = this.parseTime(this.request.tripStartTime);

    result.optimalRoute.forEach((dest, index) => {
      // זמן נסיעה לכל יעד (mock: שעה)
      const transitMinutes = 60;
      const arrivalTime = new Date(currentTime.getTime() + transitMinutes * 60000);

      // משך שהייה מה-timeDes
      const stayMinutes = dest.timeDes ? this.parseStayMinutes(dest.timeDes) : 90;
      const departureTime = new Date(arrivalTime.getTime() + stayMinutes * 60000);

      // תחנות mock — בעתיד יגיעו מה-API
      const boardingStation = 'תחנה מרכזית';
      const alightingStation = `תחנת ${dest.nameDes}`;
      const busNumber = `${100 + index + 1}`;

      stops.push({
        order: index + 1,
        destinationName: dest.nameDes,
        region: dest.region || '',
        arrivalTime: this.formatTime(arrivalTime),
        departureTime: this.formatTime(departureTime),
        stayDuration: this.formatDuration(stayMinutes),
        boardingStation,
        alightingStation,
        busLines: [{
          busNumber,
          boardingStation,
          alightingStation,
          departureTime: this.formatTime(currentTime),
          arrivalTime: this.formatTime(arrivalTime)
        }]
      });

      currentTime = departureTime;
    });

    return stops;
  }

  formatHours(hours: number): string {
    const h = Math.floor(hours);
    const m = Math.round((hours - h) * 60);
    return `${h}:${m.toString().padStart(2, '0')} שעות`;
  }

  private parseTime(timeStr: string): Date {
    const [h, m] = timeStr.split(':').map(Number);
    const d = new Date();
    d.setHours(h, m, 0, 0);
    return d;
  }

  private parseStayMinutes(timeDes: number | string): number {
    if (typeof timeDes === 'number') return timeDes;
    const parts = timeDes.split(':');
    return parseInt(parts[0]) * 60 + parseInt(parts[1]);
  }

  private formatTime(date: Date): string {
    return date.getHours().toString().padStart(2, '0') + ':' +
           date.getMinutes().toString().padStart(2, '0');
  }

  private formatDuration(minutes: number): string {
    const h = Math.floor(minutes / 60);
    const m = minutes % 60;
    return h > 0 ? `${h}ש' ${m}ד'` : `${m} דקות`;
  }
}
