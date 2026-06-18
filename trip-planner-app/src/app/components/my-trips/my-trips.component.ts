import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Trip } from '../../models/models';
import { TripService } from '../../services/trip.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-my-trips',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="container">
      <h2>הטיולים שלי</h2>
      <p *ngIf="user" class="welcome">שלום, {{ user.fullName }}</p>
      <a routerLink="/plan" class="btn-primary">+ תכנן טיול חדש</a>

      <div *ngIf="loading" class="loading">טוען...</div>

      <table *ngIf="!loading && trips.length > 0">
        <thead>
          <tr>
            <th>#</th>
            <th>שם הטיול</th>
            <th>תאריך</th>
            <th>כתובת התחלה</th>
            <th>פעולות</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let trip of trips">
            <td>{{ trip.tripId }}</td>
            <td>{{ trip.tripName }}</td>
            <td>{{ trip.tripDate | date:'dd/MM/yyyy' }}</td>
            <td>{{ trip.addressStart }}</td>
            <td>
              <a [routerLink]="['/trips', trip.tripId, 'result']" class="btn-sm">צפה במסלול</a>
              <button (click)="delete(trip.tripId)" class="btn-danger-sm">מחק</button>
            </td>
          </tr>
        </tbody>
      </table>

      <p *ngIf="!loading && trips.length === 0">אין טיולים עדיין. התחל לתכנן טיול חדש!</p>
    </div>
  `,
  styles: [`
    .container { padding: 20px; direction: rtl; }
    .welcome { color: #555; margin-bottom: 12px; }
    table { width: 100%; border-collapse: collapse; margin-top: 16px; }
    th, td { border: 1px solid #ddd; padding: 8px; text-align: right; }
    th { background: #f4f4f4; }
    .btn-primary { background: #1976d2; color: white; padding: 8px 16px; border-radius: 4px; text-decoration: none; display: inline-block; }
    .btn-sm { background: #1976d2; color: white; padding: 4px 8px; border-radius: 4px; text-decoration: none; margin-left: 4px; }
    .btn-danger-sm { background: #d32f2f; color: white; padding: 4px 8px; border-radius: 4px; border: none; cursor: pointer; }
    .loading { margin-top: 20px; color: #666; }
  `]
})
export class MyTripsComponent implements OnInit {
  trips: Trip[] = [];
  loading = true;
  user: ReturnType<AuthService['getCurrentUser']> = null;

  constructor(private tripService: TripService, private auth: AuthService) {}

  ngOnInit(): void {
    this.user = this.auth.getCurrentUser();
    const userId = this.user?.userId;
    if (!userId) { this.loading = false; return; }

    this.tripService.getByUser(userId).subscribe({
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
