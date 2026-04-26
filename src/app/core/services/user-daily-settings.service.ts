import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, of } from 'rxjs';
import { isPlatformBrowser } from '@angular/common';

import {
  UserDailySetting,
  CreateUserDailySetting,
  UpdateUserDailySetting
} from '../models/user-daily-setting.model';
import { API_BASE_URL } from './api.config';

@Injectable({ providedIn: 'root' })
export class UserDailySettingsService {
  private isBrowser: boolean;
  private baseUrl = `${API_BASE_URL}/api/user-daily-settings`;

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  getAll(): Observable<UserDailySetting[]> {
    if (!this.isBrowser) return of([]);
    return this.http.get<UserDailySetting[]>(this.baseUrl);
  }

  getById(id: number): Observable<UserDailySetting | null> {
    if (!this.isBrowser) return of(null);
    return this.http.get<UserDailySetting>(`${this.baseUrl}/${id}`);
  }

  getToday(): Observable<UserDailySetting | null> {
    if (!this.isBrowser) return of(null);
    return this.http.get<UserDailySetting>(`${this.baseUrl}/today`);
  }

  create(payload: CreateUserDailySetting): Observable<UserDailySetting> {
    return this.http.post<UserDailySetting>(this.baseUrl, payload);
  }

  update(id: number, payload: UpdateUserDailySetting): Observable<void> {
    return this.http.put<void>(`${this.baseUrl}/${id}`, payload);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }
}