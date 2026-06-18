import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  template: `
    <div class="auth-page">
      <div class="auth-card">
        <h2>התחברות</h2>
        <form (ngSubmit)="submit()">
          <div class="form-group">
            <label>אימייל</label>
            <input type="email" [(ngModel)]="email" name="email" required />
          </div>
          <div class="form-group">
            <label>סיסמה</label>
            <input type="password" [(ngModel)]="password" name="password" required />
          </div>
          <button type="submit" class="btn-primary" [disabled]="loading">
            {{ loading ? 'מתחבר...' : 'התחבר' }}
          </button>
          <p class="link-row">אין לך חשבון? <a routerLink="/register">הירשם כאן</a></p>
          <div *ngIf="error" class="error">{{ error }}</div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .auth-page { min-height: 100vh; display: flex; justify-content: center; align-items: center; background: #f0f4f8; direction: rtl; }
    .auth-card { background: white; padding: 32px; border-radius: 12px; box-shadow: 0 4px 20px rgba(0,0,0,0.1); width: 100%; max-width: 400px; }
    h2 { margin: 0 0 24px; color: #1976d2; }
    .form-group { margin-bottom: 16px; display: flex; flex-direction: column; }
    label { font-weight: 600; margin-bottom: 6px; }
    input { padding: 10px; border: 1px solid #ccc; border-radius: 8px; }
    .btn-primary { width: 100%; background: #1976d2; color: white; padding: 12px; border: none; border-radius: 8px; cursor: pointer; font-size: 1rem; }
    .btn-primary:disabled { background: #90caf9; }
    .link-row { text-align: center; margin-top: 16px; }
    .error { margin-top: 12px; color: #d32f2f; background: #ffebee; padding: 10px; border-radius: 6px; }
  `]
})
export class LoginComponent {
  email    = '';
  password = '';
  loading  = false;
  error    = '';

  constructor(private auth: AuthService, private router: Router) {
    if (auth.isLoggedIn()) router.navigate(['/my-trips']);
  }

  submit(): void {
    this.loading = true;
    this.error   = '';
    this.auth.login({ email: this.email, password: this.password }).subscribe({
      next: () => { this.loading = false; this.router.navigate(['/my-trips']); },
      error: err => {
        this.loading = false;
        this.error   = err.error?.error || 'שגיאה בהתחברות';
      }
    });
  }
}
