/* Auto-generated from service-proxies.ts */

export class PaymentIntentDto implements IPaymentIntentDto {
    invoiceId?: number;
    amount?: number;
    transferContent?: string | undefined;
    bankCode?: string | undefined;
    accountNumber?: string | undefined;
    accountName?: string | undefined;
    qrImageUrl?: string | undefined;

    constructor(data?: IPaymentIntentDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (this as any)[property] = (data as any)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.invoiceId = _data["invoiceId"];
            this.amount = _data["amount"];
            this.transferContent = _data["transferContent"];
            this.bankCode = _data["bankCode"];
            this.accountNumber = _data["accountNumber"];
            this.accountName = _data["accountName"];
            this.qrImageUrl = _data["qrImageUrl"];
        }
    }

    static fromJS(data: any): PaymentIntentDto {
        data = typeof data === 'object' ? data : {};
        let result = new PaymentIntentDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["invoiceId"] = this.invoiceId;
        data["amount"] = this.amount;
        data["transferContent"] = this.transferContent;
        data["bankCode"] = this.bankCode;
        data["accountNumber"] = this.accountNumber;
        data["accountName"] = this.accountName;
        data["qrImageUrl"] = this.qrImageUrl;
        return data;
    }
}

export interface IPaymentIntentDto {
    invoiceId?: number;
    amount?: number;
    transferContent?: string | undefined;
    bankCode?: string | undefined;
    accountNumber?: string | undefined;
    accountName?: string | undefined;
    qrImageUrl?: string | undefined;
}


