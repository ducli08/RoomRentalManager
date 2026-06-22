export enum UtilityReadingStatus {
  Confirmed = 1,
  InvoiceGenerated = 2,
}

export class UtilityReadingDto {
  id?: number;
  contractId?: number;
  roomName?: string;
  tenantName?: string;
  month?: number;
  year?: number;
  oldElectricIndex?: number;
  newElectricIndex?: number;
  electricUsage?: number;
  oldWaterIndex?: number;
  newWaterIndex?: number;
  waterUsage?: number;
  electricUnitPrice?: number;
  waterUnitPrice?: number;
  status?: UtilityReadingStatus;
  isLockedByPayment?: boolean;
  createdAt?: Date;
  updatedAt?: Date;
  creatorUser?: string;
  updaterUser?: string;
}

export class UtilityReadingFilterDto {
  month?: number;
  year?: number;
  contractId?: number;
  status?: UtilityReadingStatus;
  roomRentalId?: number;
  tenantId?: number;
}

export class UtilityReadingFilterDtoPagedRequestDto {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: string;
  filter?: UtilityReadingFilterDto;
}

export class UtilityReadingDtoPagedResultDto {
  listItem?: UtilityReadingDto[];
  totalCount?: number;
}

export class CreateOrEditUtilityReadingDto {
  id?: number;
  contractId?: number;
  month?: number;
  year?: number;
  newElectricIndex?: number;
  newWaterIndex?: number;
}

export class UtilityReadingPrepareDto {
  contractId?: number;
  month?: number;
  year?: number;
  roomName?: string;
  tenantName?: string;
  contractStartDate?: Date;
  contractEndDate?: Date;
  oldElectricIndex?: number;
  oldWaterIndex?: number;
  electricUnitPrice?: number;
  waterUnitPrice?: number;
  canSave?: boolean;
  message?: string;
}
