export interface CreateCreditCardApplicationDto {
  storeId: number;
  storeNumber: number;
  approved: boolean;  
  creditCardProductId: number;
  statusId?: number;
}

export interface CreditCardApplicationDto {
  creditCardApplicationId: string;
  statusId: number;
  creditCardProductId: number;
  storeName: string;
  storeNumber: string;
  storeId: number;
  approved: boolean;
  submittedAtUtc: string;
  // agrega los campos reales que tu API regresa
}

export interface CreatedResponse {
  creditCardApplicationId: string;
}
