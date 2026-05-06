import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { isPlatformBrowser } from '@angular/common';
import { Observable, of } from 'rxjs';

import { DashboardSummaryDto } from '../models/dashboard.models';
import { LatestTransactionDto } from '../models/latest-transaction.model';
import { API_BASE_URL } from './api.config';

export interface SalesByHour {
  hour: number;
  hourLabel: string;
  total: number;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private isBrowser: boolean;

  // ✅ Ya no proxy
  private baseUrl = `${API_BASE_URL}/api/dashboard`;

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

getSummary(): Observable<DashboardSummaryDto> {
  if (!this.isBrowser) {
    return of({
      creditCards: {
        total: 0,
        thisMonth: 0,
        today: 0,
        approved: 0,
        declined: 0,
        pending: 0
      },

      memberships: {
        total: 0,
        thisMonth: 0,
        today: 0
      },

      sales: {
        total: 0,
        thisMonth: 0,
        today: 0
      }
    });
  }

  const today = new Date();

  const date =
    today.getFullYear() +
    '-' +
    String(today.getMonth() + 1).padStart(2, '0') +
    '-' +
    String(today.getDate()).padStart(2, '0');

  const timeZone =
    Intl.DateTimeFormat().resolvedOptions().timeZone;

  return this.http.get<DashboardSummaryDto>(
    `${this.baseUrl}/summary`,
    {
      params: {
        date,
        timeZone
      }
    }
  );
}

getLatestTransactions(top: number = 10): Observable<LatestTransactionDto[]> {
  if (!this.isBrowser) {
    return of([]);
  }

  const today = new Date();

  const date =
    today.getFullYear() +
    '-' +
    String(today.getMonth() + 1).padStart(2, '0') +
    '-' +
    String(today.getDate()).padStart(2, '0');

  const timeZone = Intl.DateTimeFormat().resolvedOptions().timeZone;

  return this.http.get<LatestTransactionDto[]>(
    `${this.baseUrl}/latestTransactions`,
    {
      params: {
        top,
        date,
        timeZone
      }
    }
  );
}

  getTodaySalesByHour(): Observable<SalesByHour[]> {
    if (!this.isBrowser) {
      return of([]);
    }

    return this.http.get<SalesByHour[]>(`${this.baseUrl}/today/by-hour`);
  }
}
