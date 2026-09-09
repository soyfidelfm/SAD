export interface CatalogStore {
  storeId: number;
  storeNumber: number;
  storeName?: string;
  isActive: boolean;
}

export interface UpsertStore {
  storeNumber: number;
  storeName?: string | null;
  isActive: boolean;
}
