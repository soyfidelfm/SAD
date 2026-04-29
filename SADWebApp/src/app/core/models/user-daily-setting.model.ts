export interface UserDailySetting {
  id: number;
  userId: string;
  settingDate: string;
  salesGoalAmount: number;
  appsGoal: number;
  membershipsGoal: number;
  storeId: number;
  storeName?: string | null;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface CreateUserDailySetting {
  settingDate: string;
  salesGoalAmount: number;
  appsGoal: number;
  membershipsGoal: number;
  storeId: number;
  isActive: boolean;
}

export interface UpdateUserDailySetting {
  settingDate: string;
  salesGoalAmount: number;
  appsGoal: number;
  membershipsGoal: number;
  storeId: number;
  isActive: boolean;
}