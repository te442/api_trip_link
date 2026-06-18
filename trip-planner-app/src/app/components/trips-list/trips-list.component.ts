import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Trip } from '../../models/models';
import { TripService } from '../../services/trip.service';

@Component({
  selector: 'app-trips-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="container">
      <h2>רשימת טיולים</h2>
      <a routerLink="/trips/new" class="btn-primary">+ טיול חדש</a>

      <div *ngIf="loading" class="loading">טוען...</div>

      <table *ngIf="!loading && trips.length > 0">
        <thead>
          <tr>
            <th>#</th>
            <th>שם הטיול</th>
            <th>תאריך</th>
            <th>כתובת התחלה</th>
            <th>עלות</th>
            <th>פעולות</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let trip of trips">
            <td>{{ trip.tripId }}</td>
            <td>{{ trip.tripName }}</td>
            <td>{{ trip.tripDate | date:'dd/MM/yyyy' }}</td>
            <td>{{ trip.addressStart }}</td>
            <td>{{ trip.tripCost | currency:'ILS':'symbol':'1.0-0' }}</td>
            <td>
              <a [routerLink]="['/trips', trip.tripId]" class="btn-sm">פרטים</a>
              <button (click)="delete(trip.tripId)" class="btn-danger-sm">מחק</button>
            </td>
          </tr>
        </tbody>
      </table>

      <p *ngIf="!loading && trips.length === 0">אין טיולים עדיין.</p>
    </div>
  `,
  styles: [`
    .container { padding: 20px; direction: rtl; }
    table { width: 100%; border-collapse: collapse; margin-top: 16px; }
    th, td { border: 1px solid #ddd; padding: 8px; text-align: right; }
    th { background: #f4f4f4; }
    .btn-primary { background: #1976d2; color: white; padding: 8px 16px; border-radius: 4px; text-decoration: none; }
    .btn-sm { background: #1976d2; color: white; padding: 4px 8px; border-radius: 4px; text-decoration: none; margin-left: 4px; }
    .btn-danger-sm { background: #d32f2f; color: white; padding: 4px 8px; border-radius: 4px; border: none; cursor: pointer; }
    .loading { margin-top: 20px; color: #666; }
  `]
})
export class TripsListComponent implements OnInit {
  trips: Trip[] = [];
  loading = true;

  constructor(private tripService: TripService) {}

  ngOnInit(): void {
    this.tripService.getAll().subscribe({
      next: data => { this.trips = data; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }

  delete(id: number): void {
    if (confirm('האם למחוק את הטיול?')) {
      this.tripService.delete(id).subscribe(() => {
        this.trips = this.trips.filter(t => t.tripId !== id);
      });
    }
  }
}
