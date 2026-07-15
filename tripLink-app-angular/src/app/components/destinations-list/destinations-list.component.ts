import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Destination } from '../../models/models';
import { DestinationService } from '../../services/destination.service';
import { DestinationCardComponent } from '../destination-card/destination-card.component';

@Component({
  selector: 'app-destinations-list',
  standalone: true,
  imports: [CommonModule, FormsModule, DestinationCardComponent],
  template: `
    <div class="page">
      <header class="page-header">
        <h2>יעדים</h2>
        <p class="subtitle">לחצי על כרטיס לצפייה בפרטים</p>
      </header>

      <div class="toolbar">
        <input
          type="search"
          class="search"
          placeholder="חיפוש לפי שם, אזור או קטגוריה..."
          [(ngModel)]="searchText"
          (ngModelChange)="applyFilter()" />
        <select class="filter" [(ngModel)]="regionFilter" (ngModelChange)="applyFilter()">
          <option value="">כל האזורים</option>
          <option *ngFor="let r of regions" [value]="r">{{ r }}</option>
        </select>
      </div>

      <div *ngIf="loading" class="loading">טוען יעדים...</div>

      <div *ngIf="!loading && filtered.length > 0" class="cards-grid">
        <app-destination-card
          *ngFor="let d of filtered"
          [destination]="d"
          [selected]="selected?.desId === d.desId"
          (select)="onSelect(d)">
        </app-destination-card>
      </div>

      <p *ngIf="!loading && destinations.length === 0" class="empty">אין יעדים במערכת.</p>
      <p *ngIf="!loading && destinations.length > 0 && filtered.length === 0" class="empty">לא נמצאו יעדים לפי הסינון.</p>

      <aside *ngIf="selected" class="detail-panel">
        <button type="button" class="close-btn" (click)="selected = null" aria-label="סגור">×</button>
        <app-destination-card [destination]="selected" [selected]="true"></app-destination-card>
        <div class="detail-extra">
          <p *ngIf="selected.travelerType"><strong>סוג מטייל:</strong> {{ selected.travelerType }}</p>
          <p *ngIf="selected.levelType"><strong>רמת קושי:</strong> {{ selected.levelType }}</p>
        </div>
      </aside>
    </div>
  `,
  styles: [`
    .page {
      padding: 24px;
      direction: rtl;
      max-width: 1200px;
      margin: 0 auto;
      position: relative;
    }
    .page-header h2 {
      margin: 0;
      color: #1976d2;
      font-size: 1.75rem;
    }
    .subtitle {
      margin: 6px 0 20px;
      color: #666;
    }
    .toolbar {
      display: flex;
      flex-wrap: wrap;
      gap: 12px;
      margin-bottom: 24px;
    }
    .search, .filter {
      padding: 10px 14px;
      border: 1px solid #ccc;
      border-radius: 8px;
      font-size: 1rem;
      background: white;
    }
    .search { flex: 1; min-width: 220px; }
    .filter { min-width: 160px; }
    .cards-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
      gap: 20px;
    }
    .loading, .empty {
      text-align: center;
      padding: 48px 16px;
      color: #666;
    }
    .detail-panel {
      position: fixed;
      top: 80px;
      left: 24px;
      width: min(360px, calc(100vw - 48px));
      z-index: 100;
      background: white;
      border-radius: 14px;
      box-shadow: 0 12px 40px rgba(0, 0, 0, 0.18);
      padding: 12px;
    }
    .close-btn {
      position: absolute;
      top: 8px;
      left: 8px;
      z-index: 2;
      width: 32px;
      height: 32px;
      border: none;
      border-radius: 50%;
      background: rgba(0, 0, 0, 0.5);
      color: white;
      font-size: 1.4rem;
      line-height: 1;
      cursor: pointer;
    }
    .detail-extra {
      padding: 0 8px 8px;
      font-size: 0.9rem;
      color: #444;
    }
    .detail-extra p { margin: 8px 0; }
    @media (max-width: 700px) {
      .detail-panel {
        left: 12px;
        right: 12px;
        width: auto;
      }
    }
  `]
})
export class DestinationsListComponent implements OnInit {
  destinations: Destination[] = [];
  filtered: Destination[] = [];
  regions: string[] = [];
  loading = true;
  searchText = '';
  regionFilter = '';
  selected: Destination | null = null;

  constructor(private destService: DestinationService) {}

  ngOnInit(): void {
    this.destService.getAll().subscribe({
      next: data => {
        this.destinations = data;
        this.regions = [...new Set(data.map(d => d.region).filter(Boolean))].sort();
        this.applyFilter();
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  onSelect(dest: Destination): void {
    this.selected = this.selected?.desId === dest.desId ? null : dest;
  }

  applyFilter(): void {
    const q = this.searchText.trim().toLowerCase();
    this.filtered = this.destinations.filter(d => {
      if (this.regionFilter && d.region !== this.regionFilter) return false;
      if (!q) return true;
      const cats = (d.categories ?? []).join(' ').toLowerCase();
      return d.nameDes.toLowerCase().includes(q)
        || d.region.toLowerCase().includes(q)
        || cats.includes(q);
    });
  }
}
