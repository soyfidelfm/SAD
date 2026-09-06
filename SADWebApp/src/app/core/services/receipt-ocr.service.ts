import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { isPlatformBrowser } from '@angular/common';
import { Observable, of } from 'rxjs';
import { API_BASE_URL } from './api.config';

export interface ReceiptOcrResult {
  subTotal: number | null;
  salesTax: number | null;
  balanceTotal: number | null;
  rawText: string;
}

@Injectable({ providedIn: 'root' })
export class ReceiptOcrService {
  private isBrowser: boolean;
  private baseUrl = `${API_BASE_URL}/api/receipt-reader`;

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  readReceipt(file: File): Observable<ReceiptOcrResult> {
    if (!this.isBrowser) {
      return of({
        subTotal: null,
        salesTax: null,
        balanceTotal: null,
        rawText: ''
      });
    }

    const formData = new FormData();
    formData.append('image', file);

    return this.http.post<ReceiptOcrResult>(
      `${this.baseUrl}/read`,
      formData
    );
  }
}