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
  electricUnitPrice?: number;
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
  creatorUser?: string;
  updaterUser?: string;
  createdAt?: Date;
  updatedAt?: Date;
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
  electricUnitPrice?: number;
  canSave?: boolean;
  message?: string;
}
