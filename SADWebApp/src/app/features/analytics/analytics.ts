import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';

import {
  DashboardService,
  AnalyticsSummaryDto,
  DashboardHistoryDto,
  SalesByHour
} from '../../core/services/dashboard.service';

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule,
    MatInputModule
  ],
  templateUrl: './analytics.html',
  styleUrl: './analytics.scss',
})
export class AnalyticsComponent implements OnInit {

  private dashboardService = inject(DashboardService);

  loading = false;

  selectedPeriod = '30days';
  selectedPeriodLabel = '';

  customFrom: Date | null = null;
  customTo: Date | null = null;
  showCustomRange = false;

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
    this.updateSelectedPeriodLabel();
    this.loadAnalytics();
  }

  loadAnalytics(): void {
    this.loading = true;

    const range = this.getDateRange();

    this.dashboardService.getAnalyticsSummary(range.from, range.to).subscribe({
      next: (res) => {
        this.analytics = res;
      },
      error: (err) => {
        console.error('Error loading analytics summary', err);
      }
    });

    this.dashboardService.getHistory(range.from, range.to).subscribe({
      next: (res) => {
        this.history = res;
      },
      error: (err) => {
        console.error('Error loading history', err);
      }
    });

    this.dashboardService.getTodaySalesByHour().subscribe({
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

    if (period === 'custom') {
      this.showCustomRange = true;
      return;
    }

    this.showCustomRange = false;
    this.updateSelectedPeriodLabel();
    this.loadAnalytics();
  }

  applyCustomRange(): void {
    if (!this.customFrom || !this.customTo) {
      return;
    }

    this.selectedPeriodLabel = this.formatDateRange(this.customFrom, this.customTo);
    this.loadAnalytics();
  }

  refresh(): void {
    this.updateSelectedPeriodLabel();
    this.loadAnalytics();
  }

  private getDateRange(): { from: string; to: string } {
    const today = new Date();
    const fromDate = new Date(today);

    if (this.selectedPeriod === 'custom' && this.customFrom && this.customTo) {
      return {
        from: this.toDateString(this.customFrom),
        to: this.toDateString(this.customTo)
      };
    }

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
      to: this.toDateString(today)
    };
  }

  private updateSelectedPeriodLabel(): void {
    const today = new Date();
    const fromDate = new Date(today);

    switch (this.selectedPeriod) {
      case 'today':
        this.selectedPeriodLabel = this.formatPrettyDate(today);
        break;

      case '7days':
        fromDate.setDate(today.getDate() - 7);
        this.selectedPeriodLabel = this.formatDateRange(fromDate, today);
        break;

      case '30days':
        fromDate.setDate(today.getDate() - 30);
        this.selectedPeriodLabel = this.formatDateRange(fromDate, today);
        break;

      case 'month':
        fromDate.setDate(1);
        this.selectedPeriodLabel =
          today.toLocaleString('en-US', {
            month: 'long',
            year: 'numeric'
          });
        break;

      case 'custom':
        if (this.customFrom && this.customTo) {
          this.selectedPeriodLabel = this.formatDateRange(this.customFrom, this.customTo);
        } else {
          this.selectedPeriodLabel = 'Custom Range';
        }
        break;

      default:
        fromDate.setDate(today.getDate() - 30);
        this.selectedPeriodLabel = this.formatDateRange(fromDate, today);
        break;
    }
  }

  private formatPrettyDate(date: Date): string {
    const month = date.toLocaleString('en-US', {
      month: 'long'
    });

    const day = date.getDate();
    const year = date.getFullYear();
    const suffix = this.getDaySuffix(day);

    return `${month} ${day}${suffix} ${year}`;
  }

  private formatDateRange(from: Date, to: Date): string {
    const sameMonth =
      from.getMonth() === to.getMonth() &&
      from.getFullYear() === to.getFullYear();

    if (sameMonth) {
      const month = from.toLocaleString('en-US', {
        month: 'long'
      });

      return `${month} ${from.getDate()}–${to.getDate()}, ${to.getFullYear()}`;
    }

    const fromText = from.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric'
    });

    const toText = to.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });

    return `${fromText} – ${toText}`;
  }

  private getDaySuffix(day: number): string {
    if (day >= 11 && day <= 13) {
      return 'th';
    }

    switch (day % 10) {
      case 1:
        return 'st';

      case 2:
        return 'nd';

      case 3:
        return 'rd';

      default:
        return 'th';
    }
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