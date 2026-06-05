import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { PaymentSubmissionDto, RejectPaymentSubmissionDto, ServiceProxy } from '../../shared/services';
import { PaymentSubmissionAdminService } from '../../shared/payment-submission-admin.service';
import { paymentSubmissionStatusLabel } from '../../shared/payment-submission-status-label';

@Component({
  selector: 'app-payment-submissions',
  standalone: true,
  imports: [CommonModule, FormsModule, NzTableModule, NzButtonModule, NzTagModule, NzInputModule],
  templateUrl: './payment-submissions.component.html',
  styleUrls: ['./payment-submissions.component.css'],
})
export class PaymentSubmissionsComponent implements OnInit {
  loading = false;
  submissions: readonly PaymentSubmissionDto[] = [];

  rejectReason = '';

  constructor(
    private api: PaymentSubmissionAdminService,
    private proxy: ServiceProxy,
    private router: Router,
    private modal: NzModalService,
    private notification: NzNotificationService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  statusLabel(s?: any): string {
    return paymentSubmissionStatusLabel(s);
  }

  money(v?: number): string {
    const n = Number(v ?? 0);
    return new Intl.NumberFormat('vi-VN').format(n) + ' ₫';
  }

  load(): void {
    this.loading = true;
    this.api.getPending().subscribe({
      next: (items) => {
        this.submissions = items;
        this.loading = false;
      },
      error: (err: any) => {
        this.loading = false;
        console.error('Load pending submissions failed', err);
        this.notification.error('Lỗi', 'Không thể tải danh sách chứng từ pending.');
      },
    });
  }

  openInvoice(sub: PaymentSubmissionDto): void {
    if (!sub.invoiceId) return;
    this.router.navigate(['/main/invoices', sub.invoiceId]);
  }

  confirmApprove(sub: PaymentSubmissionDto): void {
    if (!sub.id) return;
    this.modal.confirm({
      nzTitle: 'Duyệt chứng từ',
      nzContent: `Duyệt chứng từ #${sub.id} (Invoice #${sub.invoiceId})?`,
      nzOkText: 'Duyệt',
      nzOnOk: () => this.approve(sub.id!),
      nzCancelText: 'Hủy',
    });
  }

  private approve(id: number): void {
    this.loading = true;
    this.proxy.approve(id).subscribe({
      next: () => {
        this.notification.success('Thành công', 'Đã duyệt chứng từ.');
        this.load();
      },
      error: (err: any) => {
        this.loading = false;
        console.error('Approve failed', err);
        this.notification.error('Lỗi', 'Không thể duyệt chứng từ.');
      },
    });
  }

  confirmReject(sub: PaymentSubmissionDto): void {
    if (!sub.id) return;
    this.rejectReason = '';
    this.modal.create({
      nzTitle: `Từ chối chứng từ #${sub.id}`,
      nzContent: `
        <div>
          <label>Lý do</label>
          <input id="rejectReasonInput" class="ant-input" placeholder="Nhập lý do từ chối" />
        </div>
      `,
      nzOkText: 'Từ chối',
      nzOkDanger: true,
      nzOnOk: () => {
        const el = document.getElementById('rejectReasonInput') as HTMLInputElement | null;
        const reason = (el?.value || '').trim();
        if (!reason) {
          this.notification.warning('Thiếu lý do', 'Vui lòng nhập lý do từ chối.');
          return false;
        }
        this.reject(sub.id!, reason);
        return true;
      },
      nzCancelText: 'Hủy',
    });
  }

  private reject(id: number, reason: string): void {
    this.loading = true;
    const body = new RejectPaymentSubmissionDto({ reason });
    this.proxy.reject(id, body).subscribe({
      next: () => {
        this.notification.success('Thành công', 'Đã từ chối chứng từ.');
        this.load();
      },
      error: (err: any) => {
        this.loading = false;
        console.error('Reject failed', err);
        this.notification.error('Lỗi', 'Không thể từ chối chứng từ.');
      },
    });
  }
}

