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
    .btn-sm { margin-left: var(--tl-space-xs); }
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
