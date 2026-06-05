import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzDescriptionsModule } from 'ng-zorro-antd/descriptions';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzModalService } from 'ng-zorro-antd/modal';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { InvoiceDto, ServiceProxy } from '../../../shared/services';
import { invoiceStatusLabel } from '../../../shared/invoice-status-label';

@Component({
  selector: 'app-invoice-detail',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    NzCardModule,
    NzDescriptionsModule,
    NzButtonModule,
    NzFormModule,
    NzInputModule,
    NzTagModule,
  ],
  templateUrl: './invoice-detail.component.html',
  styleUrls: ['./invoice-detail.component.css'],
})
export class InvoiceDetailComponent {
  loading = false;
  actionLoading = false;

  invoiceId!: number;
  invoice?: InvoiceDto;

  cashNote = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private proxy: ServiceProxy,
    private modal: NzModalService,
    private notification: NzNotificationService
  ) {
    const idStr = this.route.snapshot.paramMap.get('id');
    const id = idStr ? Number(idStr) : NaN;
    if (!id || Number.isNaN(id)) {
      this.router.navigate(['/main/invoices']);
      return;
    }
    this.invoiceId = id;
    this.load();
  }

  money(v?: number): string {
    const n = Number(v ?? 0);
    return new Intl.NumberFormat('vi-VN').format(n) + ' ₫';
  }

  statusLabel(): string {
    return invoiceStatusLabel(this.invoice?.status as any);
  }

  load(): void {
    this.loading = true;
    this.proxy.invoicesGET(this.invoiceId).subscribe({
      next: (inv: InvoiceDto) => {
        this.invoice = inv;
        this.loading = false;
      },
      error: (err: any) => {
        this.loading = false;
        console.error('Load invoice detail failed', err);
        this.notification.error('Lỗi', 'Không thể tải chi tiết hóa đơn.');
      },
    });
  }

  edit(): void {
    this.router.navigate(['/main/invoices', this.invoiceId, 'edit']);
  }

  confirmIssue(): void {
    this.modal.confirm({
      nzTitle: 'Phát hành hóa đơn',
      nzContent: 'Bạn có chắc muốn phát hành hóa đơn này?',
      nzOkText: 'Phát hành',
      nzOnOk: () => this.issue(),
      nzCancelText: 'Hủy',
    });
  }

  private issue(): void {
    this.actionLoading = true;
    this.proxy.issue(this.invoiceId).subscribe({
      next: () => {
        this.actionLoading = false;
        this.notification.success('Thành công', 'Đã phát hành hóa đơn.');
        this.load();
      },
      error: (err: any) => {
        this.actionLoading = false;
        console.error('Issue invoice failed', err);
        this.notification.error('Lỗi', 'Không thể phát hành hóa đơn.');
      },
    });
  }

  confirmCancel(): void {
    this.modal.confirm({
      nzTitle: 'Hủy hóa đơn',
      nzContent: 'Bạn có chắc muốn hủy hóa đơn này?',
      nzOkText: 'Hủy hóa đơn',
      nzOkDanger: true,
      nzOnOk: () => this.cancel(),
      nzCancelText: 'Không',
    });
  }

  private cancel(): void {
    this.actionLoading = true;
    this.proxy.cancel(this.invoiceId).subscribe({
      next: () => {
        this.actionLoading = false;
        this.notification.success('Thành công', 'Đã hủy hóa đơn.');
        this.load();
      },
      error: (err: any) => {
        this.actionLoading = false;
        console.error('Cancel invoice failed', err);
        this.notification.error('Lỗi', 'Không thể hủy hóa đơn.');
      },
    });
  }

  confirmCashPayment(): void {
    this.modal.confirm({
      nzTitle: 'Ghi nhận tiền mặt',
      nzContent: 'Xác nhận ghi nhận thanh toán tiền mặt cho hóa đơn này?',
      nzOkText: 'Ghi nhận',
      nzOnOk: () => this.cash(),
      nzCancelText: 'Hủy',
    });
  }

  private cash(): void {
    this.actionLoading = true;
    this.proxy.cash(this.invoiceId, this.cashNote || undefined).subscribe({
      next: () => {
        this.actionLoading = false;
        this.notification.success('Thành công', 'Đã ghi nhận thanh toán tiền mặt.');
        this.load();
      },
      error: (err: any) => {
        this.actionLoading = false;
        console.error('Cash payment failed', err);
        this.notification.error('Lỗi', 'Không thể ghi nhận tiền mặt.');
      },
    });
  }
}

