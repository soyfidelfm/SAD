import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';

import { CatalogStore } from '../../core/models/catalog-store.model';
import {
  CreateUserDailySetting,
  UpdateUserDailySetting,
  UserDailySetting
} from '../../core/models/user-daily-setting.model';

import { CatalogService } from '../../core/services/catalog.service';
import { UserDailySettingsService } from '../../core/services/user-daily-settings.service';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, MatTableModule, MatProgressSpinnerModule, MatIconModule],
  templateUrl: './settings.html',
  styleUrl: './settings.scss'
})
export class SettingsComponent implements OnInit {
  private fb = inject(FormBuilder);
  private catalogService = inject(CatalogService);
  private userDailySettingsService = inject(UserDailySettingsService);

  loading = false;
  saving = false;
  isEditMode = false;
  errorMessage = '';
  successMessage = '';
  deletingId: string | null = null;

  dataSource = new MatTableDataSource<UserDailySetting>([]);
  stores: CatalogStore[] = [];
  currentSettingId: number | null = null;

  displayedColumnsDesktop: string[] = [
    'settingDate',
    'salesGoalAmount',
    'appsGoal',
    'membershipsGoal',
    'storeName',
    'actions'
  ];

  displayedColumnsMobile: string[] = [
    'settingDate',
    'salesGoalAmount',
    'appsGoal',
    'membershipsGoal',
    'actions'
  ];

  // ⚠️ cámbialo luego por el user real desde token / auth service
  userId = '';

  form = this.fb.group({
    settingDate: [this.getTodayDate(), Validators.required],
    salesGoalAmount: [0, [Validators.required, Validators.min(0)]],
    appsGoal: [0, [Validators.required, Validators.min(0)]],
    membershipsGoal: [0, [Validators.required, Validators.min(0)]],
    storeId: [null as number | null, Validators.required],
    isActive: [true]
  });

  ngOnInit(): void {
    this.loadStores();
    this.loadSettings();

    // ⚠️ temporal: aquí pon el GUID real del usuario autenticado
    this.userId = '00000000-0000-0000-0000-000000000000';

    this.loadTodaySettings();
  }

  get f() {
    return this.form.controls;
  }

  isMobile = false;
  get displayedColumns(): string[] {
    return this.isMobile ? this.displayedColumnsMobile : this.displayedColumnsDesktop;
  }

  loadStores(): void {
    this.catalogService.getStores().subscribe({
      next: (data) => {
        this.stores = data ?? [];
      },
      error: (err) => {
        console.error('Error loading stores', err);
        this.errorMessage = 'Could not load stores.';
      }
    });
  }

  loadSettings(): void {
    this.userDailySettingsService.getAll().subscribe({
      next: (data) => {
        this.dataSource.data = data ?? [];
      },
      error: (err) => {
        console.error('Error loading settings', err);
        this.errorMessage = 'Could not load settings.';
      }
    });
  }

  loadTodaySettings(): void {
    if (!this.userId) return;

    this.loading = true;
    this.errorMessage = '';
    this.successMessage = '';

    this.userDailySettingsService.getToday().subscribe({
      next: (data) => {
        this.loading = false;

        if (!data) {
          this.isEditMode = false;
          this.currentSettingId = null;
          return;
        }

        this.mapForm(data);
        this.currentSettingId = data.id;
        this.isEditMode = true;
      },
      error: (err) => {
        this.loading = false;

        if (err?.status === 404) {
          this.isEditMode = false;
          this.currentSettingId = null;
          return;
        }

        console.error('Error loading today settings', err);
        this.errorMessage = 'Could not load today settings.';
      }
    });
  }

  submit(): void {
    if (this.form.invalid || !this.userId) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.errorMessage = '';
    this.successMessage = '';

    if (this.isEditMode && this.currentSettingId) {
      const payload: UpdateUserDailySetting = {
        settingDate: this.form.value.settingDate ?? this.getTodayDate(),
        salesGoalAmount: Number(this.form.value.salesGoalAmount ?? 0),
        appsGoal: Number(this.form.value.appsGoal ?? 0),
        membershipsGoal: Number(this.form.value.membershipsGoal ?? 0),
        storeId: Number(this.form.value.storeId),
        isActive: !!this.form.value.isActive
      };

      this.userDailySettingsService.update(this.currentSettingId, payload).subscribe({
        next: () => {
          this.saving = false;
          this.successMessage = 'Settings updated successfully.';
          this.loadTodaySettings();
          this.resetForm();
        },
        error: (err) => {
          this.saving = false;
          console.error('Error updating settings', err);
          this.errorMessage = 'Could not update settings.';
        }
      });

      return;
    }

    const createPayload: CreateUserDailySetting = {
      userId: this.userId,
      settingDate: this.form.value.settingDate ?? this.getTodayDate(),
      salesGoalAmount: Number(this.form.value.salesGoalAmount ?? 0),
      appsGoal: Number(this.form.value.appsGoal ?? 0),
      membershipsGoal: Number(this.form.value.membershipsGoal ?? 0),
      storeId: Number(this.form.value.storeId),
      isActive: !!this.form.value.isActive
    };

    this.userDailySettingsService.create(createPayload).subscribe({
      next: (created) => {
        this.saving = false;
        this.successMessage = 'Settings created successfully.';
        this.currentSettingId = created.id;
        this.isEditMode = true;
        this.mapForm(created);
        this.resetForm();
      },
      error: (err) => {
        this.saving = false;
        console.error('Error creating settings', err);
        this.errorMessage = 'Could not create settings.';
      }
    });
  }

  resetForm(): void {
    this.form.reset({
      settingDate: this.getTodayDate(),
      salesGoalAmount: null,
      appsGoal: null,
      membershipsGoal: null,
      storeId: null,
      isActive: true
    });

    this.errorMessage = '';
    this.successMessage = '';
    this.currentSettingId = null;
    this.isEditMode = false;
  }

  private mapForm(data: UserDailySetting): void {
    this.form.patchValue({
      settingDate: this.normalizeDate(data.settingDate),
      salesGoalAmount: data.salesGoalAmount,
      appsGoal: data.appsGoal,
      membershipsGoal: data.membershipsGoal,
      storeId: data.storeId,
      isActive: data.isActive
    });
  }

  private getTodayDate(): string {
    return new Date().toISOString().split('T')[0];
  }

  private normalizeDate(value: string): string {
    if (!value) return this.getTodayDate();
    return value.includes('T') ? value.split('T')[0] : value;
  }

  deleteSetting(row: UserDailySetting): void {
      const id = this.getSettingsId(row);
      if (!id) return;
  
      if (!confirm('Delete this setting?')) return;
  
      this.deletingId = id;
  
      this.userDailySettingsService
        .delete(id)
        .pipe(finalize(() => (this.deletingId = null)))
        .subscribe({
          next: () => {
            this.dataSource.data = this.dataSource.data.filter(
              x => this.getSettingsId(x) !== id
            );
          },
          error: err => {
            console.error(err);
            alert('Error deleting setting');
          }
        });
    }

  trackById = (_: number, row: UserDailySetting): string => {
      return this.getSettingsId(row);
    };

    getSettingsId(row: UserDailySetting): string {
        return row.id ? row.id.toString() : '';
      }

      isDeleting(row: UserDailySetting): boolean {
          return this.deletingId === this.getSettingsId(row);
        }
}
