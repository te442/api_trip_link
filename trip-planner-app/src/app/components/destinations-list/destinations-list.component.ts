import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Destination } from '../../models/models';
import { DestinationService } from '../../services/destination.service';

@Component({
  selector: 'app-destinations-list',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="container">
      <h2>יעדים</h2>
      <div *ngIf="loading" class="loading">טוען...</div>

      <table *ngIf="!loading && destinations.length > 0">
        <thead>
          <tr>
            <th>#</th>
            <th>שם היעד</th>
            <th>אזור</th>
            <th>רמת קושי</th>
            <th>סוג מטייל</th>
            <th>זמן (דקות)</th>
          </tr>
        </thead>
        <tbody>
          <tr *ngFor="let d of destinations">
            <td>{{ d.desId }}</td>
            <td>{{ d.nameDes }}</td>
            <td>{{ d.region }}</td>
            <td>{{ d.levelType }}</td>
            <td>{{ d.travelerType }}</td>
            <td>{{ d.timeDes }}</td>
          </tr>
        </tbody>
      </table>

      <p *ngIf="!loading && destinations.length === 0">אין יעדים.</p>
    </div>
  `,
  styles: [`
    .container { padding: 20px; direction: rtl; }
    table { width: 100%; border-collapse: collapse; margin-top: 16px; }
    th, td { border: 1px solid #ddd; padding: 8px; text-align: right; }
    th { background: #f4f4f4; }
    .loading { color: #666; }
  `]
})
export class DestinationsListComponent implements OnInit {
  destinations: Destination[] = [];
  loading = true;

  constructor(private destService: DestinationService) {}

  ngOnInit(): void {
    this.destService.getAll().subscribe({
      next: data => { this.destinations = data; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }
}
