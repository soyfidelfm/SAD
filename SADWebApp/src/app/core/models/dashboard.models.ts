// src/app/core/models/dashboard.models.ts

import { Sale } from "./sale.model";

export interface CreditCardApplicationsSummaryDto {
  total: number;
  thisMonth: number;
  today: number;
  approved: number;
  declined: number;
  pending: number;
}

export interface MembershipSalesSummaryDto {
  total: number;
  thisMonth: number;
  today: number;
}

export interface SalesSummaryDto {
  total: number;
  thisMonth: number;
  today: number;
}

export interface DashboardSummaryDto {
  creditCards:CreditCardApplicationsSummaryDto;
  memberships: MembershipSalesSummaryDto;
  sales: SalesSummaryDto;
}
