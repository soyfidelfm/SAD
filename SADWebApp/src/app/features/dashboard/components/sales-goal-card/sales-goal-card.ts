import { Component, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgApexchartsModule } from 'ng-apexcharts';

import {
  ApexNonAxisChartSeries,
  ApexChart,
  ApexPlotOptions,
  ApexFill,
  ApexStroke
} from 'ng-apexcharts';

export type ChartOptions = {
  series: ApexNonAxisChartSeries;
  chart: ApexChart;
  plotOptions: ApexPlotOptions;
  fill: ApexFill;
  stroke: ApexStroke;
  labels: string[];
};

@Component({
  selector: 'app-sales-goal-card',
  standalone: true,
  imports: [CommonModule, NgApexchartsModule],
  templateUrl: './sales-goal-card.html',
  styleUrl: './sales-goal-card.scss'
})
export class SalesGoalCardComponent implements OnChanges {

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

  chartOptions: Partial<ChartOptions> = {
    series: [0],
    chart: {
      type: 'radialBar',
      height: 270,
      sparkline: {
        enabled: true
      }
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
          background: 'rgba(80, 180, 255, 0.12)',
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
        shade: 'dark',
        type: 'horizontal',
        gradientToColors: ['#7df9ff'],
        stops: [0, 60, 100]
      }
    },
    stroke: {
      lineCap: 'round'
    },
    labels: ['Progress']
  };

  ngOnChanges(): void {
    this.chartOptions = {
      ...this.chartOptions,
      series: [this.chartPercentage]
    };
  }
}