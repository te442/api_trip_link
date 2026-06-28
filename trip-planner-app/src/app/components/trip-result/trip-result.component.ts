import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { GoogleMap, MapMarker, MapPolyline } from '@angular/google-maps';
import { TripService } from '../../services/trip.service';
import { TripStateService } from '../../services/trip-state.service';
import { normalizeOptimizeResult } from '../../services/api-normalize';
import { MapPoint, OptimizationStepTrace, ScoreTableCellTrace, ScoreTableStats, TripItinerary, TripLeg } from '../../models/models';
import { environment } from '../../../environments/environment';
import { ScoreTableGridComponent } from '../score-table-grid/score-table-grid.component';

@Component({
  selector: 'app-trip-result',
  standalone: true,
  imports: [CommonModule, RouterModule, GoogleMap, MapMarker, MapPolyline, ScoreTableGridComponent],
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

      <app-score-table-grid
        *ngIf="scoreTableCellTrace?.length"
        [cells]="scoreTableCellTrace ?? []"
        title="טבלת ציונים תלת-ממדית — כל התאים התקפים">
      </app-score-table-grid>

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
            [zoom]="mapZoom"
            (mapInitialized)="fitMapBounds()">
            <map-marker
              *ngFor="let p of itinerary.mapPoints"
              [position]="{ lat: p.lat, lng: p.lon }"
              [label]="markerLabel(p)"
              [title]="p.label">
            </map-marker>
            <map-polyline
              *ngIf="polylinePath.length > 1"
              [path]="polylinePath"
              [options]="routeLineOptions">
            </map-polyline>
          </google-map>
          <div *ngIf="returnLeg" class="map-note">
            הקו במפה כולל חזרה מנקודת הסיום אל נקודת ההתחלה.
          </div>
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
            <img *ngIf="leg.imageUrl" [src]="resolveImageUrl(leg.imageUrl)" [alt]="leg.destinationName" class="dest-img"
                 (error)="onImgError($event)" />
            <div class="times">
              הגעה: {{ leg.arrivalTime }} | עזיבה: {{ leg.departureTime }} | שהייה: {{ leg.stayDuration }}
            </div>
            <div class="transit" *ngIf="leg.transit">
              <div class="transit-header">
                <strong>תחבורה ציבורית</strong>
                <span *ngIf="leg.transit.transitEfficiency != null" class="eff">
                  יעילות {{ leg.transit.transitEfficiency | percent:'1.0-0' }}
                </span>
              </div>
              <p *ngIf="leg.transit.boardingStation" class="station boarding">
                <span class="label">עלייה</span> {{ leg.transit.boardingStation }}
              </p>
              <div *ngFor="let bus of leg.transit.busLines; let idx = index" class="bus-segment">
                <div class="bus-line-title">
                  <span class="line-badge">{{ busLineLabel(bus) }}</span>
                  <span class="vehicle">{{ vehicleLabel(bus.vehicleType) || bus.direction }}</span>
                </div>
                <div class="station-row">
                  <span class="from">{{ bus.fromStation || '—' }}</span>
                  <span class="arrow">→</span>
                  <span class="to">{{ bus.toStation || '—' }}</span>
                </div>
                <div class="time-row" *ngIf="bus.departureTime || bus.arrivalTime">
                  {{ bus.departureTime || '—' }} – {{ bus.arrivalTime || '—' }}
                </div>
              </div>
              <p *ngIf="!leg.transit.busLines?.length" class="no-lines">
                {{ leg.transit.departureTime }} → {{ leg.transit.arrivalTime }}
              </p>
              <p *ngIf="leg.transit.alightingStation" class="station alighting">
                <span class="label">ירידה</span> {{ leg.transit.alightingStation }}
              </p>
              <p *ngIf="leg.transit.walkingMinutes > 0" class="walking">
                הליכה מהתחנה ליעד: {{ leg.transit.walkingMinutes | number:'1.0-0' }} דקות
              </p>
            </div>
          </div>
          <div *ngIf="returnLeg" class="leg-card return-card">
            <div class="leg-header">
              <span class="order return-order">↩</span>
              <div>
                <strong>חזרה לנקודת ההתחלה</strong>
                <span class="region">{{ returnLeg.region }}</span>
              </div>
            </div>
            <div class="times">
              יציאה: {{ returnLeg.transit.departureTime }} | הגעה: {{ returnLeg.transit.arrivalTime }} | יעד חזרה: {{ returnLeg.destinationName }}
            </div>
            <div class="transit" *ngIf="returnLeg.transit">
              <div class="transit-header">
                <strong>תחבורה ציבורית</strong>
                <span *ngIf="returnLeg.transit.transitEfficiency != null" class="eff">
                  יעילות {{ returnLeg.transit.transitEfficiency | percent:'1.0-0' }}
                </span>
              </div>
              <p class="station">
                <span class="label">מוצא</span> {{ returnLeg.transit.fromLabel }}
              </p>
              <p *ngIf="returnLeg.transit.boardingStation" class="station boarding">
                <span class="label">עלייה</span> {{ returnLeg.transit.boardingStation }}
              </p>
              <div *ngFor="let bus of returnLeg.transit.busLines" class="bus-segment">
                <div class="bus-line-title">
                  <span class="line-badge">{{ busLineLabel(bus) }}</span>
                  <span class="vehicle">{{ vehicleLabel(bus.vehicleType) || bus.direction }}</span>
                </div>
                <div class="station-row">
                  <span class="from">{{ bus.fromStation || '—' }}</span>
                  <span class="arrow">→</span>
                  <span class="to">{{ bus.toStation || '—' }}</span>
                </div>
                <div class="time-row" *ngIf="bus.departureTime || bus.arrivalTime">
                  {{ bus.departureTime || '—' }} – {{ bus.arrivalTime || '—' }}
                </div>
              </div>
              <p *ngIf="!returnLeg.transit.busLines?.length" class="no-lines">
                {{ returnLeg.transit.departureTime }} → {{ returnLeg.transit.arrivalTime }}
              </p>
              <p *ngIf="returnLeg.transit.alightingStation" class="station alighting">
                <span class="label">ירידה</span> {{ returnLeg.transit.alightingStation }}
              </p>
              <p *ngIf="returnLeg.transit.walkingMinutes > 0" class="walking">
                הליכה בקטע החזור: {{ returnLeg.transit.walkingMinutes | number:'1.0-0' }} דקות
              </p>
              <p class="no-lines">
                הקטע מסומן במפה מהיעד האחרון אל נקודת ההתחלה.
              </p>
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

    <div class="page" *ngIf="scoreTableCellTrace?.length && !itinerary">
      <app-score-table-grid
        [cells]="scoreTableCellTrace ?? []"
        title="טבלת ציונים תלת-ממדית — כל התאים התקפים">
      </app-score-table-grid>
    </div>
  `,
  styles: [`
    .page { padding: 20px; direction: rtl; max-width: 1200px; margin: 0 auto; }
    .header h2 { margin: 0; color: #1976d2; }
    .sub { color: #666; }
    .stats { display: flex; gap: 20px; margin: 12px 0 20px; }
    .stats span { background: #e3f2fd; padding: 8px 14px; border-radius: 8px; font-weight: 600; }
    .score-table-box { background: #fff8e1; border: 1px solid #ffe082; border-radius: 10px; padding: 14px 18px; margin-bottom: 20px; }
    .pipeline-box { background: #f3f8ff; border: 1px solid #bbdefb; border-radius: 10px; padding: 14px 18px; margin-bottom: 20px; }
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
    .map-note { background: #e8f5e9; color: #2e7d32; font-size: 0.85rem; padding: 8px 12px; }
    .map-hint { font-size: 0.85rem; color: #888; padding: 8px; text-align: center; }
    .timeline h3 { margin-top: 0; }
    .leg-card { background: white; border: 1px solid #e0e0e0; border-radius: 10px; padding: 16px; margin-bottom: 16px; }
    .leg-header { display: flex; gap: 12px; align-items: center; margin-bottom: 10px; }
    .order { background: #1976d2; color: white; width: 32px; height: 32px; border-radius: 50%; display: flex; align-items: center; justify-content: center; font-weight: bold; }
    .return-order { background: #2e7d32; }
    .region { color: #888; font-size: 0.85rem; margin-right: 8px; }
    .dest-img { width: 100%; max-height: 180px; object-fit: cover; border-radius: 8px; margin: 8px 0; }
    .times { font-size: 0.9rem; color: #444; margin-bottom: 8px; }
    .transit { background: #f5f5f5; padding: 12px; border-radius: 8px; font-size: 0.85rem; }
    .transit-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 8px; }
    .transit-header .eff { color: #2e7d32; font-size: 0.8rem; }
    .station { margin: 6px 0; }
    .station .label { display: inline-block; background: #e3f2fd; color: #1565c0; padding: 2px 8px; border-radius: 4px; font-size: 0.75rem; margin-left: 6px; }
    .bus-segment { background: white; border: 1px solid #e0e0e0; border-radius: 6px; padding: 10px; margin: 8px 0; }
    .bus-line-title { display: flex; gap: 8px; align-items: center; margin-bottom: 6px; }
    .line-badge { background: #1976d2; color: white; padding: 2px 10px; border-radius: 12px; font-weight: 600; font-size: 0.8rem; }
    .vehicle { color: #666; font-size: 0.8rem; }
    .station-row { display: flex; gap: 8px; align-items: center; flex-wrap: wrap; }
    .station-row .from, .station-row .to { font-weight: 500; }
    .station-row .arrow { color: #888; }
    .time-row { color: #555; margin-top: 4px; font-size: 0.8rem; }
    .no-lines { color: #666; margin: 4px 0; }
    .walking { color: #666; margin-top: 6px; font-size: 0.8rem; }
    .return-card { border-color: #a5d6a7; background: #f6fff7; }
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
  @ViewChild(GoogleMap) private map?: GoogleMap;

  itinerary: TripItinerary | null = null;
  scoreTableStats: ScoreTableStats | null = null;
  pipelineTrace: OptimizationStepTrace[] | null = null;
  scoreTableCellTrace: ScoreTableCellTrace[] | null = null;
  loading = true;
  error = '';
  mapCenter = { lat: 31.5, lng: 34.8 };
  mapZoom = 8;
  polylinePath: { lat: number; lng: number }[] = [];
  routeLineOptions = {
    strokeColor: '#1976d2',
    strokeOpacity: 0.9,
    strokeWeight: 4
  };
  showMap = false;

  get returnLeg(): TripLeg | null {
    return this.itinerary?.returnLeg ?? null;
  }

  constructor(
    private route: ActivatedRoute,
    private tripService: TripService,
    private tripState: TripStateService
  ) {}

  ngOnInit(): void {
    const tripId = Number(this.route.snapshot.paramMap.get('id'));
    const cached = this.tripState.getOptimizeResult(tripId);

    const onData = (data: TripItinerary) => {
      this.itinerary = data;
      this.setupMap();
      this.loadMapsScript();
      this.loading = false;
    };

    if (cached) {
      const normalized = normalizeOptimizeResult(cached as unknown as Record<string, unknown>);
      this.scoreTableStats = normalized.scoreTableStats ?? null;
      this.pipelineTrace = normalized.pipelineTrace ?? null;
      this.scoreTableCellTrace = normalized.scoreTableCellTrace ?? null;

      if (normalized.legs?.length) {
        onData(this.optimizeToItinerary(normalized));
        return;
      }

      this.error = 'האופטימיזציה הסתיימה אך לא נמצא מסלול תקף. נסי להרחיב את חלון הזמן או להקטין את סף יעילות התחבורה.';
      this.loading = false;
      return;
    }

    // גיבוי: מטמון בשרת (אותה תוצאת optimize) — לא חישוב מחדש
    this.tripService.getItinerary(tripId).subscribe({
      next: onData,
      error: () => {
        this.error = 'לא נמצאה תוצאת אופטימיזציה. הרץ אופטימיזציה קודם באותו דפדפן.';
        this.loading = false;
      }
    });
  }

  private loadMapsScript(): void {
    const key = (environment as { googleMapsApiKey?: string }).googleMapsApiKey;
    if (!key) return;
    const g = (window as unknown as { google?: { maps?: unknown } }).google;
    if (g?.maps) {
      this.showMap = true;
      setTimeout(() => this.fitMapBounds());
      return;
    }
    const script = document.createElement('script');
    script.src = `https://maps.googleapis.com/maps/api/js?key=${key}`;
    script.onload = () => {
      this.showMap = true;
      setTimeout(() => this.fitMapBounds());
    };
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
      returnLeg: result.returnLeg,
      mapPoints: result.mapPoints || []
    };
  }

  private setupMap(): void {
    if (!this.itinerary?.mapPoints?.length) return;
    const pts = this.itinerary.mapPoints;
    this.polylinePath = pts.map(p => ({ lat: p.lat, lng: p.lon }));
    if (pts.length > 1) {
      this.polylinePath = [...this.polylinePath, { lat: pts[0].lat, lng: pts[0].lon }];
    }
    const mid = pts[Math.floor(pts.length / 2)];
    this.mapCenter = { lat: mid.lat, lng: mid.lon };
    this.mapZoom = pts.length === 1 ? 10 : 8;
    setTimeout(() => this.fitMapBounds());
  }

  fitMapBounds(): void {
    const googleMaps = (window as any).google?.maps;
    if (!googleMaps || !this.map?.googleMap || !this.itinerary?.mapPoints?.length) return;

    const bounds = new googleMaps.LatLngBounds();
    this.itinerary.mapPoints.forEach(p => bounds.extend({ lat: p.lat, lng: p.lon }));
    this.map.googleMap.fitBounds(bounds);
  }

  markerLabel(point: MapPoint): string {
    return point.order === 0 ? 'בית' : point.order.toString();
  }

  onImgError(event: Event): void {
    (event.target as HTMLImageElement).style.display = 'none';
  }

  resolveImageUrl(path?: string): string {
    if (!path) return '';
    if (path.startsWith('http://') || path.startsWith('https://')) return path;
    const base = environment.apiUrl.replace(/\/api\/?$/, '');
    return path.startsWith('/') ? `${base}${path}` : `${base}/${path}`;
  }

  busLineLabel(bus: import('../../models/models').BusLine): string {
    if (bus.busNumber && bus.busNumber !== '—') return `קו ${bus.busNumber}`;
    return bus.direction || 'תחבורה';
  }

  vehicleLabel(type?: string): string {
    const map: Record<string, string> = {
      BUS: 'אוטובוס',
      INTERCITY_BUS: 'אוטובוס בינעירוני',
      SUBWAY: 'רכבת תחתית',
      TRAIN: 'רכבת',
      TRAM: 'רכבת קלה',
      RAIL: 'רכבת',
      FERRY: 'מעבורת'
    };
    return type ? (map[type] ?? type) : '';
  }
}
