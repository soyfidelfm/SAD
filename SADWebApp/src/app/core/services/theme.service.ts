import { Injectable, Inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

export type RetailerTheme =
  | 'sad'
  | 'bestbuy'
  | 'target'
  | 'walmart'
  | 'costco'
  | 'sams'
  | 'aldi';

export type ThemeMode = 'light' | 'dark';

export interface RetailerThemeOption {
  id: RetailerTheme;
  label: string;
  hint: string;
  swatch: string;
}

const STORAGE_RETAILER = 'sad.theme.retailer';
const STORAGE_MODE = 'sad.theme.mode';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  readonly retailers: RetailerThemeOption[] = [
    { id: 'sad', label: 'SAD Blue', hint: 'Default product skin', swatch: '#4083ff' },
    { id: 'bestbuy', label: 'Best Buy inspired', hint: 'Bold blue + yellow energy', swatch: '#0046BE' },
    { id: 'target', label: 'Target inspired', hint: 'Warm retail red', swatch: '#CC0000' },
    { id: 'walmart', label: 'Walmart inspired', hint: 'Spark blue', swatch: '#0071CE' },
    { id: 'costco', label: 'Costco inspired', hint: 'Warehouse navy', swatch: '#005DAA' },
    { id: 'sams', label: "Sam's Club inspired", hint: 'Member teal-blue', swatch: '#0067A0' },
    { id: 'aldi', label: 'Aldi inspired', hint: 'Orange + navy', swatch: '#FF6600' }
  ];

  private readonly isBrowser: boolean;
  retailer: RetailerTheme = 'sad';
  mode: ThemeMode = 'dark';

  constructor(@Inject(PLATFORM_ID) platformId: object) {
    this.isBrowser = isPlatformBrowser(platformId);
  }

  init(): void {
    if (!this.isBrowser) return;

    const storedRetailer = localStorage.getItem(STORAGE_RETAILER) as RetailerTheme | null;
    const storedMode = localStorage.getItem(STORAGE_MODE) as ThemeMode | null;

    if (storedRetailer && this.retailers.some((r) => r.id === storedRetailer)) {
      this.retailer = storedRetailer;
    }

    if (storedMode === 'light' || storedMode === 'dark') {
      this.mode = storedMode;
    } else if (window.matchMedia?.('(prefers-color-scheme: light)').matches) {
      this.mode = 'light';
    }

    this.apply();
  }

  setRetailer(retailer: RetailerTheme): void {
    this.retailer = retailer;
    this.persist();
    this.apply();
  }

  setMode(mode: ThemeMode): void {
    this.mode = mode;
    this.persist();
    this.apply();
  }

  toggleMode(): void {
    this.setMode(this.mode === 'dark' ? 'light' : 'dark');
  }

  currentLabel(): string {
    return this.retailers.find((r) => r.id === this.retailer)?.label ?? 'SAD Blue';
  }

  private persist(): void {
    if (!this.isBrowser) return;
    localStorage.setItem(STORAGE_RETAILER, this.retailer);
    localStorage.setItem(STORAGE_MODE, this.mode);
  }

  private apply(): void {
    if (!this.isBrowser) return;
    const root = document.documentElement;
    root.setAttribute('data-retailer', this.retailer);
    root.setAttribute('data-mode', this.mode);
    root.style.colorScheme = this.mode;
  }
}
