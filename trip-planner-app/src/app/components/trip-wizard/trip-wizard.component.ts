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
  selector: 'app-trip-wizard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="page-wrapper">
      <div class="card">
        <h2>תכנון טיול חדש</h2>

        <div class="steps">
          <span [class.active]="step === 1">1. פרטים</span>
          <span [class.active]="step === 2">2. העדפות</span>
          <span [class.active]="step === 3">3. זמנים</span>
        </div>

        <div *ngIf="loadingLookups" class="loading">טוען נתונים...</div>

        <div *ngIf="!loadingLookups">
          <!-- שלב 1 -->
          <div *ngIf="step === 1">
            <div class="form-group">
              <label>שם הטיול *</label>
              <input type="text" [(ngModel)]="dto.tripName" required placeholder="לדוגמה: טיול דרום קלאסי" />
            </div>
            <div class="form-group">
              <label>אזור טיול *</label>
              <select [(ngModel)]="dto.region" required>
                <option value="">-- בחר אזור --</option>
                <option *ngFor="let r of regions" [value]="r">{{ r }}</option>
              </select>
            </div>
            <div class="form-group">
              <label>תאריך הטיול</label>
              <input type="date" [(ngModel)]="dto.tripDate" />
            </div>
            <div class="form-group">
              <label>כתובת התחלה</label>
              <input type="text" [(ngModel)]="dto.addressStart" placeholder="תחנה מרכזית באר שבע" />
            </div>
          </div>

          <!-- שלב 2 -->
          <div *ngIf="step === 2">
            <div class="form-group">
              <label>רמת קושי</label>
              <select [(ngModel)]="dto.levelId">
                <option [ngValue]="undefined">-- ללא העדפה --</option>
                <option *ngFor="let l of levels" [ngValue]="l.levelId">{{ l.levelType }}</option>
              </select>
            </div>
            <div class="form-group">
              <label>קטגוריות</label>
              <div class="checkbox-grid">
                <label *ngFor="let cat of categories" class="checkbox-item">
                  <input type="checkbox" [checked]="isCategorySelected(cat.categoriesId)"
                         (change)="toggleCategory(cat.categoriesId)" />
                  {{ cat.categoriesName }}
                </label>
              </div>
            </div>
            <div class="form-group">
              <label>מאפיינים נדרשים</label>
              <div class="checkbox-grid">
                <label *ngFor="let feat of features" class="checkbox-item">
                  <input type="checkbox" [checked]="isFeatureSelected(feat.featureId)"
                         (change)="toggleFeature(feat.featureId)" />
                  {{ feat.feature }}
                </label>
              </div>
            </div>
            <div class="form-row">
              <div class="form-group half">
                <label>מינימום יעדים</label>
                <input type="number" [(ngModel)]="dto.minNumDes" min="1" />
              </div>
              <div class="form-group half">
                <label>מקסימום יעדים</label>
                <input type="number" [(ngModel)]="dto.maxNumDes" min="1" />
              </div>
            </div>
          </div>

          <!-- שלב 3 -->
          <div *ngIf="step === 3">
            <div class="form-row">
              <div class="form-group half">
                <label>שעת יציאה</label>
                <input type="time" [(ngModel)]="dto.startTime" />
              </div>
              <div class="form-group half">
                <label>שעת חזרה</label>
                <input type="time" [(ngModel)]="dto.endTime" />
              </div>
            </div>
            <div class="form-group">
              <label>זמן נסיעה מקסימלי (דקות)</label>
              <input type="number" [(ngModel)]="maxTravelTime" />
            </div>
            <div class="form-group">
              <label>זמן חזרה (דקות)</label>
              <input type="number" [(ngModel)]="returnTravelTime" />
            </div>
            <div class="form-group">
              <label>יעילות תחבורה מינימלית (0-1)</label>
              <input type="number" step="0.01" min="0" max="1" [(ngModel)]="minTransitEfficiency" />
            </div>
          </div>

          <div class="btn-row">
            <button *ngIf="step > 1" type="button" class="btn-secondary" (click)="step = step - 1">הקודם</button>
            <button *ngIf="step < 3" type="button" class="btn-primary" (click)="nextStep()"
                    [disabled]="step === 1 && (!dto.tripName || !dto.region)">הבא</button>
            <button *ngIf="step === 3" type="button" class="btn-primary" (click)="submit()" [disabled]="loading">
              {{ loading ? 'יוצר טיול...' : 'צור טיול והמשך לאופטימיזציה' }}
            </button>
            <a routerLink="/my-trips" class="btn-secondary">ביטול</a>
          </div>
          <div *ngIf="error" class="error">{{ error }}</div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page-wrapper { min-height: 100vh; background: #f0f4f8; display: flex; justify-content: center; padding: 32px 16px; direction: rtl; }
    .card { background: white; border-radius: 12px; box-shadow: 0 4px 20px rgba(0,0,0,0.1); padding: 32px; width: 100%; max-width: 640px; }
    h2 { margin: 0 0 16px; color: #1976d2; }
    .steps { display: flex; gap: 12px; margin-bottom: 24px; }
    .steps span { padding: 6px 14px; border-radius: 20px; background: #e3f2fd; color: #555; font-size: 0.9rem; }
    .steps span.active { background: #1976d2; color: white; }
    .form-group { margin-bottom: 16px; display: flex; flex-direction: column; }
    .form-row { display: flex; gap: 16px; }
    .form-group.half { flex: 1; }
    label { font-weight: 600; margin-bottom: 6px; }
    input, select { padding: 10px; border: 1px solid #ccc; border-radius: 8px; }
    .checkbox-grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(140px, 1fr)); gap: 8px; }
    .checkbox-item { display: flex; align-items: center; gap: 6px; padding: 6px 10px; border: 1px solid #e0e0e0; border-radius: 6px; cursor: pointer; }
    .btn-row { display: flex; gap: 12px; margin-top: 24px; flex-wrap: wrap; }
    .btn-primary { background: #1976d2; color: white; padding: 12px 24px; border: none; border-radius: 8px; cursor: pointer; }
    .btn-primary:disabled { background: #90caf9; }
    .btn-secondary { background: #f5f5f5; color: #333; padding: 12px 20px; border-radius: 8px; text-decoration: none; border: 1px solid #ddd; }
    .error { margin-top: 16px; color: #d32f2f; background: #ffebee; padding: 10px; border-radius: 6px; }
    .loading { text-align: center; padding: 40px; color: #666; }
  `]
})
export class TripWizardComponent implements OnInit {
  step = 1;
  dto: CreateTrip = { tripName: '', region: '', categoryIds: [], featureIds: [], startTime: '08:00', endTime: '18:00' };
  maxTravelTime = 480;
  returnTravelTime = 60;
  minTransitEfficiency = 0.5;

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
    let pending = 4;
    const done = () => { if (--pending === 0) this.loadingLookups = false; };
    this.lookupService.getCategories().subscribe({ next: d => { this.categories = d; done(); }, error: () => done() });
    this.lookupService.getLevels().subscribe({ next: d => { this.levels = d; done(); }, error: () => done() });
    this.lookupService.getFeatures().subscribe({ next: d => { this.features = d; done(); }, error: () => done() });
    this.lookupService.getRegions().subscribe({ next: d => { this.regions = d; done(); }, error: () => done() });
  }

  nextStep(): void { if (this.step < 3) this.step++; }

  isCategorySelected(id: number): boolean { return this.dto.categoryIds?.includes(id) ?? false; }
  toggleCategory(id: number): void {
    if (!this.dto.categoryIds) this.dto.categoryIds = [];
    const idx = this.dto.categoryIds.indexOf(id);
    if (idx >= 0) this.dto.categoryIds.splice(idx, 1); else this.dto.categoryIds.push(id);
  }

  isFeatureSelected(id: number): boolean { return this.dto.featureIds?.includes(id) ?? false; }
  toggleFeature(id: number): void {
    if (!this.dto.featureIds) this.dto.featureIds = [];
    const idx = this.dto.featureIds.indexOf(id);
    if (idx >= 0) this.dto.featureIds.splice(idx, 1); else this.dto.featureIds.push(id);
  }

  submit(): void {
    this.loading = true;
    this.error = '';
    this.dto.userId = this.authService.getCurrentUser()?.userId;

    this.tripService.create(this.dto).subscribe({
      next: trip => {
        this.loading = false;
        this.router.navigate(['/plan/optimize', trip.tripId], {
          queryParams: {
            startTime: this.dto.startTime || '08:00',
            endTime: this.dto.endTime || '18:00',
            maxTravelTime: this.maxTravelTime,
            returnTravelTime: this.returnTravelTime,
            minTransitEfficiency: this.minTransitEfficiency
          }
        });
      },
      error: err => {
        this.loading = false;
        this.error = err.error?.error || err.error?.title || 'שגיאה ביצירת הטיול';
      }
    });
  }
}
