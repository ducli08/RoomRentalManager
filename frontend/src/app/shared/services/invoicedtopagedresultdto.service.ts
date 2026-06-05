/* Auto-generated from service-proxies.ts */

export class InvoiceDtoPagedResultDto implements IInvoiceDtoPagedResultDto {
    listItem?: InvoiceDto[] | undefined;
    totalCount?: number;

    constructor(data?: IInvoiceDtoPagedResultDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (this as any)[property] = (data as any)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            if (Array.isArray(_data["listItem"])) {
                this.listItem = [] as any;
                for (let item of _data["listItem"])
                    this.listItem!.push(InvoiceDto.fromJS(item));
            }
            this.totalCount = _data["totalCount"];
        }
    }

    static fromJS(data: any): InvoiceDtoPagedResultDto {
        data = typeof data === 'object' ? data : {};
        let result = new InvoiceDtoPagedResultDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (Array.isArray(this.listItem)) {
            data["listItem"] = [];
            for (let item of this.listItem)
                data["listItem"].push(item ? item.toJSON() : undefined as any);
        }
        data["totalCount"] = this.totalCount;
        return data;
    }
}

export interface IInvoiceDtoPagedResultDto {
    listItem?: InvoiceDto[] | undefined;
    totalCount?: number;
}


