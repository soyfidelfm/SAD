import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { LocalDatePipe } from '../../shared/pipes/local-date-pipe';

import {
  DashboardService,
  AnalyticsSummaryDto,
  DashboardHistoryDto,
  SalesByHour,
  SalesByHourByDateDto
} from '../../core/services/dashboard.service';
import { SalesGoalCardComponent } from '../dashboard/components/sales-goal-card/sales-goal-card';

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    LocalDatePipe,
    SalesGoalCardComponent
  ],
  templateUrl: './analytics.html',
  styleUrl: './analytics.scss',
})
export class AnalyticsComponent implements OnInit {
  private dashboardService = inject(DashboardService);

  hours = Array.from({ length: 11 }, (_, i) => i + 10);
  heatmapRows: {
    date: string;
    total: number;
    hours: { hour: number; totalSales: number; intensity: number }[];
  }[] = [];

  loading = false;

  selectedPeriod = '30days';
  selectedPeriodLabel = '';

  customFrom = '';
  customTo = '';

  analytics: AnalyticsSummaryDto = {
    totalSales: 0,
    creditCards: 0,
    memberships: 0,
    averageSale: 0,
    goalPercent: 0,
    appEfficiency: 0,
    membershipEfficiency: 0,
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

    this.dashboardService.getSalesByHourByDate(range.from, range.to).subscribe({
      next: (res) => {
        this.buildHeatmap(res);
      }
    });
  }

  changePeriod(period: string): void {
    this.selectedPeriod = period;
    this.updateSelectedPeriodLabel();

    if (period === 'custom') {
      if (!this.customFrom || !this.customTo) {
        const today = new Date();
        const from = new Date(today);
        from.setDate(today.getDate() - 7);
        this.customFrom = this.toDateString(from);
        this.customTo = this.toDateString(today);
      }
      return;
    }

    this.loadAnalytics();
  }

  applyCustomRange(): void {
    if (!this.customFrom || !this.customTo) {
      return;
    }

    this.selectedPeriod = 'custom';
    this.selectedPeriodLabel = this.formatDateRange(
      this.parseDate(this.customFrom),
      this.parseDate(this.customTo)
    );

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
        from: this.customFrom,
        to: this.customTo
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
        this.selectedPeriodLabel =
          today.toLocaleString('en-US', {
            month: 'long',
            year: 'numeric'
          });
        break;

      case 'custom':
        if (this.customFrom && this.customTo) {
          this.selectedPeriodLabel = this.formatDateRange(
            this.parseDate(this.customFrom),
            this.parseDate(this.customTo)
          );
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

  buildHeatmap(data: SalesByHourByDateDto[]): void {
    const maxSales = Math.max(...data.map((x) => x.totalSales), 1);

    const grouped = new Map<string, SalesByHourByDateDto[]>();

    data.forEach((item) => {
      if (!grouped.has(item.date)) {
        grouped.set(item.date, []);
      }

      grouped.get(item.date)!.push(item);
    });

    this.heatmapRows = Array.from(grouped.entries())
      .map(([date, items]) => {
        const total = items.reduce((sum, x) => sum + x.totalSales, 0);

        return {
          date,
          total,
          hours: this.hours.map((hour) => {
            const found = items.find((x) => x.hour === hour);
            const totalSales = found?.totalSales ?? 0;

            return {
              hour,
              totalSales,
              intensity: totalSales / maxSales
            };
          })
        };
      })
      .sort((a, b) => b.date.localeCompare(a.date));
  }

  getHeatmapCellStyle(intensity: number): Record<string, string> {
    if (intensity <= 0) {
      return {
        background: 'var(--bg-hover)',
        color: 'var(--text-muted)'
      };
    }

    const pct = Math.round(Math.max(intensity, 0.18) * 100);

    return {
      background: `color-mix(in srgb, var(--brand) ${pct}%, transparent)`,
      color: '#ffffff'
    };
  }

  getEfficiencyClass(value: number): string {
    if (value < 6000) {
      return 'green';
    }

    if (value >= 6000 && value <= 8000) {
      return 'yellow';
    }

    return 'red';
  }

  formatHour(hour: number): string {
    if (hour === 0) return '12 AM';
    if (hour === 12) return '12 PM';
    return hour < 12 ? `${hour} AM` : `${hour - 12} PM`;
  }

  private parseDate(value: string): Date {
    const [y, m, d] = value.split('-').map(Number);
    return new Date(y, m - 1, d);
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
