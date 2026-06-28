import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterModule, CommonModule],
  template: `
    <nav *ngIf="auth.isLoggedIn()" class="tl-nav">
      <a routerLink="/my-trips" class="nav-brand">
        <img src="assets/trip-link-logo.png" alt="Trip Link" />
      </a>
      <a routerLink="/my-trips" routerLinkActive="active">הטיולים שלי</a>
      <a routerLink="/plan" routerLinkActive="active">+ תכנן טיול</a>
      <a routerLink="/destinations" routerLinkActive="active">יעדים</a>
      <button class="logout-btn" (click)="auth.logout()">התנתק</button>
    </nav>
    <router-outlet></router-outlet>
  `,
  styles: []
})
export class AppComponent {
  constructor(public auth: AuthService) {}
}
