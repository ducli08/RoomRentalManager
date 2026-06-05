/* Auto-generated from service-proxies.ts */

export class RejectPaymentSubmissionDto implements IRejectPaymentSubmissionDto {
    reason!: string;

    constructor(data?: IRejectPaymentSubmissionDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (this as any)[property] = (data as any)[property];
            }
        }
    }

    init(_data?: any) {
        if (_data) {
            this.reason = _data["reason"];
        }
    }

    static fromJS(data: any): RejectPaymentSubmissionDto {
        data = typeof data === 'object' ? data : {};
        let result = new RejectPaymentSubmissionDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["reason"] = this.reason;
        return data;
    }
}

export interface IRejectPaymentSubmissionDto {
    reason: string;
}


