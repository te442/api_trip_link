import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../environments/environment';
import { Trip, CreateTrip, OptimizeRequest, OptimizeResult, TripItinerary, OptimizationProgress } from '../models/models';
import { normalizeOptimizationProgress, normalizeOptimizeResult } from './api-normalize';

@Injectable({ providedIn: 'root' })
export class TripService {
  private url = `${environment.apiUrl}/trips`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Trip[]> {
    return this.http.get<Trip[]>(this.url);
  }

  getById(id: number): Observable<Trip> {
    return this.http.get<Trip>(`${this.url}/${id}`);
  }

  getByUser(userId: string): Observable<Trip[]> {
    return this.http.get<Trip[]>(`${this.url}/user/${userId}`);
  }

  create(dto: CreateTrip): Observable<Trip> {
    return this.http.post<Trip>(this.url, dto);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.url}/${id}`);
  }

  optimize(request: OptimizeRequest): Observable<OptimizeResult> {
    return this.http.post<Record<string, unknown>>(`${this.url}/optimize`, request).pipe(
      map(raw => normalizeOptimizeResult(raw))
    );
  }

  getOptimizeProgress(traceId: string): Observable<OptimizationProgress> {
    return this.http.get<Record<string, unknown>>(`${this.url}/optimize/progress/${traceId}`).pipe(
      map(raw => normalizeOptimizationProgress(raw))
    );
  }

  saveRoute(tripId: number, destinationIds: number[]): Observable<any> {
    return this.http.post(`${this.url}/${tripId}/save-route`, destinationIds);
  }

  getItinerary(tripId: number): Observable<TripItinerary> {
    return this.http.get<TripItinerary>(`${this.url}/${tripId}/itinerary`);
  }
}
