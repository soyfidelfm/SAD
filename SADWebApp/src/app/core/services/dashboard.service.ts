import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { isPlatformBrowser } from '@angular/common';
import { Observable, of } from 'rxjs';

import { DashboardSummaryDto } from '../models/dashboard.models';
import { LatestTransactionDto } from '../models/latest-transaction.model';
import { API_BASE_URL } from './api.config';
import { I } from '@angular/cdk/keycodes';

export interface SalesByHour {
  hour: number;
  hourLabel: string;
  total: number;
}
export interface SalesByHourByDateDto {
  date: string;        // "2026-05-08"
  hour: number;        // 0-23
  totalSales: number;
}

export interface DashboardHistoryDto {
  date: string;
  totalSales: number;
  creditCards: number;
  memberships: number;
  averageSale: number;
  goalPercent: number;
}

export interface AnalyticsSummaryDto {
  totalSales: number;
  creditCards: number;
  memberships: number;
  averageSale: number;
  goalPercent: number;
  bestDay?: string | null;
  bestHour?: string | null;
  highestTransaction: number;
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private isBrowser: boolean;
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

    const date = this.getTodayLocalDate();
    const timeZone = this.getUserTimeZone();

    return this.http.get<DashboardSummaryDto>(`${this.baseUrl}/summary`, {
      params: {
        date,
        timeZone
      }
    });
  }

  getLatestTransactions(top: number = 10): Observable<LatestTransactionDto[]> {
    if (!this.isBrowser) {
      return of([]);
    }

    const date = this.getTodayLocalDate();
    const timeZone = this.getUserTimeZone();

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

    const date = this.getTodayLocalDate();
    const timeZone = this.getUserTimeZone();

    return this.http.get<SalesByHour[]>(`${this.baseUrl}/today/by-hour`, {
      params: {
        date,
        timeZone
      }
    });
  }

  getSalesByHourByDate(from: string, to: string): Observable<SalesByHourByDateDto[]> {
    if (!this.isBrowser) {
      return of([]);
    }

    const timeZone = this.getUserTimeZone();

    return this.http.get<SalesByHourByDateDto[]>(`${this.baseUrl}/sales/by-hour-by-date`, {
      params: {
        from,
        to,
        timeZone
      }
    });
  }

  getHistory(from: string, to: string): Observable<DashboardHistoryDto[]> {
    if (!this.isBrowser) {
      return of([]);
    }

    const timeZone = this.getUserTimeZone();

    return this.http.get<DashboardHistoryDto[]>(`${this.baseUrl}/history`, {
      params: {
        from,
        to,
        timeZone
      }
    });
  }

  getAnalyticsSummary(from: string, to: string): Observable<AnalyticsSummaryDto> {
    if (!this.isBrowser) {
      return of({
        totalSales: 0,
        creditCards: 0,
        memberships: 0,
        averageSale: 0,
        goalPercent: 0,
        bestDay: null,
        bestHour: null,
        highestTransaction: 0
      });
    }

    const timeZone = this.getUserTimeZone();

    return this.http.get<AnalyticsSummaryDto>(
      `${this.baseUrl}/analytics-summary`,
      {
        params: {
          from,
          to,
          timeZone
        }
      }
    );
  }

  private getTodayLocalDate(): string {
    const today = new Date();

    return (
      today.getFullYear() +
      '-' +
      String(today.getMonth() + 1).padStart(2, '0') +
      '-' +
      String(today.getDate()).padStart(2, '0')
    );
  }

  private getUserTimeZone(): string {
    return Intl.DateTimeFormat().resolvedOptions().timeZone;
  }
}