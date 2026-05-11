import { Component } from '@angular/core';
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
export class SalesGoalCardComponent {
  currentSales = 55153.04;
  goal = 44800.00;

  get percentage(): number {
    return +(this.currentSales / this.goal * 100).toFixed(2);
  }

  get chartPercentage(): number {
    return Math.min(this.percentage, 100);
  }

  chartOptions: Partial<ChartOptions> = {
    series: [100],
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

  constructor() {
    this.chartOptions.series = [this.chartPercentage];
  }
}