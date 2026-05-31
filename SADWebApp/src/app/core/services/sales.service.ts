import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { isPlatformBrowser } from '@angular/common';
import { Observable, of } from 'rxjs';

import { Sale, SaleCreateDto } from '../models/sale.model';
import { API_BASE_URL } from './api.config';

@Injectable({ providedIn: 'root' })
export class SalesService {
  private isBrowser: boolean;
  private baseUrl = `${API_BASE_URL}/api/sales`;

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  create(dto: SaleCreateDto): Observable<Sale> {
    if (!this.isBrowser) return of(null as any);

    const params = new HttpParams()
      .set('timeZone', this.getTimeZone());

    return this.http.post<Sale>(this.baseUrl, dto, { params });
  }

  getByStore(storeId: number): Observable<Sale[]> {
    if (!this.isBrowser) return of([]);

    const params = new HttpParams()
      .set('timeZone', this.getTimeZone());

    return this.http.get<Sale[]>(
      `${this.baseUrl}/store/${storeId}`,
      { params }
    );
  }

  getByStoreAndRange(
    storeId: number,
    from: Date,
    to: Date
  ): Observable<Sale[]> {
    if (!this.isBrowser) return of([]);

    const params = new HttpParams()
      .set('storeId', storeId)
      .set('from', this.toLocalDateTimeString(from))
      .set('to', this.toLocalDateTimeString(to))
      .set('timeZone', this.getTimeZone());

    return this.http.get<Sale[]>(
      `${this.baseUrl}/range`,
      { params }
    );
  }

  getLatest(top: number = 10): Observable<Sale[]> {
    if (!this.isBrowser) return of([]);

    const params = new HttpParams()
      .set('top', top)
      .set('timeZone', this.getTimeZone());

    return this.http.get<Sale[]>(
      `${this.baseUrl}/latest`,
      { params }
    );
  }

  getById(saleId: string): Observable<Sale> {
    if (!this.isBrowser) return of(null as any);

    const params = new HttpParams()
      .set('timeZone', this.getTimeZone());

    return this.http.get<Sale>(
      `${this.baseUrl}/${saleId}`,
      { params }
    );
  }

  delete(saleId: string): Observable<void> {
    if (!this.isBrowser) return of(void 0);
    return this.http.delete<void>(`${this.baseUrl}/${saleId}`);
  }

  private getTimeZone(): string {
    return Intl.DateTimeFormat()
      .resolvedOptions()
      .timeZone
      .trim();
  }

  private toLocalDateTimeString(d: Date): string {
    const pad = (n: number) => String(n).padStart(2, '0');

    const yyyy = d.getFullYear();
    const mm = pad(d.getMonth() + 1);
    const dd = pad(d.getDate());
    const hh = pad(d.getHours());
    const mi = pad(d.getMinutes());
    const ss = pad(d.getSeconds());

    return `${yyyy}-${mm}-${dd}T${hh}:${mi}:${ss}`;
  }
}