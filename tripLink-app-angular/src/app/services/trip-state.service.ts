import { Injectable } from '@angular/core';
import { CreateTrip, OptimizeResult } from '../models/models';
import { normalizeOptimizeResult } from './api-normalize';

const STORAGE_PREFIX = 'trip_link_optimize_';
const LEGACY_SESSION_KEY = 'lastOptimizeResult';
const PLAN_DRAFT_KEY = 'trip_link_plan_draft';

export interface TripPlanDraft {
  dto: CreateTrip;
  maxTravelTime: number;
  returnTravelTime: number;
  addressValidated: boolean;
  travelerId?: number;
}

@Injectable({ providedIn: 'root' })
export class TripStateService {
  private readonly memoryByTripId = new Map<number, OptimizeResult>();
  private planDraft: TripPlanDraft | null = null;

  setOptimizeResult(result: OptimizeResult): void {
    const normalized = normalizeOptimizeResult(result as unknown as Record<string, unknown>);
    const tripId = normalized.tripId;
    if (tripId == null || tripId <= 0) {
      console.warn('TripStateService: result missing tripId — not persisted');
      return;
    }

    this.memoryByTripId.set(tripId, normalized);
    localStorage.setItem(this.storageKey(tripId), JSON.stringify(normalized));
    sessionStorage.removeItem(LEGACY_SESSION_KEY);
  }

  getOptimizeResult(tripId: number): OptimizeResult | null {
    if (tripId <= 0) return null;

    const cached = this.memoryByTripId.get(tripId);
    if (cached) return cached;

    const fromLocal = this.readFromStorage(localStorage, tripId);
    if (fromLocal) {
      this.memoryByTripId.set(tripId, fromLocal);
      return fromLocal;
    }

    const legacy = this.readLegacySession(tripId);
    if (legacy) {
      this.setOptimizeResult(legacy);
      return legacy;
    }

    return null;
  }

  hasOptimizeResult(tripId: number): boolean {
    return this.getOptimizeResult(tripId) != null;
  }

  setPlanDraft(draft: TripPlanDraft): void {
    this.planDraft = draft;
    localStorage.setItem(PLAN_DRAFT_KEY, JSON.stringify(draft));
  }

  getPlanDraft(): TripPlanDraft | null {
    if (this.planDraft) return this.planDraft;

    const raw = localStorage.getItem(PLAN_DRAFT_KEY);
    if (!raw) return null;
    try {
      const parsed = JSON.parse(raw) as TripPlanDraft;
      this.planDraft = parsed;
      return parsed;
    } catch {
      localStorage.removeItem(PLAN_DRAFT_KEY);
      return null;
    }
  }

  clearPlanDraft(): void {
    this.planDraft = null;
    localStorage.removeItem(PLAN_DRAFT_KEY);
  }

  clear(tripId?: number): void {
    if (tripId != null && tripId > 0) {
      this.memoryByTripId.delete(tripId);
      localStorage.removeItem(this.storageKey(tripId));
      return;
    }

    this.memoryByTripId.clear();
    const keysToRemove: string[] = [];
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i);
      if (key?.startsWith(STORAGE_PREFIX)) keysToRemove.push(key);
    }
    keysToRemove.forEach(k => localStorage.removeItem(k));
    sessionStorage.removeItem(LEGACY_SESSION_KEY);
  }

  private storageKey(tripId: number): string {
    return `${STORAGE_PREFIX}${tripId}`;
  }

  private readFromStorage(store: Storage, tripId: number): OptimizeResult | null {
    const raw = store.getItem(this.storageKey(tripId));
    if (!raw) return null;
    try {
      return normalizeOptimizeResult(JSON.parse(raw));
    } catch {
      store.removeItem(this.storageKey(tripId));
      return null;
    }
  }

  private readLegacySession(tripId: number): OptimizeResult | null {
    const raw = sessionStorage.getItem(LEGACY_SESSION_KEY);
    if (!raw) return null;
    try {
      const parsed = normalizeOptimizeResult(JSON.parse(raw));
      return parsed.tripId === tripId ? parsed : null;
    } catch {
      return null;
    }
  }
}
