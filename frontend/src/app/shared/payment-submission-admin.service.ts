import { Inject, Injectable, Optional } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { API_BASE_URL, PaymentSubmissionDto } from './services';

@Injectable({ providedIn: 'root' })
export class PaymentSubmissionAdminService {
  constructor(
    private http: HttpClient,
    @Optional() @Inject(API_BASE_URL) private baseUrl?: string
  ) {}

  getPending(): Observable<PaymentSubmissionDto[]> {
    const url = (this.baseUrl ?? 'http://localhost:5233') + `/api/payment-submissions/pending`;
    return this.http.get<any>(url).pipe(
      map((res) => {
        if (!Array.isArray(res)) return [];
        return res.map((x) => PaymentSubmissionDto.fromJS(x));
      })
    );
  }
}

