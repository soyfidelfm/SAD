import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
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
    return this.http.post<Sale>(this.baseUrl, dto);
  }

  getByStore(storeId: number): Observable<Sale[]> {
    if (!this.isBrowser) return of([]);
    return this.http.get<Sale[]>(`${this.baseUrl}/store/${storeId}`);
  }

  // ✅ Rango real: 1 request
  getByStoreAndRange(storeId: number, from: Date, to: Date): Observable<Sale[]> {
    if (!this.isBrowser) return of([]);

    const fromLocal = this.toLocalDateTimeString(from); // sin Z
    const toLocal = this.toLocalDateTimeString(to);     // sin Z

    const url =
      `${this.baseUrl}/range?storeId=${storeId}` +
      `&from=${encodeURIComponent(fromLocal)}` +
      `&to=${encodeURIComponent(toLocal)}`;

    return this.http.get<Sale[]>(url);
  }

  delete(saleId: string): Observable<void> {
    if (!this.isBrowser) return of(void 0);
    return this.http.delete<void>(`${this.baseUrl}/${saleId}`);
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
