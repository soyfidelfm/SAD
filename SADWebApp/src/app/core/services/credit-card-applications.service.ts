import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map, of } from 'rxjs';
import { isPlatformBrowser } from '@angular/common';

import {
  CreateCreditCardApplicationDto,
  CreditCardApplicationDto,
  CreatedResponse
} from '../models/credit-card-application.models';

import { API_BASE_URL } from './api.config';

@Injectable({ providedIn: 'root' })
export class CreditCardApplicationsService {
  private isBrowser: boolean;
  private baseUrl = `${API_BASE_URL}/api/credit-card-applications`;

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  create(dto: CreateCreditCardApplicationDto): Observable<string> {
    if (!this.isBrowser) return of('');

    const params = new HttpParams()
      .set('timeZone', this.getTimeZone());

    return this.http
      .post<CreatedResponse>(
        this.baseUrl,
        dto,
        { params }
      )
      .pipe(
        map(r => r.creditCardApplicationId)
      );
  }

  getLatest(top = 50): Observable<CreditCardApplicationDto[]> {
    if (!this.isBrowser) return of([]);

    const params = new HttpParams()
      .set('top', top)
      .set('timeZone', this.getTimeZone());

    return this.http.get<CreditCardApplicationDto[]>(
      `${this.baseUrl}/latest`,
      { params }
    );
  }

  getById(id: string): Observable<CreditCardApplicationDto> {
    if (!this.isBrowser) return of(null as any);

    const params = new HttpParams()
      .set('timeZone', this.getTimeZone());

    return this.http.get<CreditCardApplicationDto>(
      `${this.baseUrl}/${id}`,
      { params }
    );
  }

  delete(id: string): Observable<void> {
    if (!this.isBrowser) return of(void 0);

    const params = new HttpParams()
      .set('timeZone', this.getTimeZone());

    return this.http.delete<void>(
      `${this.baseUrl}/${id}`,
      { params }
    );
  }

  private getTimeZone(): string {
    return Intl.DateTimeFormat()
      .resolvedOptions()
      .timeZone
      .trim();
  }

  private toLocalDateTimeString(d: Date): string {
    const pad = (n: number) =>
      String(n).padStart(2, '0');

    const yyyy = d.getFullYear();
    const mm = pad(d.getMonth() + 1);
    const dd = pad(d.getDate());

    const hh = pad(d.getHours());
    const mi = pad(d.getMinutes());
    const ss = pad(d.getSeconds());

    return `${yyyy}-${mm}-${dd}T${hh}:${mi}:${ss}`;
  }
}