// src/app/core/models/sale.model.ts

export interface Sale {
  saleId: string;
  storeId: number;
  userId: string;

  saleDate: string;
  subtotal: number;
  tax: number;
  total: number;

  paymentMethod?: string | null;
  notes?: string | null;

  createdAt: string;
  updatedAt?: string | null;
}

export interface SaleCreateDto {
  storeId: number;
  saleDate?: string | null;

  subtotal: number;
  tax: number;
  total: number;

  paymentMethod?: string | null;
  notes?: string | null;
}