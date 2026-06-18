import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { Destination, Station } from '../models/models';

@Injectable({ providedIn: 'root' })
export class DestinationService {
  private url = `${environment.apiUrl}/destinations`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<Destination[]> {
    return this.http.get<Destination[]>(this.url);
  }

  getById(id: number): Observable<Destination> {
    return this.http.get<Destination>(`${this.url}/${id}`);
  }

  getByRegion(region: string): Observable<Destination[]> {
    return this.http.get<Destination[]>(`${this.url}/region/${region}`);
  }

  getByLevel(levelId: number): Observable<Destination[]> {
    return this.http.get<Destination[]>(`${this.url}/level/${levelId}`);
  }

  getStations(desId: number): Observable<Station[]> {
    return this.http.get<Station[]>(`${this.url}/${desId}/stations`);
  }
}
