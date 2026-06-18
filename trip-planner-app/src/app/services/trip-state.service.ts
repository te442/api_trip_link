import { Injectable } from '@angular/core';
import { OptimizeResult } from '../models/models';

@Injectable({ providedIn: 'root' })
export class TripStateService {
  private lastResult: OptimizeResult | null = null;

  setOptimizeResult(result: OptimizeResult): void {
    this.lastResult = result;
    sessionStorage.setItem('lastOptimizeResult', JSON.stringify(result));
  }

  getOptimizeResult(): OptimizeResult | null {
    if (this.lastResult) return this.lastResult;
    const raw = sessionStorage.getItem('lastOptimizeResult');
    if (!raw) return null;
    try {
      this.lastResult = JSON.parse(raw);
      return this.lastResult;
    } catch {
      return null;
    }
  }

  clear(): void {
    this.lastResult = null;
    sessionStorage.removeItem('lastOptimizeResult');
  }
}
