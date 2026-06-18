import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { GoogleMap, MapMarker, MapPolyline } from '@angular/google-maps';
import { TripService } from '../../services/trip.service';
import { TripStateService } from '../../services/trip-state.service';
import { MapPoint, OptimizationStepTrace, ScoreTableCellTrace, ScoreTableStats, TripItinerary, TripLeg } from '../../models/models';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-trip-result',
  standalone: true,
  imports: [CommonModule, RouterModule, GoogleMap, MapMarker, MapPolyline],
  template: `
    <div class="page" *ngIf="itinerary">
      <div class="header">
        <h2>{{ itinerary.tripName }}</h2>
        <p class="sub">התחלה: {{ itinerary.addressStart }}</p>
        <div class="stats">
          <span>{{ itinerary.destinationCount }} יעדים</span>
          <span>ציון: {{ itinerary.totalScore | number:'1.1-1' }}</span>
          <span>יעילות: {{ itinerary.transitEfficiency | percent:'1.0-0' }}</span>
        </div>
      </div>

      <div *ngIf="pipelineTrace?.length" class="pipeline-box">
        <h3>מעקב שלבי האופטימיזציה</h3>
        <ul class="pipeline-list">
          <li *ngFor="let step of pipelineTrace" [class.failed]="step.status === 'Failed'">
            <span class="icon">{{ step.status === 'Completed' ? '✓' : step.status === 'Failed' ? '✕' : '○' }}</span>
            <div>
              <strong>{{ step.label }}</strong>
              <span class="meta" *ngIf="step.durationMs != null"> — {{ step.durationMs }} ms</span>
              <p *ngIf="step.detail" class="detail">{{ step.detail }}</p>
            </div>
          </li>
        </ul>
      </div>

      <div *ngIf="scoreTableCellTrace?.length" class="cells-box">
        <h3>טבלה תלת-ממדית — {{ (scoreTableCellTrace ?? []).length }} תאים</h3>
        <div class="cells-table-wrap">
          <table class="cells-table">
            <thead>
              <tr>
                <th>תא</th>
                <th>מסלול</th>
                <th>שעה</th>
                <th>ציון</th>
                <th>אוטובוס</th>
                <th>הליכה</th>
                <th>יעילות</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngFor="let cell of (scoreTableCellTrace ?? [])" [class.invalid]="!cell.isValid">
                <td>[{{ cell.i }},{{ cell.j }},{{ cell.h }}]</td>
                <td>{{ cell.fromLabel }} → {{ cell.toLabel }}</td>
                <td>{{ cell.departureTime }}</td>
                <td>{{ cell.transitionScore }}</td>
                <td>{{ cell.busTransitHours }}ש</td>
                <td>{{ cell.walkingHours }}ש</td>
                <td>{{ cell.transitEfficiency }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <div *ngIf="scoreTableStats" class="score-table-box">
        <h3>טבלת ציונים תלת-מימדית (ScoreTable)</h3>
        <p>{{ scoreTableStats.description }}</p>
        <div class="stats">
          <span>תאים: {{ scoreTableStats.totalCells }}</span>
          <span>תקפים: {{ scoreTableStats.validCells }}</span>
          <span>יחס תקפים: {{ scoreTableStats.validRatio | percent:'1.0-1' }}</span>
        </div>
      </div>

      <div class="layout">
        <div class="map-panel">
          <google-map *ngIf="showMap"
            [height]="'420px'"
            [width]="'100%'"
            [center]="mapCenter"
            [zoom]="mapZoom">
            <map-marker
              *ngFor="let p of itinerary.mapPoints"
              [position]="{ lat: p.lat, lng: p.lon }"
              [label]="p.order.toString()"
              [title]="p.label">
            </map-marker>
            <map-polyline *ngIf="polylinePath.length > 1" [path]="polylinePath"></map-polyline>
          </google-map>
          <p *ngIf="!showMap" class="map-hint">להצגת מפה: הגדר googleMapsApiKey ב-environment.ts והפעל Maps JavaScript API</p>
        </div>

        <div class="timeline">
          <h3>מסלול הטיול</h3>
          <div *ngFor="let leg of itinerary.legs" class="leg-card" [id]="'leg-' + leg.order">
            <div class="leg-header">
              <span class="order">{{ leg.order }}</span>
              <div>
                <strong>{{ leg.destinationName }}</strong>
                <span class="region">{{ leg.region }}</span>
              </div>
            </div>
            <img *ngIf="leg.imageUrl" [src]="leg.imageUrl" [alt]="leg.destinationName" class="dest-img"
                 (error)="onImgError($event)" />
            <div class="times">
              הגעה: {{ leg.arrivalTime }} | עזיבה: {{ leg.departureTime }} | שהייה: {{ leg.stayDuration }}
            </div>
            <div class="transit" *ngIf="leg.transit">
              <div *ngFor="let bus of leg.transit.busLines" class="bus-line">
                קו {{ bus.busNumber }} — {{ bus.direction }}
                <br>מ-{{ bus.fromStation }} ({{ bus.departureTime }}) ל-{{ bus.toStation }} ({{ bus.arrivalTime }})
              </div>
              <p *ngIf="leg.transit.alightingStation">תחנת ירידה: {{ leg.transit.alightingStation }}</p>
              <p *ngIf="leg.transit.walkingMinutes > 0">הליכה מהתחנה: {{ leg.transit.walkingMinutes | number:'1.0-0' }} דקות</p>
            </div>
          </div>
        </div>
      </div>

      <div *ngIf="itinerary.narrative" class="narrative">
        <h3>סיכום מילולי</h3>
        <pre>{{ itinerary.narrative }}</pre>
      </div>

      <div class="actions">
        <a routerLink="/my-trips" class="btn">חזור לטיולים שלי</a>
        <a routerLink="/plan" class="btn-primary">תכנן טיול חדש</a>
      </div>
    </div>

    <div *ngIf="loading" class="loading">טוען מסלול...</div>
    <div *ngIf="error" class="error-page">{{ error }}</div>
  `,
  styles: [`
    .page { padding: 20px; direction: rtl; max-width: 1200px; margin: 0 auto; }
    .header h2 { margin: 0; color: #1976d2; }
    .sub { color: #666; }
    .stats { display: flex; gap: 20px; margin: 12px 0 20px; }
    .stats span { background: #e3f2fd; padding: 8px 14px; border-radius: 8px; font-weight: 600; }
    .score-table-box { background: #fff8e1; border: 1px solid #ffe082; border-radius: 10px; padding: 14px 18px; margin-bottom: 20px; }
    .pipeline-box { background: #f3f8ff; border: 1px solid #bbdefb; border-radius: 10px; padding: 14px 18px; margin-bottom: 20px; }
    .cells-box { background: #fffde7; border: 1px solid #fff176; border-radius: 10px; padding: 14px 18px; margin-bottom: 20px; }
    .cells-box h3 { margin: 0 0 10px; color: #f57f17; font-size: 1rem; }
    .cells-table-wrap { max-height: 400px; overflow: auto; }
    .cells-table { width: 100%; border-collapse: collapse; font-size: 0.85rem; }
    .cells-table th, .cells-table td { padding: 6px 8px; border-bottom: 1px solid #eee; text-align: right; }
    .cells-table tr.invalid { background: #ffebee; color: #c62828; }
    .pipeline-box h3 { margin: 0 0 10px; color: #1565c0; font-size: 1rem; }
    .pipeline-list { list-style: none; padding: 0; margin: 0; }
    .pipeline-list li { display: flex; gap: 10px; padding: 6px 0; border-bottom: 1px solid #e3f2fd; font-size: 0.9rem; }
    .pipeline-list li.failed { color: #c62828; }
    .pipeline-list .icon { width: 20px; color: #2e7d32; }
    .pipeline-list .meta { color: #888; font-size: 0.85rem; }
    .pipeline-list .detail { margin: 2px 0 0; color: #555; font-size: 0.85rem; }
    .score-table-box h3 { margin: 0 0 8px; color: #f57f17; font-size: 1rem; }
    .layout { display: grid; grid-template-columns: 1fr 1fr; gap: 24px; }
    @media (max-width: 900px) { .layout { grid-template-columns: 1fr; } }
    .map-panel { border-radius: 12px; overflow: hidden; box-shadow: 0 2px 12px rgba(0,0,0,0.1); }
    .map-hint { font-size: 0.85rem; color: #888; padding: 8px; text-align: center; }
    .timeline h3 { margin-top: 0; }
    .leg-card { background: white; border: 1px solid #e0e0e0; border-radius: 10px; padding: 16px; margin-bottom: 16px; }
    .leg-header { display: flex; gap: 12px; align-items: center; margin-bottom: 10px; }
    .order { background: #1976d2; color: white; width: 32px; height: 32px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: bold; }
    .region { color: #888; font-size: 0.85rem; margin-right: 8px; }
    .dest-img { width: 100%; max-height: 180px; object-fit: cover; border-radius: 8px; margin: 8px 0; }
    .times { font-size: 0.9rem; color: #444; margin-bottom: 8px; }
    .transit { background: #f5f5f5; padding: 10px; border-radius: 6px; font-size: 0.85rem; }
    .bus-line { margin-bottom: 6px; }
    .narrative { margin-top: 24px; background: #fafafa; padding: 16px; border-radius: 8px; }
    .narrative pre { white-space: pre-wrap; font-family: inherit; margin: 0; }
    .actions { margin-top: 24px; display: flex; gap: 12px; }
    .btn { padding: 10px 20px; border-radius: 8px; text-decoration: none; border: 1px solid #ccc; color: #333; }
    .btn-primary { padding: 10px 20px; border-radius: 8px; text-decoration: none; background: #1976d2; color: white; }
    .loading, .error-page { text-align: center; padding: 60px; color: #666; }
    .error-page { color: #d32f2f; }
  `]
})
export class TripResultComponent implements OnInit {
  itinerary: TripItinerary | null = null;
  scoreTableStats: ScoreTableStats | null = null;
  pipelineTrace: OptimizationStepTrace[] | null = null;
  scoreTableCellTrace: ScoreTableCellTrace[] | null = null;
  loading = true;
  error = '';
  mapCenter = { lat: 31.5, lng: 34.8 };
  mapZoom = 8;
  polylinePath: { lat: number; lng: number }[] = [];
  showMap = false;

  constructor(
    private route: ActivatedRoute,
    private tripService: TripService,
    private tripState: TripStateService
  ) {}

  ngOnInit(): void {
    const tripId = Number(this.route.snapshot.paramMap.get('id'));
    const cached = this.tripState.getOptimizeResult();

    const onData = (data: TripItinerary) => {
      this.itinerary = data;
      this.setupMap();
      this.loadMapsScript();
      this.loading = false;
    };

    if (cached && cached.tripId === tripId && cached.legs?.length) {
      this.scoreTableStats = cached.scoreTableStats ?? null;
      this.pipelineTrace = cached.pipelineTrace ?? null;
      this.scoreTableCellTrace = cached.scoreTableCellTrace ?? null;
      onData(this.optimizeToItinerary(cached));
      return;
    }

    this.tripService.getItinerary(tripId).subscribe({
      next: onData,
      error: () => {
        this.error = 'לא נמצא מסלול לטיול זה. הרץ אופטימיזציה קודם.';
        this.loading = false;
      }
    });
  }

  private loadMapsScript(): void {
    const key = environment.googleMapsApiKey;
    if (!key) return;
    const g = (window as unknown as { google?: { maps?: unknown } }).google;
    if (g?.maps) {
      this.showMap = true;
      return;
    }
    const script = document.createElement('script');
    script.src = `https://maps.googleapis.com/maps/api/js?key=${key}`;
    script.onload = () => { this.showMap = true; };
    document.head.appendChild(script);
  }

  private optimizeToItinerary(result: import('../../models/models').OptimizeResult): TripItinerary {
    return {
      tripId: result.tripId ?? 0,
      tripName: result.tripName || '',
      addressStart: result.addressStart || '',
      destinationCount: result.destinationCount,
      totalScore: result.totalScore,
      timeUsed: result.timeUsed,
      timeAvailable: result.timeAvailable,
      transitEfficiency: result.transitEfficiency,
      narrative: result.narrative,
      legs: result.legs || [],
      mapPoints: result.mapPoints || []
    };
  }

  private setupMap(): void {
    if (!this.itinerary?.mapPoints?.length) return;
    const pts = this.itinerary.mapPoints;
    this.polylinePath = pts.map(p => ({ lat: p.lat, lng: p.lon }));
    const mid = pts[Math.floor(pts.length / 2)];
    this.mapCenter = { lat: mid.lat, lng: mid.lon };
    this.mapZoom = pts.length === 1 ? 10 : 8;
  }

  onImgError(event: Event): void {
    (event.target as HTMLImageElement).style.display = 'none';
  }
}
