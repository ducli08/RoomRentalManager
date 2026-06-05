import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzCardModule } from 'ng-zorro-antd/card';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { CreateOrEditInvoiceDto, ServiceProxy } from '../../../shared/services';

@Component({
  selector: 'app-invoice-form',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
    NzCardModule,
    NzFormModule,
    NzInputModule,
    NzDatePickerModule,
    NzButtonModule,
  ],
  templateUrl: './invoice-form.component.html',
  styleUrls: ['./invoice-form.component.css'],
})
export class InvoiceFormComponent implements OnInit {
  saving = false;
  loading = false;

  isEdit = false;
  invoiceId?: number;

  dto: CreateOrEditInvoiceDto = new CreateOrEditInvoiceDto();

  constructor(
    private proxy: ServiceProxy,
    private route: ActivatedRoute,
    private router: Router,
    private notification: NzNotificationService
  ) {}

  ngOnInit(): void {
    const idStr = this.route.snapshot.paramMap.get('id');
    if (idStr) {
      const id = Number(idStr);
      if (id && !Number.isNaN(id)) {
        this.isEdit = true;
        this.invoiceId = id;
        this.loadForEdit(id);
        return;
      }
    }

    // defaults for create
    const now = new Date();
    this.dto.invoiceDate = now;
    this.dto.dueDate = new Date(now.getTime() + 7 * 24 * 60 * 60 * 1000);
    this.dto.totalAmount = 0;
  }

  private loadForEdit(id: number): void {
    this.loading = true;
    this.proxy.invoicesGET(id).subscribe({
      next: (inv: any) => {
        this.dto = new CreateOrEditInvoiceDto({
          id: inv.id,
          contractId: inv.contractId ?? 0,
          invoiceDate: inv.invoiceDate ?? new Date(),
          dueDate: inv.dueDate ?? new Date(),
          totalAmount: inv.totalAmount ?? 0,
        });
        this.loading = false;
      },
      error: (err: any) => {
        this.loading = false;
        console.error('Load invoice failed', err);
        this.notification.error('Lỗi', 'Không thể tải hóa đơn để chỉnh sửa.');
      },
    });
  }

  save(): void {
    if (!this.dto.contractId || this.dto.contractId <= 0) {
      this.notification.warning('Không hợp lệ', 'Contract ID là bắt buộc.');
      return;
    }
    if (!this.dto.invoiceDate || !this.dto.dueDate) {
      this.notification.warning('Không hợp lệ', 'Ngày hóa đơn và hạn thanh toán là bắt buộc.');
      return;
    }
    if (Number(this.dto.totalAmount) <= 0) {
      this.notification.warning('Không hợp lệ', 'Tổng tiền phải > 0.');
      return;
    }

    this.saving = true;
    const req = this.dto;

    const done = () => (this.saving = false);
    if (this.isEdit && this.invoiceId) {
      this.proxy.invoicesPUT(this.invoiceId, req).subscribe({
        next: () => {
          done();
          this.notification.success('Thành công', 'Đã cập nhật hóa đơn.');
          this.router.navigate(['/main/invoices', this.invoiceId]);
        },
        error: (err: any) => {
          done();
          console.error('Update invoice failed', err);
          this.notification.error('Lỗi', 'Không thể cập nhật hóa đơn.');
        },
      });
    } else {
      this.proxy.invoicesPOST(req).subscribe({
        next: () => {
          done();
          this.notification.success('Thành công', 'Đã tạo hóa đơn.');
          this.router.navigate(['/main/invoices']);
        },
        error: (err: any) => {
          done();
          console.error('Create invoice failed', err);
          this.notification.error('Lỗi', 'Không thể tạo hóa đơn.');
        },
      });
    }
  }
}

