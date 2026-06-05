/* Auto-generated from service-proxies.ts */

export class InvoiceFilterDto implements IInvoiceFilterDto {
    contractId?: number | undefined;
    status?: InvoiceStatus;
    isOverdue?: boolean | undefined;

    constructor(data?: IInvoiceFilterDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (this as any)[property] = (data as any)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.contractId = _data["contractId"];
            this.status = _data["status"];
            this.isOverdue = _data["isOverdue"];
        }
    }

    static fromJS(data: any): InvoiceFilterDto {
        data = typeof data === 'object' ? data : {};
        let result = new InvoiceFilterDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["contractId"] = this.contractId;
        data["status"] = this.status;
        data["isOverdue"] = this.isOverdue;
        return data;
    }
}

export interface IInvoiceFilterDto {
    contractId?: number | undefined;
    status?: InvoiceStatus;
    isOverdue?: boolean | undefined;
}


