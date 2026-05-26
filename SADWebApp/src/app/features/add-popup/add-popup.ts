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
import { ReceiptOcrService } from '../../core/services/receipt-ocr.service';

import { CatalogStore } from '../../core/models/catalog-store.model';
import { CatalogMembership } from '../../core/models/catalog-membership.model';

export type AddPopupMode = 'credit' | 'membership' | 'sale';

type AddPopupForm = {
  storeId: FormControl<number | null>;
  storeNumber: FormControl<number | null>;

  approved: FormControl<boolean | null>;

  membershipProductId: FormControl<number | null>;
  termMonths: FormControl<number | null>;

  saleAmount: FormControl<number | null>;
  taxAmount: FormControl<number | null>;
  totalAmount: FormControl<number | null>;
  paymentMethod: FormControl<string | null>;

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

  @Output() submitForm =
    new EventEmitter<{ mode: AddPopupMode; payload: any }>();

  form: FormGroup<AddPopupForm>;

  stores: CatalogStore[] = [];
  membershipProducts: CatalogMembership[] = [];

  loadingStores = false;
  storesError = false;

  loadingMemberships = false;
  membershipsError = false;

  ocrLoading = false;
  ocrError = false;

  paymentMethods: string[] = [
    'CASH',
    'DEBIT',
    'VISA',
    'MASTERCARD',
    'AMEX',
    'DISCOVER',
    'APPLE PAY',
    'GOOGLE PAY',
    'PAYPAL'
  ];

  private pendingOcrStoreNumber: number | null = null;

  constructor(
    private fb: FormBuilder,
    private catalog: CatalogService,
    private receiptOcr: ReceiptOcrService
  ) {
    this.form = this.fb.group<AddPopupForm>({
      storeId: this.fb.control<number | null>(1),
      storeNumber: this.fb.control<number | null>(null),

      approved: this.fb.control<boolean | null>(null),

      membershipProductId: this.fb.control<number | null>(null),
      termMonths: this.fb.control<number | null>(null),

      saleAmount: this.fb.control<number | null>(null),
      taxAmount: this.fb.control<number | null>(null),
      totalAmount: this.fb.control<number | null>(null),
      paymentMethod: this.fb.control<string | null>(null),

      notes: this.fb.control('', { nonNullable: true })
    });
  }

  ngOnInit(): void {
    this.ensureCatalogsForMode();
    this.applyValidatorsByMode();

    this.form.controls.storeId.valueChanges.subscribe(storeId => {
      const store = this.stores.find(x => x.storeId === storeId);

      this.form.controls.storeNumber.setValue(
        store?.storeNumber ?? null,
        { emitEvent: false }
      );
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['mode']) {
      this.ensureCatalogsForMode();
      this.applyValidatorsByMode();
      this.resetModeSpecificControls();
    }
  }

  onReceiptSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (!input.files || input.files.length === 0) {
      return;
    }

    const file = input.files[0];

    this.ocrLoading = true;
    this.ocrError = false;

    this.receiptOcr
      .readReceipt(file)
      .subscribe({
        next: (result: any) => {
          console.log('OCR RESULT', result);

          const subtotal =
            result.subtotal ??
            result.subTotal ??
            result.subTotalAmount ??
            null;

          const tax =
            result.tax ??
            result.salesTax ??
            result.taxAmount ??
            null;

          const total =
            result.total ??
            result.balanceTotal ??
            result.totalDue ??
            null;

          const paymentMethod =
            result.paymentMethod ?? null;

          const storeNumber =
            Number(result.storeNumber ?? 0);

          const items = result.items ?? [];

          if (storeNumber > 0) {
            this.pendingOcrStoreNumber = storeNumber;
            this.trySelectStoreByNumber(storeNumber);
          }

          const itemLines = items.length > 0
            ? items
                .map((item: any, index: number) => {
                  const sku = item.sku ?? item.SKU ?? 'N/A';

                  const description =
                    item.description ??
                    item.Description ??
                    item.name ??
                    item.Name ??
                    'N/A';

                  return `${index + 1}. SKU: ${sku} - ${description}`;
                })
                .join('\n')
            : 'No items found';

          this.form.patchValue({
            saleAmount: subtotal != null ? Number(subtotal) : null,
            taxAmount: tax != null ? Number(tax) : null,
            totalAmount: total != null ? Number(total) : null,
            paymentMethod: paymentMethod,
            notes:
`Items:
${itemLines}`
          });

          this.ocrLoading = false;
        },

        error: err => {
          console.error('OCR ERROR', err);

          this.ocrError = true;
          this.ocrLoading = false;
        }
      });
  }

  private trySelectStoreByNumber(storeNumber: number): void {
    if (!this.stores.length) {
      return;
    }

    const store = this.stores.find(
      s => Number(s.storeNumber) === Number(storeNumber)
    );

    if (!store) {
      console.warn('Store not found for OCR store number:', storeNumber);
      return;
    }

    this.form.patchValue({
      storeId: store.storeId,
      storeNumber: store.storeNumber
    });
  }

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

        const currentStoreId = this.form.controls.storeId.value;
        const store = this.stores.find(x => x.storeId === currentStoreId);

        this.form.controls.storeNumber.setValue(
          store?.storeNumber ?? null,
          { emitEvent: false }
        );

        if (!this.form.controls.storeId.value) {
          const defaultStore = this.stores.find(s => s.storeId === 1);

          if (defaultStore) {
            this.form.patchValue({
              storeId: defaultStore.storeId,
              storeNumber: defaultStore.storeNumber
            });
          }
        }

        if (this.pendingOcrStoreNumber != null) {
          this.trySelectStoreByNumber(this.pendingOcrStoreNumber);
        }
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
        const mapped: CatalogMembership[] =
          (data ?? []).map((x: any) => ({
            membershipProductId:
              x.membershipProductId ?? x.MembershipProductId,

            productCode:
              x.productCode ?? x.ProductCode,

            productName:
              x.productName ?? x.ProductName,

            isActive:
              x.isActive ?? x.IsActive
          }));

        this.membershipProducts = mapped
          .filter(m => !!m && m.isActive && !!m.productName)
          .sort((a, b) =>
            (a.productName ?? '').localeCompare(b.productName ?? '')
          );
      });
  }

  private ensureCatalogsForMode(): void {
    if (this.mode === 'credit') {
      if (!this.stores.length && !this.loadingStores) {
        this.loadStores();
      }

      return;
    }

    if (this.mode === 'membership') {
      if (!this.stores.length && !this.loadingStores) {
        this.loadStores();
      }

      if (!this.membershipProducts.length && !this.loadingMemberships) {
        this.loadMembershipProducts();
      }

      return;
    }

    if (this.mode === 'sale') {
      if (!this.stores.length && !this.loadingStores) {
        this.loadStores();
      }
    }
  }

  private applyValidatorsByMode(): void {
    this.clearValidators();

    if (this.mode === 'credit') {
      this.form.controls.storeId.setValidators([
        Validators.required
      ]);

      this.form.controls.approved.setValidators([
        Validators.required
      ]);
    }

    if (this.mode === 'membership') {
      this.form.controls.storeId.setValidators([
        Validators.required
      ]);

      this.form.controls.membershipProductId.setValidators([
        Validators.required
      ]);

      this.form.controls.termMonths.setValidators([
        Validators.required,
        Validators.min(1)
      ]);
    }

    if (this.mode === 'sale') {
      this.form.controls.storeId.setValidators([
        Validators.required
      ]);

      this.form.controls.saleAmount.setValidators([
        Validators.required,
        Validators.min(0.01)
      ]);

      this.form.controls.taxAmount.setValidators([
        Validators.required,
        Validators.min(0)
      ]);

      this.form.controls.totalAmount.setValidators([
        Validators.required,
        Validators.min(0.01)
      ]);

      this.form.controls.paymentMethod.setValidators([
        Validators.required
      ]);
    }

    Object.values(this.form.controls)
      .forEach(c => c.updateValueAndValidity());
  }

  private clearValidators(): void {
    Object.values(this.form.controls)
      .forEach(ctrl => ctrl.clearValidators());
  }

  private resetModeSpecificControls(): void {
    this.form.controls.approved.reset(null);
    this.form.controls.membershipProductId.reset(null);
    this.form.controls.termMonths.reset(null);

    this.form.controls.saleAmount.reset(null);
    this.form.controls.taxAmount.reset(null);
    this.form.controls.totalAmount.reset(null);
    this.form.controls.paymentMethod.reset(null);

    this.form.controls.notes.reset('');

    this.pendingOcrStoreNumber = null;
    this.ocrError = false;
    this.ocrLoading = false;
  }

  onBackdropClick(): void {
    this.close.emit();
  }

  onDialogClick(event: MouseEvent): void {
    event.stopPropagation();
  }

  onSubmit(): void {
    this.form.markAllAsTouched();

    if (this.form.invalid) {
      return;
    }

    const v = this.form.getRawValue();

    let payload: any = {};

    if (this.mode === 'credit') {
      payload = {
        storeId: v.storeId,
        storeNumber: v.storeNumber,
        approved: v.approved,
        creditCardProductId: 1,
        statusId: v.approved ? 1 : 3
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

  subtotal: v.saleAmount,
  tax: v.taxAmount,
  total: v.totalAmount,

  paymentMethod: v.paymentMethod,
  notes: v.notes
};
    }

    this.submitForm.emit({
      mode: this.mode,
      payload
    });

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