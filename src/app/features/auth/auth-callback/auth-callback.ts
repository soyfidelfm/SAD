import { Component, Inject, OnInit, PLATFORM_ID } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService, ExternalProvider } from '../../../core/services/auth.service';

@Component({
  standalone: true,
  imports: [CommonModule],
  template: `<div style="padding:24px">Signing you in...</div>`
})
export class AuthCallbackComponent implements OnInit {
  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private auth: AuthService,
    @Inject(PLATFORM_ID) private platformId: object
  ) {}

  ngOnInit(): void {
    // ✅ Evita ejecutar en SSR (Node) donde no existe sessionStorage
    if (!isPlatformBrowser(this.platformId)) return;

    const providerParam = this.route.snapshot.queryParamMap.get('provider');
    const provider: ExternalProvider =
      providerParam === 'google' || providerParam === 'apple'
        ? providerParam
        : 'microsoft';

    const code = this.route.snapshot.queryParamMap.get('code') ?? '';

    const aNumber = sessionStorage.getItem('aNumber') ?? '';

    if (!code || !aNumber) {
      this.router.navigateByUrl('/login');
      return;
    }

    // this.auth.externalLogin({ provider, aNumber, code }).subscribe({
    //   next: () => this.router.navigateByUrl('/'),
    //   error: () => this.router.navigateByUrl('/login')
    // });
  }
}
