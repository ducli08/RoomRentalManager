import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit, Optional } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzDescriptionsModule } from 'ng-zorro-antd/descriptions';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzImageModule } from 'ng-zorro-antd/image';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { API_BASE_URL, InvoiceDto, PaymentIntentDto, PaymentSubmissionDto, ServiceProxy } from '../../../shared/services';
import { invoiceStatusLabel } from '../../../shared/invoice-status-label';
import { paymentSubmissionStatusLabel } from '../../../shared/payment-submission-status-label';
import { MyInvoiceUploadService } from '../../../shared/my-invoice-upload.service';
import { resolveAssetUrl } from '../../../shared/resolve-asset-url';

@Component({
  selector: 'app-my-invoice-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    NzCardModule,
    NzDescriptionsModule,
    NzButtonModule,
    NzImageModule,
    NzFormModule,
    NzInputModule,
    NzTagModule,
  ],
  templateUrl: './my-invoice-detail.component.html',
  styleUrls: ['./my-invoice-detail.component.css'],
})
export class MyInvoiceDetailComponent implements OnInit {
  loading = false;
  intentLoading = false;
  submitLoading = false;

  invoiceId!: number;
  invoice?: InvoiceDto;
  intent?: PaymentIntentDto;
  lastSubmission?: PaymentSubmissionDto;

  declaredAmount?: number;
  selectedFile?: File;

  baseUrl?: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private proxy: ServiceProxy,
    private upload: MyInvoiceUploadService,
    private notification: NzNotificationService,
    @Optional() @Inject(API_BASE_URL) baseUrl?: string
  ) {
    this.baseUrl = baseUrl;
  }

  ngOnInit(): void {
    const idStr = this.route.snapshot.paramMap.get('id');
    const id = idStr ? Number(idStr) : NaN;
    if (!id || Number.isNaN(id)) {
      this.router.navigate(['/main/my-invoices']);
      return;
    }
    this.invoiceId = id;
    this.loadInvoice();
  }

  money(v?: number): string {
    const n = Number(v ?? 0);
    return new Intl.NumberFormat('vi-VN').format(n) + ' ₫';
  }

  statusLabel(): string {
    return invoiceStatusLabel(this.invoice?.status as any);
  }

  submissionStatusLabel(): string {
    return paymentSubmissionStatusLabel(this.lastSubmission?.status as any);
  }

  qrImageSrc(): string | undefined {
    if (!this.intent?.qrImageUrl) return undefined;
    return resolveAssetUrl(this.baseUrl, this.intent.qrImageUrl);
  }

  evidenceSrc(): string | undefined {
    if (!this.lastSubmission?.evidenceUrl) return undefined;
    return resolveAssetUrl(this.baseUrl, this.lastSubmission.evidenceUrl);
  }

  loadInvoice(): void {
    this.loading = true;
    this.proxy.invoicesGET2(this.invoiceId).subscribe({
      next: (inv) => {
        this.invoice = inv;
        this.loading = false;
      },
      error: (err: any) => {
        this.loading = false;
        console.error('Load my invoice failed', err);
        this.notification.error('Lỗi', 'Không thể tải chi tiết hóa đơn.');
      },
    });
  }

  createIntent(): void {
    this.intentLoading = true;
    this.proxy.paymentIntents(this.invoiceId).subscribe({
      next: (intent) => {
        this.intent = intent;
        this.declaredAmount = intent.amount ?? this.invoice?.balanceDue ?? undefined;
        this.intentLoading = false;
        this.notification.success('Thành công', 'Đã tạo thông tin thanh toán.');
      },
      error: (err: any) => {
        this.intentLoading = false;
        console.error('Create payment intent failed', err);
        this.notification.error('Lỗi', 'Không thể tạo payment intent.');
      },
    });
  }

  onFileChange(ev: Event): void {
    const input = ev.target as HTMLInputElement | null;
    const f = input?.files?.[0];
    if (!f) {
      this.selectedFile = undefined;
      return;
    }
    // basic client-side validation
    const isImage = /^image\/(png|jpe?g)$/i.test(f.type);
    if (!isImage) {
      this.notification.warning('Không hợp lệ', 'Chỉ hỗ trợ ảnh PNG/JPG.');
      this.selectedFile = undefined;
      if (input) input.value = '';
      return;
    }
    this.selectedFile = f;
  }

  submitEvidence(): void {
    if (!this.intent) {
      this.notification.warning('Thiếu thông tin', 'Vui lòng tạo payment intent trước.');
      return;
    }
    if (!this.selectedFile) {
      this.notification.warning('Thiếu file', 'Vui lòng chọn ảnh chứng từ.');
      return;
    }
    const amt = Number(this.declaredAmount);
    if (!amt || Number.isNaN(amt) || amt <= 0) {
      this.notification.warning('Không hợp lệ', 'Số tiền khai báo không hợp lệ.');
      return;
    }

    this.submitLoading = true;
    this.upload.submitPaymentEvidence(this.invoiceId, this.selectedFile, amt).subscribe({
      next: (sub) => {
        this.lastSubmission = sub;
        this.submitLoading = false;
        this.notification.success('Đã gửi', 'Chứng từ đã được gửi, vui lòng chờ duyệt.');
      },
      error: (err: any) => {
        this.submitLoading = false;
        console.error('Submit evidence failed', err);
        this.notification.error('Lỗi', 'Không thể gửi chứng từ.');
      },
    });
  }
}

