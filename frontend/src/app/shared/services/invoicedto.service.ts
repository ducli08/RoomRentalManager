/* Auto-generated from service-proxies.ts */

export class InvoiceDto implements IInvoiceDto {
    id?: number;
    contractId?: number;
    invoiceDate?: Date;
    dueDate?: Date;
    totalAmount?: number;
    amountPaid?: number;
    balanceDue?: number;
    isOverdue?: boolean;
    status?: InvoiceStatus;

    constructor(data?: IInvoiceDto) {
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
            this.amountPaid = _data["amountPaid"];
            this.balanceDue = _data["balanceDue"];
            this.isOverdue = _data["isOverdue"];
            this.status = _data["status"];
        }
    }

    static fromJS(data: any): InvoiceDto {
        data = typeof data === 'object' ? data : {};
        let result = new InvoiceDto();
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
        data["amountPaid"] = this.amountPaid;
        data["balanceDue"] = this.balanceDue;
        data["isOverdue"] = this.isOverdue;
        data["status"] = this.status;
        return data;
    }
}

export interface IInvoiceDto {
    id?: number;
    contractId?: number;
    invoiceDate?: Date;
    dueDate?: Date;
    totalAmount?: number;
    amountPaid?: number;
    balanceDue?: number;
    isOverdue?: boolean;
    status?: InvoiceStatus;
}


