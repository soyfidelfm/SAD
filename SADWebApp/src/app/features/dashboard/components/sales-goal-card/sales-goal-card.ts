import { Component, Input, OnChanges, OnDestroy, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgApexchartsModule } from 'ng-apexcharts';
import { Subscription } from 'rxjs';

import {
  ApexNonAxisChartSeries,
  ApexChart,
  ApexPlotOptions,
  ApexFill,
  ApexStroke
} from 'ng-apexcharts';

import { ThemeService } from '../../../../core/services/theme.service';

export type ChartOptions = {
  series: ApexNonAxisChartSeries;
  chart: ApexChart;
  plotOptions: ApexPlotOptions;
  fill: ApexFill;
  stroke: ApexStroke;
  labels: string[];
  colors: string[];
};

@Component({
  selector: 'app-sales-goal-card',
  standalone: true,
  imports: [CommonModule, NgApexchartsModule],
  templateUrl: './sales-goal-card.html',
  styleUrl: './sales-goal-card.scss'
})
export class SalesGoalCardComponent implements OnInit, OnChanges, OnDestroy {
  private theme = inject(ThemeService);
  private themeSub?: Subscription;

  @Input() currentSales: number = 0;
  @Input() goal: number = 0;
  @Input() percentage: number = 0;

  get safePercentage(): number {
    return Number(this.percentage || 0);
  }

  get chartPercentage(): number {
    return Math.min(this.safePercentage, 100);
  }

  get isAboveGoal(): boolean {
    return this.safePercentage > 100;
  }

  get aboveGoalText(): string {
    return `+${(this.safePercentage - 100).toFixed(2)}% Above Goal`;
  }

  chartOptions: Partial<ChartOptions> = this.buildChartOptions();

  ngOnInit(): void {
    this.themeSub = this.theme.changes$.subscribe(() => this.refreshChart());
  }

  ngOnChanges(): void {
    this.refreshChart();
  }

  ngOnDestroy(): void {
    this.themeSub?.unsubscribe();
  }

  private refreshChart(): void {
    this.chartOptions = this.buildChartOptions();
  }

  private buildChartOptions(): Partial<ChartOptions> {
    const option = this.theme.currentOption();
    const isDark = this.theme.mode === 'dark';

    return {
      series: [this.chartPercentage],
      colors: [option.swatch],
      chart: {
        type: 'radialBar',
        height: 270,
        sparkline: {
          enabled: true
        },
        background: 'transparent',
        foreColor: isDark ? '#e9eefc' : '#0f172a'
      },
      plotOptions: {
        radialBar: {
          startAngle: -270,
          endAngle: 90,
          hollow: {
            size: '68%',
            background: 'transparent'
          },
          track: {
            background: isDark ? 'rgba(255,255,255,0.08)' : 'rgba(15,23,42,0.08)',
            strokeWidth: '90%'
          },
          dataLabels: {
            show: false
          }
        }
      },
      fill: {
        type: 'gradient',
        gradient: {
          shade: isDark ? 'dark' : 'light',
          type: 'horizontal',
          colorStops: [
            { offset: 0, color: option.swatch, opacity: 1 },
            { offset: 100, color: option.swatchEnd, opacity: 1 }
          ]
        }
      },
      stroke: {
        lineCap: 'round'
      },
      labels: ['Progress']
    };
  }
}
