export interface CreateMembershipSaleDto {
  userId: string;
  storeId: number;
  membershipProductId: number;
  statusId: number;
}

export interface MembershipSaleDto {
  membershipSaleId: string;
  userId: string;
  storeId: number;
  membershipProductId: number;
  statusId: number;
  soldAtUtc: string; // ISO string
}

export interface CreatedMembershipSaleResponse {
  membershipSaleId: string;
}
