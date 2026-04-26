import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map, of } from 'rxjs';
import { isPlatformBrowser } from '@angular/common';

import {
  CreateMembershipSaleDto,
  MembershipSaleDto,
  CreatedMembershipSaleResponse
} from '../models/create-membership-sale.dto';
import { API_BASE_URL } from './api.config';

@Injectable({ providedIn: 'root' })
export class MembershipSalesService {
  private isBrowser: boolean;
  private baseUrl = `${API_BASE_URL}/api/membership-sales`;

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  create(dto: CreateMembershipSaleDto): Observable<string> {
    if (!this.isBrowser) return of('');
    return this.http.post<CreatedMembershipSaleResponse>(this.baseUrl, dto).pipe(
      map(r => r.membershipSaleId)
    );
  }

  getLatest(top = 50): Observable<MembershipSaleDto[]> {
    if (!this.isBrowser) return of([]);
    const params = new HttpParams().set('top', top);
    return this.http.get<MembershipSaleDto[]>(`${this.baseUrl}/latest`, { params });
  }
}
