import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { TripService } from '../../services/trip.service';
import { LookupService } from '../../services/lookup.service';
import { AuthService } from '../../services/auth.service';
import { TripStateService } from '../../services/trip-state.service';
import {
  CreateTrip,
  CategoryItem,
  DifficultyLevel,
  FeatureType,
  TravelerType
} from '../../models/models';
import { AddressAutocompleteComponent } from '../address-autocomplete/address-autocomplete.component';

@Component({
  selector: 'app-trip-wizard',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, AddressAutocompleteComponent],
  template: `
    <div class="page-wrapper">
      <div class="card">
        <h2>תכנון טיול חדש</h2>

        <div *ngIf="loadingLookups" class="loading">טוען נתונים...</div>

        <div *ngIf="!loadingLookups">
          <div class="form-group">
            <label>שם הטיול</label>
            <input type="text" [(ngModel)]="dto.tripName" placeholder="לדוגמה: טיול דרום קלאסי" />
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
            <label>כתובת התחלה *</label>
            <app-address-autocomplete
              [value]="dto.addressStart || ''"
              (valueChange)="onAddressChange($event)"
              [validated]="addressValidated"
              (validatedChange)="addressValidated = $event"
              [showErrorWhenInvalid]="showAddressError"
              placeholder="התחילי להקליד כתובת..."
            />
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
            <label>רמת קושי</label>
            <select [(ngModel)]="dto.levelId">
              <option [ngValue]="undefined">-- ללא העדפה --</option>
              <option *ngFor="let l of levels" [ngValue]="l.levelId">{{ l.levelType }}</option>
            </select>
          </div>

          <div class="form-group">
            <label>סוג מטייל</label>
            <select [(ngModel)]="travelerId">
              <option [ngValue]="undefined">-- ללא העדפה --</option>
              <option *ngFor="let t of travelerTypes" [ngValue]="t.travelerId">{{ t.typeTravelerName }}</option>
            </select>
          </div>

          <div class="form-group">
            <label>העדפות</label>
            <div class="checkbox-grid">
              <label *ngFor="let feat of features" class="checkbox-item">
                <input type="checkbox" [checked]="isFeatureSelected(feat.featureId)"
                       (change)="toggleFeature(feat.featureId)" />
                {{ feat.feature }}
              </label>
            </div>
          </div>

          <div class="form-group">
            <label>מינימום יעדים</label>
            <input type="number" [(ngModel)]="dto.minNumDes" min="1" />
          </div>

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

          <div class="btn-row">
            <button type="button" class="btn-primary" (click)="submit()"
                    [disabled]="loading || !canSubmit()">
              {{ loading ? 'יוצר טיול...' : 'מעבר לתוצאת המערכת' }}
            </button>
            <a routerLink="/my-trips" class="btn-secondary">ביטול</a>
          </div>
          <div *ngIf="error" class="error">{{ error }}</div>
        </div>
      </div>
    </div>
  `,
  styles: []
})
export class TripWizardComponent implements OnInit, OnDestroy {
  dto: CreateTrip = { tripName: '', region: '', categoryIds: [], featureIds: [], startTime: '08:00', endTime: '18:00' };
  maxTravelTime = 480;
  returnTravelTime = 60;
  travelerId?: number;

  categories: CategoryItem[] = [];
  levels: DifficultyLevel[] = [];
  features: FeatureType[] = [];
  travelerTypes: TravelerType[] = [];
  regions: string[] = [];

  loadingLookups = true;
  loading = false;
  error = '';
  addressValidated = false;
  showAddressError = false;

  constructor(
    private tripService: TripService,
    private lookupService: LookupService,
    private authService: AuthService,
    private tripState: TripStateService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.restoreDraft();

    let pending = 5;
    const done = () => { if (--pending === 0) this.loadingLookups = false; };
    this.lookupService.getCategories().subscribe({ next: d => { this.categories = d; done(); }, error: () => done() });
    this.lookupService.getLevels().subscribe({ next: d => { this.levels = d; done(); }, error: () => done() });
    this.lookupService.getFeatures().subscribe({ next: d => { this.features = d; done(); }, error: () => done() });
    this.lookupService.getRegions().subscribe({ next: d => { this.regions = d; done(); }, error: () => done() });
    this.lookupService.getTravelerTypes().subscribe({ next: d => { this.travelerTypes = d; done(); }, error: () => done() });
  }

  ngOnDestroy(): void {
    if (this.dto.tripName || this.dto.region || this.dto.addressStart) {
      this.saveDraft();
    }
  }

  private restoreDraft(): void {
    const draft = this.tripState.getPlanDraft();
    if (!draft) return;
    this.dto = { ...draft.dto, categoryIds: [...(draft.dto.categoryIds ?? [])], featureIds: [...(draft.dto.featureIds ?? [])] };
    this.maxTravelTime = draft.maxTravelTime;
    this.returnTravelTime = draft.returnTravelTime;
    this.addressValidated = draft.addressValidated;
    this.travelerId = draft.travelerId ?? (draft as { travelerIds?: number[] }).travelerIds?.[0];
  }

  private saveDraft(): void {
    this.tripState.setPlanDraft({
      dto: {
        ...this.dto,
        categoryIds: [...(this.dto.categoryIds ?? [])],
        featureIds: [...(this.dto.featureIds ?? [])]
      },
      maxTravelTime: this.maxTravelTime,
      returnTravelTime: this.returnTravelTime,
      addressValidated: this.addressValidated,
      travelerId: this.travelerId
    });
  }

  onAddressChange(value: string): void {
    this.dto.addressStart = value;
  }

  canSubmit(): boolean {
    return !!this.dto.tripName?.trim()
      && !!this.dto.region
      && !!this.dto.addressStart?.trim()
      && this.addressValidated;
  }

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
    this.showAddressError = true;
    if (!this.canSubmit()) {
      this.error = 'יש לבחור כתובת התחלה מהרשימה לפני יצירת הטיול';
      return;
    }
    this.loading = true;
    this.error = '';
    this.saveDraft();
    this.dto.userId = this.authService.getCurrentUser()?.userId;

    this.tripService.create(this.dto).subscribe({
      next: trip => {
        this.loading = false;
        this.router.navigate(['/plan/optimize', trip.tripId], {
          queryParams: {
            startTime: this.dto.startTime || '08:00',
            endTime: this.dto.endTime || '18:00',
            maxTravelTime: this.maxTravelTime,
            returnTravelTime: this.returnTravelTime
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
