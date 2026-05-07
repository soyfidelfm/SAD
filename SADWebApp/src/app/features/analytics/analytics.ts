import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';

import {
  DashboardService,
  AnalyticsSummaryDto,
  DashboardHistoryDto,
  SalesByHour
} from '../../core/services/dashboard.service';

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './analytics.html',
  styleUrl: './analytics.scss',
})
export class AnalyticsComponent implements OnInit {

  private dashboardService = inject(DashboardService);

  loading = false;

  selectedPeriod = '30days';

  analytics: AnalyticsSummaryDto = {
    totalSales: 0,
    creditCards: 0,
    memberships: 0,
    averageSale: 0,
    goalPercent: 0,
    bestDay: null,
    bestHour: null,
    highestTransaction: 0
  };

  history: DashboardHistoryDto[] = [];

  salesByHour: SalesByHour[] = [];

  ngOnInit(): void {
    this.loadAnalytics();
  }

  loadAnalytics(): void {
    this.loading = true;

    const range = this.getDateRange();

    this.dashboardService
      .getAnalyticsSummary(range.from, range.to)
      .subscribe({
        next: (res) => {
          this.analytics = res;
        },
        error: (err) => {
          console.error('Error loading analytics summary', err);
        }
      });

    this.dashboardService
      .getHistory(range.from, range.to)
      .subscribe({
        next: (res) => {
          this.history = res;
        },
        error: (err) => {
          console.error('Error loading history', err);
        }
      });

    this.dashboardService
      .getTodaySalesByHour()
      .subscribe({
        next: (res) => {
          this.salesByHour = res;
          this.loading = false;
        },
        error: (err) => {
          console.error('Error loading hourly sales', err);
          this.loading = false;
        }
      });
  }

  changePeriod(period: string): void {
    this.selectedPeriod = period;
    this.loadAnalytics();
  }

  refresh(): void {
    this.loadAnalytics();
  }

  private getDateRange(): { from: string; to: string } {

    const today = new Date();

    const to = this.toDateString(today);

    const fromDate = new Date(today);

    switch (this.selectedPeriod) {

      case 'today':
        break;

      case '7days':
        fromDate.setDate(today.getDate() - 7);
        break;

      case '30days':
        fromDate.setDate(today.getDate() - 30);
        break;

      case 'month':
        fromDate.setDate(1);
        break;

      default:
        fromDate.setDate(today.getDate() - 30);
        break;
    }

    return {
      from: this.toDateString(fromDate),
      to
    };
  }

  private toDateString(date: Date): string {

    return (
      date.getFullYear() +
      '-' +
      String(date.getMonth() + 1).padStart(2, '0') +
      '-' +
      String(date.getDate()).padStart(2, '0')
    );
  }
}