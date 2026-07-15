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
  styles: []
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
