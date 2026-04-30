import {
  Component,
  EventEmitter,
  Input,
  Output,
  OnChanges,
  SimpleChanges,
  OnInit
} from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
  FormGroup,
  FormControl
} from '@angular/forms';
import { catchError, finalize, of } from 'rxjs';

import { CatalogService } from '../../core/services/catalog.service';
import { CatalogStore } from '../../core/models/catalog-store.model';
import { CatalogMembership } from '../../core/models/catalog-membership.model';

export type AddPopupMode = 'credit' | 'membership' | 'sale';

type AddPopupForm = {
  storeId: FormControl<number | null>;
  storeNumber: FormControl<number | null>;

  // credit
  approved: FormControl<boolean | null>;

  // membership
  membershipProductId: FormControl<number | null>;
  termMonths: FormControl<number | null>;

  // sale
  saleAmount: FormControl<number | null>;
  notes: FormControl<string>;
};

@Component({
  selector: 'app-add-popup',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './add-popup.html',
  styleUrls: ['./add-popup.scss']
})
export class AddPopupComponent implements OnInit, OnChanges {
  @Input({ required: true }) mode!: AddPopupMode;
  @Output() close = new EventEmitter<void>();
  @Output() submitForm = new EventEmitter<{ mode: AddPopupMode; payload: any }>();

  form: FormGroup<AddPopupForm>;

  // catalogs
  stores: CatalogStore[] = [];
  membershipProducts: CatalogMembership[] = [];

  // states
  loadingStores = false;
  storesError = false;

  loadingMemberships = false;
  membershipsError = false;

  constructor(private fb: FormBuilder, private catalog: CatalogService) {
    this.form = this.fb.group<AddPopupForm>({
      storeId: this.fb.control<number | null>(null),
      storeNumber: this.fb.control<number | null>(null),

      approved: this.fb.control<boolean | null>(null),

      membershipProductId: this.fb.control<number | null>(null),
      termMonths: this.fb.control<number | null>(null),

      saleAmount: this.fb.control<number | null>(null),
      notes: this.fb.control('', { nonNullable: true })
    });
  }

  ngOnInit(): void {
    this.ensureCatalogsForMode();
    this.applyValidatorsByMode();

    // Cuando eligen storeId, autocompleta storeNumber
    this.form.controls.storeId.valueChanges.subscribe(storeId => {
      const s = this.stores.find(x => x.storeId === storeId);
      this.form.controls.storeNumber.setValue(s?.storeNumber ?? null, { emitEvent: false });
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['mode']) {
      this.ensureCatalogsForMode();
      this.applyValidatorsByMode();
      this.resetModeSpecificControls();
    }
  }

  // ---------- Catalog loaders (PUBLIC: used by Retry buttons) ----------

  loadStores(): void {
    this.loadingStores = true;
    this.storesError = false;

    this.catalog
      .getStores()
      .pipe(
        catchError(err => {
          console.error('Error loading stores', err);
          this.storesError = true;
          return of([] as CatalogStore[]);
        }),
        finalize(() => (this.loadingStores = false))
      )
      .subscribe(data => {
        this.stores = (data ?? [])
          .filter(s => s.isActive)
          .sort((a, b) => a.storeNumber - b.storeNumber);

        // si ya había un store seleccionado, rehidrata storeNumber
        const currentStoreId = this.form.controls.storeId.value;
        const s = this.stores.find(x => x.storeId === currentStoreId);
        this.form.controls.storeNumber.setValue(s?.storeNumber ?? null, { emitEvent: false });
      });
  }

  loadMembershipProducts(): void {
    this.loadingMemberships = true;
    this.membershipsError = false;

    this.catalog
      .getMembershipProducts()
      .pipe(
        catchError(err => {
          console.error('Error loading membership products', err);
          this.membershipsError = true;
          return of([] as CatalogMembership[]);
        }),
        finalize(() => (this.loadingMemberships = false))
      )
      .subscribe((data: any) => {
        // ✅ soporta camelCase o PascalCase
        const mapped: CatalogMembership[] = (data ?? []).map((x: any) => ({
          membershipProductId: x.membershipProductId ?? x.MembershipProductId,
          productCode: x.productCode ?? x.ProductCode,
          productName: x.productName ?? x.ProductName,
          isActive: x.isActive ?? x.IsActive
        }));0

        this.membershipProducts = mapped
          .filter(m => !!m && m.isActive && !!m.productName)
          .sort((a, b) => (a.productName ?? '').localeCompare(b.productName ?? ''));
      });
  }

  // ---------- Mode behavior ----------

  private ensureCatalogsForMode(): void {
  if (this.mode === 'credit') {
    if (!this.stores.length && !this.loadingStores) this.loadStores();
    return;
  }

  if (this.mode === 'membership') {
    if (!this.stores.length && !this.loadingStores) this.loadStores();
    if (!this.membershipProducts.length && !this.loadingMemberships) this.loadMembershipProducts();
    return;
  }

  if (this.mode === 'sale') {
    if (!this.stores.length && !this.loadingStores) this.loadStores(); // ✅
  }
}

  private applyValidatorsByMode(): void {
  this.clearValidators();

  if (this.mode === 'credit') {
    this.form.controls.storeId.setValidators([Validators.required]);
    this.form.controls.approved.setValidators([Validators.required]);
  }

  if (this.mode === 'membership') {
    this.form.controls.storeId.setValidators([Validators.required]);
    this.form.controls.membershipProductId.setValidators([Validators.required]);
    this.form.controls.termMonths.setValidators([Validators.required, Validators.min(1)]);
  }

  if (this.mode === 'sale') {
    this.form.controls.storeId.setValidators([Validators.required]); // ✅
    this.form.controls.saleAmount.setValidators([Validators.required, Validators.min(0.01)]);
  }

  Object.values(this.form.controls).forEach(c => c.updateValueAndValidity());
}

  private clearValidators(): void {
    Object.values(this.form.controls).forEach(ctrl => ctrl.clearValidators());
  }

  private resetModeSpecificControls(): void {
    this.form.controls.approved.reset(null);
    this.form.controls.membershipProductId.reset(null);
    this.form.controls.termMonths.reset(null);
    this.form.controls.saleAmount.reset(null);
    this.form.controls.notes.reset('');
  }

  // ---------- UI events ----------

  onBackdropClick(): void {
    this.close.emit();
  }

  onDialogClick(event: MouseEvent): void {
    event.stopPropagation();
  }

  onSubmit(): void {
    this.form.markAllAsTouched();
    if (this.form.invalid) return;

    const v = this.form.getRawValue();

    let payload: any = {};

    if (this.mode === 'credit') {
      payload = {
        storeId: v.storeId,
        storeNumber: v.storeNumber,
        approved: v.approved,
        creditCardProductId: 1,
        statusId: v.approved ? 1 : 3 // 👈 FIX
      };
    } else if (this.mode === 'membership') {
      payload = {
        storeId: v.storeId,
        storeNumber: v.storeNumber,
        membershipProductId: v.membershipProductId,
        termMonths: v.termMonths,
        statusId: 1
      };
    } else {
  payload = {
    storeId: v.storeId,
    storeNumber: v.storeNumber,
    saleAmount: v.saleAmount,
    notes: v.notes
  };
}


    this.submitForm.emit({ mode: this.mode, payload });
    this.close.emit();
  }

  get title(): string {
    switch (this.mode) {
      case 'credit':
        return 'New Credit Card Application';
      case 'membership':
        return 'New Membership';
      case 'sale':
        return 'New Sale';
    }
  }
}
