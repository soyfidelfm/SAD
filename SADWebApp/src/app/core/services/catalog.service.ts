import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { isPlatformBrowser } from '@angular/common';

import { CatalogStore, UpsertStore } from '../models/catalog-store.model';
import { CatalogMembership } from '../models/catalog-membership.model';
import { API_BASE_URL } from './api.config';

@Injectable({ providedIn: 'root' })
export class CatalogService {
  private isBrowser: boolean;
  private baseUrl = `${API_BASE_URL}/api/catalog`;
  private storesUrl = `${API_BASE_URL}/api/stores`;

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

  getAllStores(): Observable<CatalogStore[]> {
    if (!this.isBrowser) return of([]);
    return this.http.get<CatalogStore[]>(this.storesUrl);
  }

  createStore(payload: UpsertStore): Observable<CatalogStore> {
    return this.http.post<CatalogStore>(this.storesUrl, payload);
  }

  updateStore(id: number, payload: UpsertStore): Observable<void> {
    return this.http.put<void>(`${this.storesUrl}/${id}`, payload);
  }

  deactivateStore(id: number): Observable<void> {
    return this.http.delete<void>(`${this.storesUrl}/${id}`);
  }

  getMembershipProducts(): Observable<CatalogMembership[]> {
    if (!this.isBrowser) return of([]);
    return this.http.get<CatalogMembership[]>(`${this.baseUrl}/membership-products`);
  }
}
