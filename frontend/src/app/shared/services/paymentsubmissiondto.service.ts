/* Auto-generated from service-proxies.ts */

export class PaymentSubmissionDto implements IPaymentSubmissionDto {
    id?: number;
    invoiceId?: number;
    declaredAmount?: number;
    evidenceUrl?: string | undefined;
    status?: PaymentSubmissionStatus;
    rejectedReason?: string | undefined;
    createdAt?: Date;
    creatorUser?: string | undefined;

    constructor(data?: IPaymentSubmissionDto) {
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
            this.invoiceId = _data["invoiceId"];
            this.declaredAmount = _data["declaredAmount"];
            this.evidenceUrl = _data["evidenceUrl"];
            this.status = _data["status"];
            this.rejectedReason = _data["rejectedReason"];
            this.createdAt = _data["createdAt"] ? new Date(_data["createdAt"].toString()) : undefined as any;
            this.creatorUser = _data["creatorUser"];
        }
    }

    static fromJS(data: any): PaymentSubmissionDto {
        data = typeof data === 'object' ? data : {};
        let result = new PaymentSubmissionDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["invoiceId"] = this.invoiceId;
        data["declaredAmount"] = this.declaredAmount;
        data["evidenceUrl"] = this.evidenceUrl;
        data["status"] = this.status;
        data["rejectedReason"] = this.rejectedReason;
        data["createdAt"] = this.createdAt ? this.createdAt.toISOString() : undefined as any;
        data["creatorUser"] = this.creatorUser;
        return data;
    }
}

export interface IPaymentSubmissionDto {
    id?: number;
    invoiceId?: number;
    declaredAmount?: number;
    evidenceUrl?: string | undefined;
    status?: PaymentSubmissionStatus;
    rejectedReason?: string | undefined;
    createdAt?: Date;
    creatorUser?: string | undefined;
}

export enum PaymentSubmissionStatus {
    _1 = 1,
    _2 = 2,
    _3 = 3,
}


