import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="auth-page">
      <div class="auth-card">
        <h2>הרשמה</h2>
        <form (ngSubmit)="submit()">
          <div class="form-group">
            <label>שם מלא</label>
            <input type="text" [(ngModel)]="fullName" name="fullName" required />
          </div>
          <div class="form-group">
            <label>אימייל</label>
            <input type="email" [(ngModel)]="email" name="email" required />
          </div>
          <div class="form-group">
            <label>טלפון</label>
            <input type="tel" [(ngModel)]="phone" name="phone" />
          </div>
          <div class="form-group">
            <label>סיסמה</label>
            <input type="password" [(ngModel)]="password" name="password" required minlength="6" />
          </div>
          <button type="submit" class="btn-primary" [disabled]="loading">
            {{ loading ? 'נרשם...' : 'הירשם' }}
          </button>
          <p class="link-row">יש לך חשבון? <a routerLink="/login">התחבר כאן</a></p>
          <div *ngIf="error" class="error">{{ error }}</div>
        </form>
      </div>
    </div>
  `,
  styles: []
})
export class RegisterComponent {
  fullName = '';
  email    = '';
  phone    = '';
  password = '';
  loading  = false;
  error    = '';

  constructor(private auth: AuthService, private router: Router) {
    if (auth.isLoggedIn()) router.navigate(['/my-trips']);
  }

  submit(): void {
    this.loading = true;
    this.error   = '';
    this.auth.register({
      fullName: this.fullName,
      email: this.email,
      phone: this.phone,
      password: this.password
    }).subscribe({
      next: () => { this.loading = false; this.router.navigate(['/my-trips']); },
      error: err => {
        this.loading = false;
        this.error   = err.error?.error || 'שגיאה בהרשמה';
      }
    });
  }
}
