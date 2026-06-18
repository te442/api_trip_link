import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TripService } from '../../services/trip.service';
import { LookupService } from '../../services/lookup.service';
import { AuthService } from '../../services/auth.service';
import {
  CreateTrip,
  CategoryItem,
  DifficultyLevel,
  FeatureType
} from '../../models/models';

@Component({
  selector: 'app-trip-create',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="page-wrapper">
      <div class="card">
        <h2>🗺️ יצירת טיול חדש</h2>

        <div *ngIf="loadingLookups" class="loading">טוען נתונים...</div>

        <form *ngIf="!loadingLookups" (ngSubmit)="submit()" #f="ngForm">

          <!-- שם הטיול -->
          <div class="form-group">
            <label>שם הטיול *</label>
            <input type="text" [(ngModel)]="dto.tripName" name="tripName" required placeholder="לדוגמה: טיול דרום קלאסי" />
          </div>

          <!-- אזור -->
          <div class="form-group">
            <label>אזור טיול *</label>
            <select [(ngModel)]="dto.region" name="region" required>
              <option value="">-- בחר אזור --</option>
              <option *ngFor="let r of regions" [value]="r">{{ r }}</option>
            </select>
          </div>

          <!-- תאריך -->
          <div class="form-group">
            <label>תאריך הטיול</label>
            <input type="date" [(ngModel)]="dto.tripDate" name="tripDate" />
          </div>

          <!-- שעת יציאה -->
          <div class="form-row">
            <div class="form-group half">
              <label>שעת יציאה</label>
              <input type="time" [(ngModel)]="dto.startTime" name="startTime" />
            </div>
            <div class="form-group half">
              <label>שעת חזרה</label>
              <input type="time" [(ngModel)]="dto.endTime" name="endTime" />
            </div>
          </div>

          <!-- כתובת התחלה -->
          <div class="form-group">
            <label>כתובת התחלה</label>
            <input type="text" [(ngModel)]="dto.addressStart" name="addressStart" placeholder="לדוגמה: תחנה מרכזית באר שבע" />
          </div>

          <!-- רמת קושי -->
          <div class="form-group">
            <label>רמת קושי</label>
            <select [(ngModel)]="dto.levelId" name="levelId">
              <option [ngValue]="undefined">-- ללא העדפה --</option>
              <option *ngFor="let l of levels" [ngValue]="l.levelId">{{ l.levelType }}</option>
            </select>
          </div>

          <!-- קטגוריות -->
          <div class="form-group">
            <label>קטגוריות (ניתן לבחור מספר)</label>
            <div class="checkbox-grid">
              <label *ngFor="let cat of categories" class="checkbox-item">
                <input type="checkbox"
                       [checked]="isCategorySelected(cat.categoriesId)"
                       (change)="toggleCategory(cat.categoriesId)" />
                {{ cat.categoriesName }}
              </label>
            </div>
          </div>

          <!-- מאפיינים / נגישות -->
          <div class="form-group">
            <label>מאפיינים נדרשים</label>
            <div class="checkbox-grid">
              <label *ngFor="let feat of features" class="checkbox-item">
                <input type="checkbox"
                       [checked]="isFeatureSelected(feat.featureId)"
                       (change)="toggleFeature(feat.featureId)" />
                {{ feat.feature }}
              </label>
            </div>
          </div>

          <!-- כמות יעדים -->
          <div class="form-row">
            <div class="form-group half">
              <label>מינימום יעדים</label>
              <input type="number" [(ngModel)]="dto.minNumDes" name="minNumDes" min="1" placeholder="1" />
            </div>
            <div class="form-group half">
              <label>מקסימום יעדים</label>
              <input type="number" [(ngModel)]="dto.maxNumDes" name="maxNumDes" min="1" placeholder="5" />
            </div>
          </div>

          <!-- כפתורים -->
          <div class="btn-row">
            <button type="submit" class="btn-primary" [disabled]="loading || !dto.tripName || !dto.region">
              {{ loading ? 'יוצר טיול...' : '✅ צור טיול' }}
            </button>
            <a routerLink="/trips" class="btn-secondary">ביטול</a>
          </div>

          <div *ngIf="error" class="error">{{ error }}</div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .page-wrapper {
      min-height: 100vh;
      background: #f0f4f8;
      display: flex;
      justify-content: center;
      align-items: flex-start;
      padding: 32px 16px;
      direction: rtl;
    }
    .card {
      background: white;
      border-radius: 12px;
      box-shadow: 0 4px 20px rgba(0,0,0,0.1);
      padding: 32px;
      width: 100%;
      max-width: 640px;
    }
    h2 {
      margin: 0 0 24px;
      color: #1976d2;
      font-size: 1.5rem;
    }
    .form-group {
      margin-bottom: 16px;
      display: flex;
      flex-direction: column;
    }
    .form-row {
      display: flex;
      gap: 16px;
    }
    .form-group.half {
      flex: 1;
    }
    label {
      font-weight: 600;
      margin-bottom: 6px;
      color: #333;
      font-size: 0.9rem;
    }
    input[type="text"],
    input[type="number"],
    input[type="date"],
    input[type="time"],
    select {
      padding: 10px 12px;
      border: 1px solid #ccc;
      border-radius: 8px;
      font-size: 1rem;
      transition: border-color 0.2s;
      background: #fafafa;
    }
    input:focus, select:focus {
      outline: none;
      border-color: #1976d2;
      background: white;
    }
    .checkbox-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(140px, 1fr));
      gap: 8px;
      margin-top: 4px;
    }
    .checkbox-item {
      display: flex;
      align-items: center;
      gap: 6px;
      font-weight: normal;
      cursor: pointer;
      padding: 6px 10px;
      border: 1px solid #e0e0e0;
      border-radius: 6px;
      transition: background 0.15s;
    }
    .checkbox-item:hover {
      background: #e3f2fd;
    }
    .checkbox-item input[type="checkbox"] {
      width: 16px;
      height: 16px;
      cursor: pointer;
    }
    .btn-row {
      display: flex;
      gap: 12px;
      margin-top: 24px;
      align-items: center;
    }
    .btn-primary {
      background: #1976d2;
      color: white;
      padding: 12px 28px;
      border: none;
      border-radius: 8px;
      font-size: 1rem;
      cursor: pointer;
      transition: background 0.2s;
    }
    .btn-primary:hover:not(:disabled) { background: #1565c0; }
    .btn-primary:disabled { background: #90caf9; cursor: not-allowed; }
    .btn-secondary {
      background: #f5f5f5;
      color: #333;
      padding: 12px 20px;
      border-radius: 8px;
      text-decoration: none;
      font-size: 1rem;
      border: 1px solid #ddd;
    }
    .btn-secondary:hover { background: #e0e0e0; }
    .error {
      margin-top: 16px;
      color: #d32f2f;
      background: #ffebee;
      padding: 10px 14px;
      border-radius: 6px;
    }
    .loading {
      text-align: center;
      color: #666;
      padding: 40px;
    }
  `]
})
export class TripCreateComponent implements OnInit {
  dto: CreateTrip = {
    tripName: '',
    region: '',
    categoryIds: [],
    featureIds: []
  };

  categories: CategoryItem[] = [];
  levels: DifficultyLevel[] = [];
  features: FeatureType[] = [];
  regions: string[] = [];

  loadingLookups = true;
  loading = false;
  error = '';

  constructor(
    private tripService: TripService,
    private lookupService: LookupService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Load all lookup data in parallel
    let pending = 4;
    const done = () => { if (--pending === 0) this.loadingLookups = false; };

    this.lookupService.getCategories().subscribe({ next: d => { this.categories = d; done(); }, error: () => done() });
    this.lookupService.getLevels().subscribe({ next: d => { this.levels = d; done(); }, error: () => done() });
    this.lookupService.getFeatures().subscribe({ next: d => { this.features = d; done(); }, error: () => done() });
    this.lookupService.getRegions().subscribe({ next: d => { this.regions = d; done(); }, error: () => done() });
  }

  isCategorySelected(id: number): boolean {
    return this.dto.categoryIds?.includes(id) ?? false;
  }

  toggleCategory(id: number): void {
    if (!this.dto.categoryIds) this.dto.categoryIds = [];
    const idx = this.dto.categoryIds.indexOf(id);
    if (idx >= 0) this.dto.categoryIds.splice(idx, 1);
    else this.dto.categoryIds.push(id);
  }

  isFeatureSelected(id: number): boolean {
    return this.dto.featureIds?.includes(id) ?? false;
  }

  toggleFeature(id: number): void {
    if (!this.dto.featureIds) this.dto.featureIds = [];
    const idx = this.dto.featureIds.indexOf(id);
    if (idx >= 0) this.dto.featureIds.splice(idx, 1);
    else this.dto.featureIds.push(id);
  }

  submit(): void {
    if (!this.dto.tripName || !this.dto.region) return;
    this.loading = true;
    this.error = '';

    this.dto.userId = this.authService.getCurrentUser()?.userId;
    this.tripService.create(this.dto).subscribe({
      next: trip => {
        this.loading = false;
        this.router.navigate(['/optimizer'], { queryParams: { tripId: trip.tripId } });
      },
      error: err => {
        this.loading = false;
        this.error = 'שגיאה ביצירת הטיול: ' + (err.error?.title || err.message || 'שגיאה לא ידועה');
      }
    });
  }
}
