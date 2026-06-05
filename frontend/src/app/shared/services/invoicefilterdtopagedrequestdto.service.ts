/* Auto-generated from service-proxies.ts */

export class InvoiceFilterDtoPagedRequestDto implements IInvoiceFilterDtoPagedRequestDto {
    page?: number;
    pageSize?: number;
    sortBy?: string | undefined;
    sortOrder?: string | undefined;
    filter?: InvoiceFilterDto;

    constructor(data?: IInvoiceFilterDtoPagedRequestDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (this as any)[property] = (data as any)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.page = _data["page"];
            this.pageSize = _data["pageSize"];
            this.sortBy = _data["sortBy"];
            this.sortOrder = _data["sortOrder"];
            this.filter = _data["filter"] ? InvoiceFilterDto.fromJS(_data["filter"]) : undefined as any;
        }
    }

    static fromJS(data: any): InvoiceFilterDtoPagedRequestDto {
        data = typeof data === 'object' ? data : {};
        let result = new InvoiceFilterDtoPagedRequestDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["page"] = this.page;
        data["pageSize"] = this.pageSize;
        data["sortBy"] = this.sortBy;
        data["sortOrder"] = this.sortOrder;
        data["filter"] = this.filter ? this.filter.toJSON() : undefined as any;
        return data;
    }
}

export interface IInvoiceFilterDtoPagedRequestDto {
    page?: number;
    pageSize?: number;
    sortBy?: string | undefined;
    sortOrder?: string | undefined;
    filter?: InvoiceFilterDto;
}

export enum InvoiceStatus {
    _1 = 1,
    _2 = 2,
    _3 = 3,
    _4 = 4,
    _5 = 5,
}


