// src/app/core/models/dashboard.models.ts

export interface CreditCardApplicationsSummaryDto {
  total: number;
  today: number;
  approved: number;
  declined: number;
  pending: number;
}

export interface MembershipSalesSummaryDto {
  total: number;
  today: number;
}

export interface TodaySalesSummaryDto {
  todaySalesTotal: number;
}

export interface DashboardSummaryDto {
  creditCards: CreditCardApplicationsSummaryDto;
  memberships: MembershipSalesSummaryDto;
  todaySales: TodaySalesSummaryDto;
}
