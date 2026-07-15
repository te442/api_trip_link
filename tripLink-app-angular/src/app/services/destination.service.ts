import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { environment } from '../../environments/environment';
import { Destination, Station } from '../models/models';

@Injectable({ providedIn: 'root' })
export class DestinationService {
  private url = `${environment.apiUrl}/destinations`;
  private readonly assetBase = environment.apiUrl.replace(/\/api\/?$/, '');

  constructor(private http: HttpClient) {}

  getAll(): Observable<Destination[]> {
    return this.http.get<unknown[]>(this.url).pipe(
      map(list => list.map(row => this.normalize(row)))
    );
  }

  getById(id: number): Observable<Destination> {
    return this.http.get<unknown>(`${this.url}/${id}`).pipe(
      map(row => this.normalize(row))
    );
  }

  getByRegion(region: string): Observable<Destination[]> {
    return this.http.get<unknown[]>(`${this.url}/region/${region}`).pipe(
      map(list => list.map(row => this.normalize(row)))
    );
  }

  getByLevel(levelId: number): Observable<Destination[]> {
    return this.http.get<unknown[]>(`${this.url}/level/${levelId}`).pipe(
      map(list => list.map(row => this.normalize(row)))
    );
  }

  getStations(desId: number): Observable<Station[]> {
    return this.http.get<Station[]>(`${this.url}/${desId}/stations`);
  }

  private normalize(raw: unknown): Destination {
    const d = (raw ?? {}) as Record<string, unknown>;
    const cats = (d['categories'] ?? d['Categories'] ?? []) as unknown[];
    return {
      desId: Number(d['desId'] ?? d['DesId'] ?? 0),
      nameDes: String(d['nameDes'] ?? d['NameDes'] ?? ''),
      region: String(d['region'] ?? d['Region'] ?? ''),
      levelId: (d['levelId'] ?? d['LevelId']) as number | undefined,
      levelType: (d['levelType'] ?? d['LevelType']) as string | undefined,
      travelerId: (d['travelerId'] ?? d['TravelerId']) as number | undefined,
      travelerType: (d['travelerType'] ?? d['TravelerType']) as string | undefined,
      timeDes: (d['timeDes'] ?? d['TimeDes']) as number | string | undefined,
      imageUrl: this.resolveImageUrl(d),
      categories: cats.map(c => String(c)).filter(Boolean),
    };
  }

  resolveImageUrl(raw: unknown): string | undefined {
    const d = (raw ?? {}) as Record<string, unknown>;
    const path = String(d['imageUrl'] ?? d['ImageUrl'] ?? '');
    if (!path) return undefined;
    if (path.startsWith('http://') || path.startsWith('https://')) return path;
    if (path.startsWith('/')) return `${this.assetBase}${path}`;
    return `${this.assetBase}/${path}`;
  }
}
