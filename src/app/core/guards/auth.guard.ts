import { inject, PLATFORM_ID } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  CanActivateChildFn,
  CanActivateFn,
  Router,
  RouterStateSnapshot,
  UrlTree
} from '@angular/router';
import { isPlatformBrowser } from '@angular/common';
import { AuthService } from '../services/auth.service';

function checkAuth(_route: ActivatedRouteSnapshot, _state: RouterStateSnapshot): boolean | UrlTree {
  const platformId = inject(PLATFORM_ID);
  const auth = inject(AuthService);
  const router = inject(Router);

  // ✅ Si es SSR/prerender, NO redirijas (en server no hay localStorage).
  if (!isPlatformBrowser(platformId)) return true;

  return auth.isLoggedIn() ? true : router.parseUrl('/login');
}

export const authGuard: CanActivateFn = (route, state) => checkAuth(route, state);
export const authChildGuard: CanActivateChildFn = (childRoute, state) => checkAuth(childRoute, state);
