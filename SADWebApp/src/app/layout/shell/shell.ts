import { Component, Inject, PLATFORM_ID, OnInit } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import {
  Router,
  RouterOutlet,
  RouterLink,
  RouterLinkActive,
  ActivatedRoute
} from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

interface NavItem {
  label: string;
  route: string;
  icon: string;
  exact?: boolean;
}

@Component({
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive],
  templateUrl: './shell.html',
  styleUrls: ['./shell.scss']
})
export class ShellComponent implements OnInit {
  sidebarClosed = true;
  firstName: string | null = null;
  private isBrowser: boolean;

  // 🔥 MENÚ DINÁMICO CON FA ICONS
  navItems: NavItem[] = [
    {
      label: 'Dashboard',
      route: '/',
      icon: 'fa-solid fa-chart-line',
      exact: true
    },
    {
      label: 'Analytics',
      route: '/analytics',
      icon: 'fa-solid fa-chart-pie'   
    },
    {
      label: 'Sales',
      route: '/sales',
      icon: 'fa-solid fa-dollar-sign'
    },
    {
      label: 'Stores',
      route: '/stores',
      icon: 'fa-solid fa-store'
    },
    {
      label:'Settings',
      route: '/settings',
      icon: 'fa-solid fa-cog'
    }
  ];

  constructor(
    private auth: AuthService,
    private router: Router,
    private route: ActivatedRoute,
    @Inject(PLATFORM_ID) platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  ngOnInit(): void {
    if (!this.isBrowser) return;

    // Token en query
    const token = this.route.snapshot.queryParamMap.get('token');
    if (token) {
      this.auth.setAccessToken(token);
      this.router.navigate([], { replaceUrl: true, queryParams: {} });
    }

    // Validación login
    if (!this.auth.isLoggedIn()) {
      this.router.navigateByUrl('/login');
      return;
    }

    // Nombre
    this.firstName = this.auth.getUserFirstName();
  }

  toggleSidebar() {
    this.sidebarClosed = !this.sidebarClosed;
  }

  logout() {
    this.auth.logout();
    if (this.isBrowser) {
      this.router.navigateByUrl('/login');
    }
  }
}