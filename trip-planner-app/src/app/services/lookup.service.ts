import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CategoryItem, DifficultyLevel, TravelerType, FeatureType } from '../models/models';

@Injectable({ providedIn: 'root' })
export class LookupService {
  private url = `${environment.apiUrl}/lookups`;

  constructor(private http: HttpClient) {}

  getCategories(): Observable<CategoryItem[]> {
    return this.http.get<CategoryItem[]>(`${this.url}/categories`);
  }

  getLevels(): Observable<DifficultyLevel[]> {
    return this.http.get<DifficultyLevel[]>(`${this.url}/levels`);
  }

  getTravelerTypes(): Observable<TravelerType[]> {
    return this.http.get<TravelerType[]>(`${this.url}/traveler-types`);
  }

  getFeatures(): Observable<FeatureType[]> {
    return this.http.get<FeatureType[]>(`${this.url}/features`);
  }

  getRegions(): Observable<string[]> {
    return this.http.get<string[]>(`${this.url}/regions`);
  }
}
