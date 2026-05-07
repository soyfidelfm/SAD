import { Routes } from '@angular/router';
import { authGuard, authChildGuard } from './core/guards/auth.guard';
import { loggedOutGuard } from './core/guards/logged-out.guard';

export const routes: Routes = [
  {
    path: 'login',
    canActivate: [loggedOutGuard],
    loadComponent: () =>
      import('./features/auth/login/login').then(m => m.LoginComponent),
  },
  {
    path: '',
    loadComponent: () =>
      import('./layout/shell/shell').then(m => m.ShellComponent),
    canActivate: [authGuard],
    canActivateChild: [authChildGuard],
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () =>
          import('./features/dashboard/dashboard').then(
            m => m.DashboardComponent
          ),
      },
      {
        path: 'analytics',
        loadComponent: () =>
          import('./features/analytics/analytics').then(m => m.AnalyticsComponent),
      },

      // ✅ STORES dentro del layout
      {
        path: 'stores',
        loadComponent: () =>
          import('./features/stores/stores').then(m => m.StoresComponent),
      },

      // ✅ NUEVO: SALES dentro del layout
      {
        path: 'sales',
        loadComponent: () =>
          import('./features/sales/sales').then(m => m.SalesComponent),
      },
      // ✅ NUEVO: SETTINGS dentro del layout
      {
        path: 'settings',
        loadComponent: () =>
          import('./features/settings/settings').then(m => m.SettingsComponent),
      }
    ]
  },

  { path: '**', redirectTo: '' }
];
