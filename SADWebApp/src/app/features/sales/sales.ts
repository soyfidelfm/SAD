import { Component, Inject, PLATFORM_ID, afterNextRender, OnDestroy } from '@angular/core';
import { CommonModule, isPlatformBrowser } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';

import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

import { SalesService } from '../../core/services/sales.service';
import { Sale } from '../../core/models/sale.model';

import { LocalDatePipe } from '../../shared/pipes/local-date-pipe'
@Component({
  selector: 'app-sales',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    LocalDatePipe
  ],
  templateUrl: './sales.html',
  styleUrls: ['./sales.scss']
})
export class SalesComponent implements OnDestroy {
  private isBrowser: boolean;

  storeId!: number;
  storeName: string | null = null;

  form: FormGroup;
  loading = false;
  deletingId: string | null = null;

  dataSource = new MatTableDataSource<Sale>([]);

  displayedColumnsDesktop: string[] = [
    'saleDate',
    'subtotal',
    'tax',
    'total',
    'paymentMethod',
    'notes',
    'actions'
  ];

  displayedColumnsMobile: string[] = [
    'saleDate',
    'total'
  ];

  isMobile = false;

  get displayedColumns(): string[] {
    return this.isMobile ? this.displayedColumnsMobile : this.displayedColumnsDesktop;
  }

  private mq?: MediaQueryList;
  private mqHandler?: (e: MediaQueryListEvent) => void;

  constructor(
    private fb: FormBuilder,
    private salesService: SalesService,
    @Inject(PLATFORM_ID) platformId: object
  ) {
    this.isBrowser = isPlatformBrowser(platformId);

    const to = new Date();
    const from = new Date(to);
    from.setDate(to.getDate() - 30);

    this.form = this.fb.group({
      fromDateTime: [this.toLocalInput(from), Validators.required],
      toDateTime: [this.toLocalInput(to), Validators.required]
    });

    if (this.isBrowser) {
      this.mq = window.matchMedia('(max-width: 640px)');
      this.isMobile = this.mq.matches;

      this.mqHandler = (e: MediaQueryListEvent) => {
        this.isMobile = e.matches;
      };

      this.mq.addEventListener('change', this.mqHandler);

      afterNextRender(() => {
        const storedStoreId = sessionStorage.getItem('storeId');
        const storedStoreName = sessionStorage.getItem('storeName');

        if (!storedStoreId) {
          console.error('StoreId not found in sessionStorage');
          window.location.href = '/login';
          return;
        }

        this.storeId = Number(storedStoreId);

        if (Number.isNaN(this.storeId)) {
          console.error('Invalid storeId value:', storedStoreId);
          window.location.href = '/login';
          return;
        }

        this.storeName = storedStoreName;
        this.search();
      });
    }
  }

  ngOnDestroy(): void {
    if (this.mq && this.mqHandler) {
      this.mq.removeEventListener('change', this.mqHandler);
    }
  }

  search(): void {
    if (this.form.invalid) return;
    if (!this.storeId) return;

    const fromStr = this.form.value.fromDateTime as string;
    const toStr = this.form.value.toDateTime as string;

    const from = new Date(fromStr);
    const to = new Date(toStr);

    this.loading = true;

    this.salesService
      .getByStoreAndRange(this.storeId, from, to)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: rows => {
          this.dataSource.data = rows ?? [];
        },
        error: err => {
          console.error(err);
          this.dataSource.data = [];
          alert('Error loading sales');
        }
      });
  }

  getSaleId(row: Sale): string {
    const r: any = row;
    return String(r.saleId ?? r.id ?? '');
  }

  isDeleting(row: Sale): boolean {
    return this.deletingId === this.getSaleId(row);
  }

  deleteSale(row: Sale): void {
    const id = this.getSaleId(row);
    if (!id) return;

    if (!confirm('Delete this sale?')) return;

    this.deletingId = id;

    this.salesService
      .delete(id)
      .pipe(finalize(() => (this.deletingId = null)))
      .subscribe({
        next: () => {
          this.dataSource.data = this.dataSource.data.filter(
            x => this.getSaleId(x) !== id
          );
        },
        error: err => {
          console.error(err);
          alert('Error deleting sale');
        }
      });
  }

  trackById = (_: number, row: Sale): string => {
    return this.getSaleId(row);
  };

  private toLocalInput(d: Date): string {
    const pad = (n: number) => String(n).padStart(2, '0');

    const yyyy = d.getFullYear();
    const mm = pad(d.getMonth() + 1);
    const dd = pad(d.getDate());
    const hh = pad(d.getHours());
    const mi = pad(d.getMinutes());

    return `${yyyy}-${mm}-${dd}T${hh}:${mi}`;
  }
}