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
    return this.http.post<CreatedResponse>(this.baseUrl, dto).pipe(
      map(r => r.creditCardApplicationId)
    );
  }

  getLatest(top = 50): Observable<CreditCardApplicationDto[]> {
    if (!this.isBrowser) return of([]);
    const params = new HttpParams().set('top', top);
    return this.http.get<CreditCardApplicationDto[]>(`${this.baseUrl}/latest`, { params });
  }
}
