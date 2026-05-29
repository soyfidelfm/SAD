import { Component, ElementRef, HostListener, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { shareReplay } from 'rxjs/operators';

import { AddPopupComponent, AddPopupMode } from '../add-popup/add-popup';

import { CreditCardApplicationsService } from '../../core/services/credit-card-applications.service';
import { CreditCardApplicationDto } from '../../core/models/credit-card-application.models';

import { DashboardService } from '../../core/services/dashboard.service';
import { DashboardSummaryDto } from '../../core/models/dashboard.models';

import { MembershipSalesService } from '../../core/services/membership-sales.service';
import { CreateMembershipSaleDto } from '../../core/models/create-membership-sale.dto';

import { SalesService } from '../../core/services/sales.service';
import { SaleCreateDto } from '../../core/models/sale.model';

import { UserDailySettingsService } from '../../core/services/user-daily-settings.service';
import { LatestTransactionDto } from '../../core/models/latest-transaction.model';

import { LocalDatePipe } from '../../shared/pipes/local-date-pipe';

@Component({
  standalone: true,
  imports: [CommonModule, AddPopupComponent, LocalDatePipe],
  templateUrl: './dashboard.html',
  styleUrls: ['./dashboard.scss']
})
export class DashboardComponent implements OnInit {
  private creditService = inject(CreditCardApplicationsService);
  private dashboardService = inject(DashboardService);
  private membershipSalesService = inject(MembershipSalesService);
  private userDailySettingsService = inject(UserDailySettingsService);
  private salesService = inject(SalesService);

  summary$ = this.dashboardService.getSummary().pipe(shareReplay(1));

  menuOpen = false;
  today = new Date();

  dailyCreditAppGoal = 0;
  dailyMembershipGoal = 0;
  dailySalesGoal = 0;

  errorMessage = '';
  successMessage = '';

  popupOpen = false;
  popupMode: AddPopupMode = 'credit';

  latestTransactions: LatestTransactionDto[] = [];
  loadingLatest = false;
  latestError = '';

  salesByHour: {
    hour: number;
    hourLabel: string;
    total: number;
  }[] = [];

  constructor(private elementRef: ElementRef) {}

  ngOnInit(): void {
    this.refreshSummary();
  }

  toggleMenu(): void {
    this.menuOpen = !this.menuOpen;
  }

  openPopup(mode: AddPopupMode): void {
    this.menuOpen = false;
    this.popupMode = mode;
    this.popupOpen = true;
    this.errorMessage = '';
    this.successMessage = '';
  }

  closePopup(): void {
    this.popupOpen = false;
  }

  onPopupSubmit(e: { mode: AddPopupMode; payload: any }): void {
    if (e.mode === 'credit') {
      this.createCreditCardApplication(e.payload);
      return;
    }

    if (e.mode === 'membership') {
      this.createMembership(e.payload);
      return;
    }

    if (e.mode === 'sale') {
      this.createSale(e.payload);
      return;
    }
  }

  private createCreditCardApplication(payload: {
    storeId: number;
    storeNumber: number;
    approved: boolean;
    creditCardProductId: number;
    statusId?: number;
  }): void {
    this.creditService
      .create({
        statusId: payload.statusId ?? 1,
        creditCardProductId: payload.creditCardProductId ?? 1,
        storeId: payload.storeId,
        storeNumber: payload.storeNumber,
        approved: payload.approved
      })
      .subscribe({
        next: () => {
          this.successMessage = 'Credit card application saved.';
          this.closePopup();
          this.refreshSummary();
        },
        error: (err) => {
          console.error('❌ Error creating credit application', err);
          this.latestError = 'Error creating credit card application.';
          this.errorMessage = 'Error creating credit card application.';
        }
      });
  }

  private createMembership(payload: CreateMembershipSaleDto): void {
    this.membershipSalesService.create(payload).subscribe({
      next: () => {
        this.successMessage = 'Membership sale saved.';
        this.closePopup();
        this.refreshSummary();
      },
      error: (err) => {
        console.error('❌ Error saving membership sale', err);
        this.errorMessage = 'Error saving membership sale.';
      }
    });
  }

  private createSale(payload: {
  storeId: number;

  saleDate?: string | null;

  subtotal: number;
  tax: number;
  total: number;

  notes?: string;
  paymentMethod?: string | null;
}): void {

  const dto: SaleCreateDto = {
    storeId: payload.storeId,

    saleDate: payload.saleDate ?? null,

    subtotal: Number(payload.subtotal),
    tax: Number(payload.tax),
    total: Number(payload.total),

    notes: payload.notes ?? null,
    paymentMethod: payload.paymentMethod ?? null
  };

  console.log('FINAL DTO', dto);

  this.salesService.create(dto).subscribe({
    next: () => {
      this.successMessage = 'Sale saved.';
      this.closePopup();
      this.refreshSummary();
    },
    error: (err) => {
      console.error('❌ Error saving sale', err);
      this.errorMessage = 'Error saving sale.';
    }
  });
}

  refreshSummary(): void {
    this.summary$ = this.dashboardService.getSummary().pipe(shareReplay(1));

    this.loadLatestTransactions();
    this.loadSalesByHour();
    this.loadSettings();
  }

  loadLatestTransactions(): void {
    this.loadingLatest = true;
    this.latestError = '';

    this.dashboardService.getLatestTransactions(5).subscribe({
      next: (data) => {
        this.latestTransactions = data;
        this.loadingLatest = false;
      },
      error: (err) => {
        console.error(err);
        this.latestError = 'Error loading latest transactions.';
        this.loadingLatest = false;
      }
    });
  }

  loadSettings(): void {
    this.userDailySettingsService.getToday().subscribe({
      next: (setting) => {
        if (setting) {
          this.dailyCreditAppGoal = setting.appsGoal ?? 0;
          this.dailyMembershipGoal = setting.membershipsGoal ?? 0;
          this.dailySalesGoal = setting.salesGoalAmount ?? 0;
        }
      },
      error: (err) => {
        console.error('Error loading settings', err);
        this.errorMessage = 'Could not load settings.';
      }
    });
  }

  loadSalesByHour(): void {
    this.dashboardService.getTodaySalesByHour().subscribe({
      next: (data) => {
        this.salesByHour = data;
      },
      error: (err) => {
        console.error('Error loading sales by hour', err);
        this.salesByHour = [];
      }
    });
  }

  getStatusClass(value: number): string {
    if (value === 0) return 'status-red';
    if (value === 1) return 'status-yellow';
    return 'status-green';
  }

  getSalesAmountClass(amount: number): string {
    const dailyTarget = this.dailySalesGoal || 6400;
    const lowThreshold = dailyTarget / 3;

    if (amount >= dailyTarget) return 'status-green';
    if (amount >= lowThreshold) return 'status-yellow';
    return 'status-red';
  }

  getSalesProgressText(amount: number): string {
    if (!amount || amount <= 0) {
      return '0% of daily goal';
    }

    const goal = this.dailySalesGoal || 1;
    const percent = Math.round((amount / goal) * 100);

    if (percent >= 100) {
      return `${percent}% of daily goal · Goal met 🎉`;
    }

    return `${percent}% of daily goal`;
  }

  getSalesPercent(amount: number): number {
    const goal = this.dailySalesGoal || 1;
    const safeAmount = Number(amount ?? 0);

    if (safeAmount <= 0) return 0;

    const pct = Math.round((safeAmount / goal) * 100);
    return Math.max(0, Math.min(100, pct));
  }

  getSalesBarClass(amount: number): string {
    const goal = this.dailySalesGoal || 1;
    const safeAmount = Number(amount ?? 0);

    if (safeAmount >= goal) return 'green';
    if (safeAmount >= goal / 3) return 'yellow';
    return 'red';
  }

  getHourlyBarHeight(total: number): number {
    const max = Math.max(...this.salesByHour.map(x => x.total), 0);

    if (max === 0) {
      return 0;
    }

    return Math.max((total / max) * 100, 8);
  }

  private formatDate(date: Date): string {
    return date.toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric'
    });
  }

  trackById = (_: number, item: CreditCardApplicationDto) =>
    item.creditCardApplicationId;

  @HostListener('document:click', ['$event'])
  onClickOutside(event: MouseEvent): void {
    if (!this.menuOpen) return;

    const clickedInside = this.elementRef.nativeElement.contains(event.target as Node);

    if (!clickedInside) {
      this.menuOpen = false;
    }
  }
}