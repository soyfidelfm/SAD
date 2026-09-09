import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';

import { CatalogStore, UpsertStore } from '../../core/models/catalog-store.model';
import { CatalogService } from '../../core/services/catalog.service';

@Component({
  selector: 'app-stores',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './stores.html',
  styleUrl: './stores.scss'
})
export class StoresComponent implements OnInit {
  private fb = inject(FormBuilder);
  private catalog = inject(CatalogService);

  stores: CatalogStore[] = [];
  filteredStores: CatalogStore[] = [];
  loading = false;
  saving = false;
  isEditMode = false;
  currentStoreId: number | null = null;
  deletingId: number | null = null;
  errorMessage = '';
  successMessage = '';

  form = this.fb.group({
    storeNumber: [null as number | null, [Validators.required, Validators.min(1)]],
    storeName: [''],
    isActive: [true]
  });

  filterForm = this.fb.group({
    search: ['']
  });

  ngOnInit(): void {
    this.filterForm.controls.search.valueChanges.subscribe(() => this.applyFilter());
    this.loadStores();
  }

  get f() {
    return this.form.controls;
  }

  get activeCount(): number {
    return this.stores.filter((s) => s.isActive).length;
  }

  loadStores(): void {
    this.loading = true;
    this.errorMessage = '';

    this.catalog.getAllStores().subscribe({
      next: (data) => {
        this.stores = data ?? [];
        this.applyFilter();
        this.loading = false;
      },
      error: (err) => {
        console.error('Error loading stores', err);
        this.loading = false;
        this.errorMessage = 'Could not load stores.';
      }
    });
  }

  applyFilter(): void {
    const q = (this.filterForm.value.search ?? '').trim().toLowerCase();
    if (!q) {
      this.filteredStores = [...this.stores];
      return;
    }

    this.filteredStores = this.stores.filter((store) => {
      const name = (store.storeName ?? '').toLowerCase();
      const number = String(store.storeNumber);
      return name.includes(q) || number.includes(q);
    });
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const payload: UpsertStore = {
      storeNumber: Number(this.form.value.storeNumber),
      storeName: this.form.value.storeName?.trim() || null,
      isActive: !!this.form.value.isActive
    };

    this.saving = true;
    this.errorMessage = '';
    this.successMessage = '';

    if (this.isEditMode && this.currentStoreId) {
      this.catalog.updateStore(this.currentStoreId, payload).subscribe({
        next: () => {
          this.saving = false;
          this.successMessage = 'Store updated.';
          this.resetForm();
          this.loadStores();
        },
        error: (err) => {
          this.saving = false;
          this.errorMessage = err?.error?.message || 'Could not update store.';
        }
      });
      return;
    }

    this.catalog.createStore(payload).subscribe({
      next: () => {
        this.saving = false;
        this.successMessage = 'Store created.';
        this.resetForm();
        this.loadStores();
      },
      error: (err) => {
        this.saving = false;
        this.errorMessage = err?.error?.message || 'Could not create store.';
      }
    });
  }

  editStore(store: CatalogStore): void {
    this.isEditMode = true;
    this.currentStoreId = store.storeId;
    this.successMessage = '';
    this.errorMessage = '';
    this.form.reset({
      storeNumber: store.storeNumber,
      storeName: store.storeName ?? '',
      isActive: store.isActive
    });
  }

  resetForm(): void {
    this.form.reset({
      storeNumber: null,
      storeName: '',
      isActive: true
    });
    this.isEditMode = false;
    this.currentStoreId = null;
  }

  deactivateStore(store: CatalogStore): void {
    if (!store.storeId) return;
    if (!confirm(`Deactivate store #${store.storeNumber}? It will stay in history but drop out of dropdowns.`))
      return;

    this.deletingId = store.storeId;

    this.catalog
      .deactivateStore(store.storeId)
      .pipe(finalize(() => (this.deletingId = null)))
      .subscribe({
        next: () => {
          this.loadStores();
          if (this.currentStoreId === store.storeId) {
            this.resetForm();
          }
        },
        error: (err) => {
          console.error(err);
          this.errorMessage = 'Could not deactivate store.';
        }
      });
  }

  isDeleting(store: CatalogStore): boolean {
    return this.deletingId === store.storeId;
  }

  trackById = (_: number, row: CatalogStore): number => row.storeId;
}
