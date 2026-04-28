import { Component, Inject, PLATFORM_ID, OnInit } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { environment } from '../../../../environments/environment'; // ✅ sin .ts
import { AuthService } from '../../../core/services/auth.service';
import { API_BASE_URL } from '../../../core/services/api.config';

type ExternalProvider = 'microsoft' | 'google' | 'apple';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrls: ['./login.scss']
})
export class LoginComponent implements OnInit {
  aNumber = new FormControl('', { nonNullable: true, validators: [Validators.required] });
  storeNumber = new FormControl('', { nonNullable: true, validators: [Validators.required] });

  private isBrowser: boolean;
  private baseUrl = `${API_BASE_URL}/api/auth`;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private auth: AuthService,
    @Inject(PLATFORM_ID) platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngOnInit(): void {
    if (!this.isBrowser) return;

    // ✅ Backend ahora redirige con: /login?token=...&storeId=...&aNumber=...
    const qp = this.route.snapshot.queryParamMap;

    const token = qp.get('token');
    const storeId = qp.get('storeId');
    const aNumberFromQuery = qp.get('aNumber');

    // Si el usuario refresca o regresa, rellena inputs con lo que ya tenías
    const cachedANumber = sessionStorage.getItem('aNumber');
    const cachedStoreNumber = sessionStorage.getItem('storeNumber');

    if (cachedANumber) this.aNumber.setValue(cachedANumber);
    if (cachedStoreNumber) this.storeNumber.setValue(cachedStoreNumber);

    // ✅ Si viene token, ya estamos logueados
    if (token) {
      this.auth.setAccessToken(token);

      // Guarda storeId en sessionStorage (esto es lo que querías)
      if (storeId) sessionStorage.setItem('storeId', storeId);

      // Guarda aNumber si viene en query (si no viene, conserva el que ya tenías)
      if (aNumberFromQuery) sessionStorage.setItem('aNumber', aNumberFromQuery);

      // Limpia la URL (quita token/storeId/aNumber del querystring)
      this.router.navigateByUrl('/', { replaceUrl: true });
      return;
    }
  }

  loginWith(provider: ExternalProvider): void {
    if (!this.isBrowser) return;

    // 1) Marcar como tocados para mostrar validación
    this.aNumber.markAsTouched();
    this.storeNumber.markAsTouched();

    // 2) Detener si inválido
    if (this.aNumber.invalid || this.storeNumber.invalid) return;

    const aNumber = this.aNumber.value.trim();
    const storeNumber = this.storeNumber.value.trim();

    // 3) Persistir temporalmente (para cuando regrese del callback)
    sessionStorage.setItem('aNumber', aNumber);
    sessionStorage.setItem('storeNumber', storeNumber);

    // 4) URL al backend (start)
    const url =
      `${this.baseUrl}/${provider}/start` +
      `?aNumber=${encodeURIComponent(aNumber)}` +
      `&storeNumber=${encodeURIComponent(storeNumber)}`;

      console.log('API_BASE_URL:', API_BASE_URL);
console.log('baseUrl:', this.baseUrl);
console.log('FINAL URL:', `${this.baseUrl}/${provider}/start`);

    // 5) Redirección
    window.location.assign(url);
  }
}
