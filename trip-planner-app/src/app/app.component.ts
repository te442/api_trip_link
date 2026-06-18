import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterModule, CommonModule],
  template: `
    <nav *ngIf="auth.isLoggedIn()">
      <span class="brand">מתכנן טיולים</span>
      <a routerLink="/my-trips" routerLinkActive="active">הטיולים שלי</a>
      <a routerLink="/plan" routerLinkActive="active">+ תכנן טיול</a>
      <a routerLink="/destinations" routerLinkActive="active">יעדים</a>
      <button class="logout-btn" (click)="auth.logout()">התנתק</button>
    </nav>
    <router-outlet></router-outlet>
  `,
  styles: [`
    nav {
      background: #1976d2;
      color: white;
      padding: 12px 24px;
      display: flex;
      align-items: center;
      gap: 20px;
      direction: rtl;
    }
    .brand { font-size: 1.2rem; font-weight: bold; margin-left: auto; }
    nav a { color: white; text-decoration: none; padding: 6px 12px; border-radius: 4px; }
    nav a.active { background: rgba(255,255,255,0.25); }
    nav a:hover { background: rgba(255,255,255,0.15); }
    .logout-btn {
      background: rgba(255,255,255,0.15);
      color: white;
      border: 1px solid rgba(255,255,255,0.4);
      padding: 6px 12px;
      border-radius: 4px;
      cursor: pointer;
    }
    .logout-btn:hover { background: rgba(255,255,255,0.25); }
  `]
})
export class AppComponent {
  constructor(public auth: AuthService) {}
}
