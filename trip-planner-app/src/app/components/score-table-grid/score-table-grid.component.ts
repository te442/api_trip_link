import { Component, Input, OnChanges } from '@angular/core';

import { CommonModule } from '@angular/common';

import { FormsModule } from '@angular/forms';

import { ScoreTableCellTrace } from '../../models/models';



type SortKey = keyof Pick<

  ScoreTableCellTrace,

  'seq' | 'i' | 'j' | 'h' | 'fromLabel' | 'toLabel' | 'departureTime' | 'transitionScore' | 'busTransitHours' | 'walkingHours' | 'transitEfficiency'

>;



@Component({

  selector: 'app-score-table-grid',

  standalone: true,

  imports: [CommonModule, FormsModule],

  template: `

    <div class="grid-panel">

      <div class="toolbar">

        <h3>{{ title }}</h3>

        <span class="count">{{ filteredCells.length }} תאים מוצגים</span>
        <span class="count" *ngIf="httpEstimated">
          בקשות HTTP: {{ httpCompleted || 0 }} / ~{{ httpEstimated }} (הערכה)
        </span>

      </div>



      <div class="filters">

        <input type="search" [(ngModel)]="filterText" (ngModelChange)="applyFilters()" placeholder="סינון לפי מקור, יעד או שעה..." />

        <label class="chk">

          <input type="checkbox" [(ngModel)]="validOnly" (ngModelChange)="applyFilters()" />

          רק תאים תקפים

        </label>

        <button type="button" class="btn-export" (click)="exportCsv()" [disabled]="!filteredCells.length">

          ייצוא CSV

        </button>

      </div>



      <p *ngIf="waiting" class="waiting">ממתין לתאים מ-Google Maps...</p>



      <div class="table-wrap">

        <table>

          <thead>

            <tr>

              <th (click)="sortBy('seq')"># {{ sortMark('seq') }}</th>
              <th (click)="sortBy('i')">i {{ sortMark('i') }}</th>

              <th (click)="sortBy('j')">j {{ sortMark('j') }}</th>

              <th (click)="sortBy('h')">דקה {{ sortMark('h') }}</th>

              <th (click)="sortBy('fromLabel')">מקור {{ sortMark('fromLabel') }}</th>

              <th (click)="sortBy('toLabel')">יעד {{ sortMark('toLabel') }}</th>

              <th (click)="sortBy('departureTime')">שעה {{ sortMark('departureTime') }}</th>

              <th>סוג</th>

              <th>מטמון</th>

              <th>תקף</th>

              <th (click)="sortBy('transitionScore')">מסלולים/ציון {{ sortMark('transitionScore') }}</th>

              <th (click)="sortBy('busTransitHours')">אוטובוס {{ sortMark('busTransitHours') }}</th>

              <th (click)="sortBy('walkingHours')">הליכה {{ sortMark('walkingHours') }}</th>

              <th (click)="sortBy('transitEfficiency')">יעילות {{ sortMark('transitEfficiency') }}</th>

              <th>ישיר</th>

            </tr>

          </thead>

          <tbody>

            <tr *ngFor="let cell of filteredCells" [class.invalid]="!cell.isValid" [class.api-row]="cell.apiKind && cell.apiKind !== 'תא'">

              <td>{{ cell.seq || '—' }}</td>
              <td>{{ cell.i }}</td>

              <td>{{ cell.j }}</td>

              <td>{{ cell.h }}</td>

              <td>{{ cell.fromLabel }}</td>

              <td>{{ cell.toLabel }}</td>

              <td>{{ cell.departureTime }}</td>

              <td>{{ cell.apiKind || 'תא' }}</td>

              <td>{{ cell.fromCache ? 'כן' : '—' }}</td>

              <td>{{ cell.isValid ? 'כן' : 'לא' }}</td>

              <td>{{ cell.transitionScore | number:'1.3-3' }}</td>

              <td>{{ cell.busTransitHours | number:'1.2-2' }}ש</td>

              <td>{{ cell.walkingHours | number:'1.2-2' }}ש</td>

              <td>{{ cell.transitEfficiency | number:'1.2-2' }}</td>

              <td>{{ cell.hasDirectBus ? 'כן' : '—' }}</td>

            </tr>

            <tr *ngIf="!filteredCells.length">

              <td colspan="15" class="empty">אין שורות להצגה — ממתין לקריאות Google Maps...</td>

            </tr>

          </tbody>

        </table>

      </div>

    </div>

  `,

  styles: [`

    .grid-panel { background: #fffde7; border: 1px solid #fff176; border-radius: 8px; padding: 16px; margin: 20px 0; direction: rtl; }

    .toolbar { display: flex; align-items: baseline; gap: 12px; flex-wrap: wrap; margin-bottom: 12px; }

    .toolbar h3 { margin: 0; color: #f57f17; font-size: 1rem; }

    .count { color: #666; font-size: 0.9rem; }

    .filters { display: flex; gap: 12px; align-items: center; flex-wrap: wrap; margin-bottom: 12px; }

    .filters input[type="search"] { flex: 1; min-width: 200px; padding: 8px 10px; border: 1px solid #ddd; border-radius: 6px; }

    .chk { display: flex; align-items: center; gap: 6px; font-size: 0.9rem; color: #444; }

    .btn-export { background: #fff; border: 1px solid #f57f17; color: #e65100; padding: 8px 14px; border-radius: 6px; cursor: pointer; }

    .btn-export:disabled { opacity: 0.5; cursor: default; }

    .waiting { margin: 0 0 12px; color: #e65100; font-size: 0.9rem; }

    .table-wrap { max-height: 520px; overflow: auto; border: 1px solid #eee; border-radius: 6px; background: #fff; }

    table { width: 100%; border-collapse: collapse; font-size: 0.85rem; }

    th, td { padding: 7px 9px; border-bottom: 1px solid #eee; text-align: right; white-space: nowrap; }

    th { position: sticky; top: 0; background: #fff8e1; z-index: 1; cursor: pointer; user-select: none; }

    tr.invalid { background: #ffebee; color: #c62828; }
    tr.api-row { background: #e3f2fd; }

    tr:hover:not(.invalid) { background: #fafafa; }

    .empty { text-align: center; color: #888; padding: 24px !important; }

  `]

})

export class ScoreTableGridComponent implements OnChanges {

  @Input() cells: ScoreTableCellTrace[] = [];

  @Input() title = 'טבלת ציונים תלת-ממדית';
  @Input() httpCompleted?: number;
  @Input() httpEstimated?: number;
  @Input() waiting = false;



  filterText = '';

  validOnly = false;

  sortColumn: SortKey = 'seq';

  sortAsc = true;

  filteredCells: ScoreTableCellTrace[] = [];



  ngOnChanges(): void {

    this.applyFilters();

  }



  sortBy(column: SortKey): void {

    if (this.sortColumn === column) {

      this.sortAsc = !this.sortAsc;

    } else {

      this.sortColumn = column;

      this.sortAsc = true;

    }

    this.applyFilters();

  }



  sortMark(column: SortKey): string {

    if (this.sortColumn !== column) return '';

    return this.sortAsc ? '▲' : '▼';

  }



  exportCsv(): void {

    const header = ['seq', 'i', 'j', 'minute', 'from', 'to', 'departure', 'apiKind', 'fromCache', 'valid', 'scoreOrOptions', 'busHours', 'walkHours', 'efficiency', 'directBus'];

    const rows = this.filteredCells.map(c => [

      c.seq ?? '', c.i, c.j, c.h, c.fromLabel, c.toLabel, c.departureTime,
      c.apiKind ?? '', c.fromCache ? 1 : 0,

      c.isValid ? 1 : 0, c.transitionScore, c.busTransitHours, c.walkingHours,

      c.transitEfficiency, c.hasDirectBus ? 1 : 0

    ]);

    const csv = [header, ...rows]

      .map(row => row.map(v => `"${String(v).replace(/"/g, '""')}"`).join(','))

      .join('\n');

    const blob = new Blob(['\uFEFF' + csv], { type: 'text/csv;charset=utf-8;' });

    const url = URL.createObjectURL(blob);

    const a = document.createElement('a');

    a.href = url;

    a.download = `score-table-${new Date().toISOString().slice(0, 19).replace(/[:T]/g, '-')}.csv`;

    a.click();

    URL.revokeObjectURL(url);

  }



  applyFilters(): void {

    const q = this.filterText.trim().toLowerCase();

    let list = [...this.cells];



    if (this.validOnly) {

      list = list.filter(c => c.isValid);

    }



    if (q) {

      list = list.filter(c =>

        c.fromLabel.toLowerCase().includes(q) ||

        c.toLabel.toLowerCase().includes(q) ||

        c.departureTime.includes(q) ||

        `${c.i},${c.j},${c.h}`.includes(q)

      );

    }



    const dir = this.sortAsc ? 1 : -1;

    list.sort((a, b) => {

      const av = a[this.sortColumn] ?? 0;

      const bv = b[this.sortColumn] ?? 0;

      if (typeof av === 'number' && typeof bv === 'number') return (av - bv) * dir;

      return String(av).localeCompare(String(bv), 'he') * dir;

    });



    this.filteredCells = list;

  }

}

