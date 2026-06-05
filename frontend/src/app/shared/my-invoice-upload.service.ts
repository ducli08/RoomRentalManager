import { Inject, Injectable, Optional } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { API_BASE_URL, PaymentSubmissionDto } from './services';

@Injectable({ providedIn: 'root' })
export class MyInvoiceUploadService {
  constructor(
    private http: HttpClient,
    @Optional() @Inject(API_BASE_URL) private baseUrl?: string
  ) {}

  submitPaymentEvidence(invoiceId: number, file: File, declaredAmount: number): Observable<PaymentSubmissionDto> {
    const url = (this.baseUrl ?? 'https://localhost:7246') + `/api/my/invoices/${invoiceId}/payment-submissions`;

    const form = new FormData();
    // Attempt common parameter names to maximize compatibility.
    form.append('file', file, file.name);
    form.append('evidence', file, file.name);
    form.append('declaredAmount', String(declaredAmount));

    return this.http.post<any>(url, form).pipe(map((res) => PaymentSubmissionDto.fromJS(res)));
  }
}

