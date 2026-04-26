import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { isPlatformBrowser } from '@angular/common';

import { CatalogStore } from '../models/catalog-store.model';
import { CatalogMembership } from '../models/catalog-membership.model';
import { API_BASE_URL } from './api.config';

@Injectable({ providedIn: 'root' })
export class CatalogService {
  private isBrowser: boolean;
  private baseUrl = `${API_BASE_URL}/api/catalog`;

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  getStores(): Observable<CatalogStore[]> {
    if (!this.isBrowser) return of([]);
    return this.http.get<CatalogStore[]>(`${this.baseUrl}/stores`);
  }

  getMembershipProducts(): Observable<CatalogMembership[]> {
    if (!this.isBrowser) return of([]);
    return this.http.get<CatalogMembership[]>(`${this.baseUrl}/membership-products`);
  }
}
