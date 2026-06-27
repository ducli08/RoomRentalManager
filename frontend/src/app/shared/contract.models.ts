export enum StatusContract {
    Active = 1,
    Inactive = 2,
    Canceled = 3,
}

export class ContractDto {
    id?: number;
    roomRentalId?: number;
    roomName?: string;
    tenantId?: number;
    tenantIds?: number[];
    tenantName?: string;
    tenantNames?: string[];
    startDate?: Date;
    endDate?: Date;
    depositAmout?: number;
    monthlyRent?: number;
    electricUnitPrice?: number;
    waterUnitPrice?: number;
    garbageFeePerMonthPerPerson?: number;
    statusContract?: StatusContract;
    createdAt?: Date;
    updatedAt?: Date;
    creatorUser?: string;
    updaterUser?: string;
}

export class ContractFilterDto {
    roomRentalId?: number;
    tenantId?: number;
    statusContract?: StatusContract;
    startDateFrom?: Date;
    startDateTo?: Date;
    endDateFrom?: Date;
    endDateTo?: Date;
    creatorUser?: string;
}

export class ContractFilterDtoPagedRequestDto {
    page?: number;
    pageSize?: number;
    sortBy?: string;
    sortOrder?: string;
    filter?: ContractFilterDto;
}

export class ContractDtoPagedResultDto {
    listItem?: ContractDto[];
    totalCount?: number;
}

export class CreateOrEditContractDto {
    id?: number;
    roomRentalId?: number;
    tenantId?: number;
    tenantIds?: number[];
    startDate?: Date;
    endDate?: Date;
    depositAmout?: string;
    monthlyRent?: string;
    electricUnitPrice?: string;
    waterUnitPrice?: string;
    garbageFeePerMonthPerPerson?: string;
    statusContract?: StatusContract;
}
