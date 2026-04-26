import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { isPlatformBrowser } from '@angular/common';
import { Observable, of } from 'rxjs';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';

export type ExternalProvider = 'microsoft' | 'google' | 'apple';

export interface ExternalLoginRequest {
  provider: ExternalProvider;
  aNumber: string;
  code: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken?: string;
}

export interface MeResponse {
  userId: string | null;
  aNumber: string | null;
  storeId: string | null;
  storeName: string | null;
  role: string | null;
  displayName?: string | null;
  email?: string | null;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private isBrowser: boolean;

  // 🔑 Keys (cámbialos si ya usas otros)
  private readonly ACCESS_TOKEN_KEY = 'accessToken';
  private readonly REFRESH_TOKEN_KEY = 'refreshToken';

  private readonly STORE_ID_KEY = 'storeId';
  private readonly STORE_NAME_KEY = 'storeName';
  private readonly ROLE_KEY = 'role';
  private readonly A_NUMBER_KEY = 'aNumber';

  constructor(
    private http: HttpClient,
    @Inject(PLATFORM_ID) platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  // =========================
  // Token storage
  // =========================
  setAccessToken(token: string): void {
  if (!this.isBrowser) return;

  // ✅ Guardar token
  sessionStorage.setItem(this.ACCESS_TOKEN_KEY, token);

  // ✅ Decodificar y persistir contexto (storeId, storeName, role, aNumber)
  try {
    const payload = this.decodeJwtPayload(token);

    // OJO: ajusta nombres según tus claims reales
    const storeId =
      payload.storeId ?? payload.StoreId ?? payload.store_id ?? payload.sid ?? null;

    const storeName =
      payload.storeName ?? payload.StoreName ?? payload.store_name ?? null;

    const role =
      payload.role ?? payload.Role ?? payload.roles ?? null;

    const aNumber =
      payload.aNumber ?? payload.ANumber ?? payload.anumber ?? payload.anum ?? null;

    if (storeId != null) sessionStorage.setItem(this.STORE_ID_KEY, String(storeId));
    if (storeName != null) sessionStorage.setItem(this.STORE_NAME_KEY, String(storeName));

    // role puede venir como string o array (roles)
    if (role != null) {
      const normalizedRole = Array.isArray(role) ? role[0] : role;
      sessionStorage.setItem(this.ROLE_KEY, String(normalizedRole));
    }

    if (aNumber != null) sessionStorage.setItem(this.A_NUMBER_KEY, String(aNumber));
  } catch {
    // si falla el decode, no truena el login; /me() puede rellenar después
  }
}


  setRefreshToken(token: string): void {
    if (!this.isBrowser) return;
    sessionStorage.setItem(this.REFRESH_TOKEN_KEY, token);
  }

  getAccessToken(): string | null {
    if (!this.isBrowser) return null;
    return sessionStorage.getItem(this.ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    if (!this.isBrowser) return null;
    return sessionStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  isLoggedIn(): boolean {
    return !!this.getAccessToken();
  }

  logout(): void {
    if (!this.isBrowser) return;
    sessionStorage.removeItem(this.ACCESS_TOKEN_KEY);
    sessionStorage.removeItem(this.REFRESH_TOKEN_KEY);

    sessionStorage.removeItem(this.STORE_ID_KEY);
    sessionStorage.removeItem(this.STORE_NAME_KEY);
    sessionStorage.removeItem(this.ROLE_KEY);
    sessionStorage.removeItem(this.A_NUMBER_KEY);
  }

  // =========================
  // /me endpoint
  // =========================
  /**
   * Llama GET /api/auth/me usando el token actual.
   * Nota: si ya tienes interceptor, no necesitas headers manuales.
   */
  me(): Observable<MeResponse> {
    if (!this.isBrowser) {
      return of({
        userId: null,
        aNumber: null,
        storeId: null,
        storeName: null,
        role: null
      });
    }

    const token = this.getAccessToken();
    if (!token) {
      return of({
        userId: null,
        aNumber: null,
        storeId: null,
        storeName: null,
        role: null
      });
    }

    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<MeResponse>(`${environment.apiUrl}/api/auth/me`, { headers }).pipe(
      tap((me) => this.persistMe(me))
    );
  }

  /**
   * Guarda store/role/aNumber (solo para UI/guards).
   * Fuente real: JWT + /me (backend manda).
   */
  private persistMe(me: MeResponse): void {
    if (!this.isBrowser) return;

    if (me.storeId != null) sessionStorage.setItem(this.STORE_ID_KEY, me.storeId);
    if (me.storeName != null) sessionStorage.setItem(this.STORE_NAME_KEY, me.storeName);
    if (me.role != null) sessionStorage.setItem(this.ROLE_KEY, me.role);
    if (me.aNumber != null) sessionStorage.setItem(this.A_NUMBER_KEY, me.aNumber);
  }

  // =========================
  // Session context getters
  // =========================
  getStoreId(): string | null {
    if (!this.isBrowser) return null;
    return sessionStorage.getItem(this.STORE_ID_KEY);
  }

  getStoreName(): string | null {
    if (!this.isBrowser) return null;
    return sessionStorage.getItem(this.STORE_NAME_KEY);
  }

  getRole(): string | null {
    if (!this.isBrowser) return null;
    return sessionStorage.getItem(this.ROLE_KEY);
  }

  getANumber(): string | null {
    if (!this.isBrowser) return null;
    return sessionStorage.getItem(this.A_NUMBER_KEY);
  }

  // =========================
  // Helpers: token payload
  // =========================
  getUserFirstName(): string | null {
    const token = this.getAccessToken();
    if (!token) return null;

    try {
      const payload = this.decodeJwtPayload(token);

      const fullName: string | undefined =
        payload.name || payload.displayName || payload.preferred_username;

      if (!fullName) return null;
      return fullName.split(' ')[0];
    } catch {
      return null;
    }
  }

  /**
   * Si quieres leer cosas del token sin pegarle al backend.
   * OJO: solo para UI, no para seguridad.
   */
  getJwtPayload(): any | null {
    const token = this.getAccessToken();
    if (!token) return null;

    try {
      return this.decodeJwtPayload(token);
    } catch {
      return null;
    }
  }

  private decodeJwtPayload(token: string): any {
  if (!this.isBrowser) throw new Error('Not in browser');

  const base64Url = token.split('.')[1];
  const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');

  const jsonPayload = decodeURIComponent(
    atob(base64)
      .split('')
      .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
      .join('')
  );

  return JSON.parse(jsonPayload);
}


  // =========================
  // Optional: query cleanup hook
  // =========================
  clearQueryTokenCleanup(): void {
    // opcional: si luego quieres limpiar ?token=... del URL, lo haces en tu componente con router.navigate(...)
  }
}
