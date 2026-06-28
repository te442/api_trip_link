import { Injectable } from '@angular/core';

import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';

import { Observable, catchError, map, of, throwError } from 'rxjs';

import { environment } from '../../environments/environment';



export interface PlaceSuggestion {

  description: string;

  mainText?: string;

  secondaryText?: string;

  placeId?: string;

}



@Injectable({ providedIn: 'root' })

export class PlacesService {

  private readonly url = `${environment.apiUrl}/places/autocomplete`;



  constructor(private http: HttpClient) {}



  autocomplete(input: string): Observable<PlaceSuggestion[]> {

    const q = input?.trim() ?? '';

    if (q.length < 2) return of([]);



    const params = new HttpParams().set('input', q);

    return this.http.get<Record<string, unknown>[]>(this.url, { params }).pipe(

      map(rows => rows.map(row => this.normalize(row))),

      catchError((err: HttpErrorResponse) => {

        const message = err.error?.error

          || err.error?.title

          || 'שגיאה בחיפוש כתובות — ודאי שה-API רץ ושמפתח Google Maps מוגדר';

        console.warn('Places autocomplete failed:', message);

        return throwError(() => new Error(message));

      })

    );

  }



  private normalize(row: Record<string, unknown>): PlaceSuggestion {

    return {

      description: String(row['description'] ?? row['Description'] ?? ''),

      mainText: String(row['mainText'] ?? row['MainText'] ?? row['description'] ?? row['Description'] ?? ''),

      secondaryText: String(row['secondaryText'] ?? row['SecondaryText'] ?? ''),

      placeId: String(row['placeId'] ?? row['PlaceId'] ?? '') || undefined

    };

  }

}

