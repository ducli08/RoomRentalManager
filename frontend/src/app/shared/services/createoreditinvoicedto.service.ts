/* Auto-generated from service-proxies.ts */

export class CreateOrEditInvoiceDto implements ICreateOrEditInvoiceDto {
    id?: number | undefined;
    contractId!: number;
    invoiceDate!: Date;
    dueDate!: Date;
    totalAmount!: number;

    constructor(data?: ICreateOrEditInvoiceDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (this as any)[property] = (data as any)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.id = _data["id"];
            this.contractId = _data["contractId"];
            this.invoiceDate = _data["invoiceDate"] ? new Date(_data["invoiceDate"].toString()) : undefined as any;
            this.dueDate = _data["dueDate"] ? new Date(_data["dueDate"].toString()) : undefined as any;
            this.totalAmount = _data["totalAmount"];
        }
    }

    static fromJS(data: any): CreateOrEditInvoiceDto {
        data = typeof data === 'object' ? data : {};
        let result = new CreateOrEditInvoiceDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["contractId"] = this.contractId;
        data["invoiceDate"] = this.invoiceDate ? this.invoiceDate.toISOString() : undefined as any;
        data["dueDate"] = this.dueDate ? this.dueDate.toISOString() : undefined as any;
        data["totalAmount"] = this.totalAmount;
        return data;
    }
}

export interface ICreateOrEditInvoiceDto {
    id?: number | undefined;
    contractId: number;
    invoiceDate: Date;
    dueDate: Date;
    totalAmount: number;
}


