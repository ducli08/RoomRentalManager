import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzGridModule } from 'ng-zorro-antd/grid';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzTagModule } from 'ng-zorro-antd/tag';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import {
  InvoiceDto,
  InvoiceDtoPagedResultDto,
  InvoiceFilterDto,
  InvoiceFilterDtoPagedRequestDto,
  InvoiceStatus,
  ServiceProxy,
} from '../../shared/services';
import { invoiceStatusLabel } from '../../shared/invoice-status-label';

type InvoiceStatusOption = { value: InvoiceStatus; label: string };

@Component({
  selector: 'app-invoices',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NzTableModule,
    NzButtonModule,
    NzSelectModule,
    NzFormModule,
    NzGridModule,
    NzInputModule,
    NzTagModule,
  ],
  templateUrl: './invoices.component.html',
  styleUrls: ['./invoices.component.css'],
})
export class InvoicesComponent implements OnInit {
  loading = false;

  invoices: readonly InvoiceDto[] = [];
  total = 0;
  pageIndex = 1;
  pageSize = 10;

  filter: InvoiceFilterDto = new InvoiceFilterDto();

  statusOptions: InvoiceStatusOption[] = [
    { value: InvoiceStatus._1, label: 'Nháp' },
    { value: InvoiceStatus._2, label: 'Đã phát hành' },
    { value: InvoiceStatus._3, label: 'Thanh toán một phần' },
    { value: InvoiceStatus._4, label: 'Đã thanh toán' },
    { value: InvoiceStatus._5, label: 'Đã hủy' },
  ];

  constructor(
    private proxy: ServiceProxy,
    private router: Router,
    private notification: NzNotificationService
  ) {}

  ngOnInit(): void {
    this.filter.contractId = undefined;
    this.filter.isOverdue = undefined;
    this.filter.status = undefined as any;
    this.search();
  }

  statusLabel(s?: InvoiceStatus): string {
    return invoiceStatusLabel(s);
  }

  statusTagColor(s?: InvoiceStatus): string {
    switch (s as unknown as number) {
      case 1:
        return 'default';
      case 2:
        return 'blue';
      case 3:
        return 'orange';
      case 4:
        return 'green';
      case 5:
        return 'red';
      default:
        return 'default';
    }
  }

  money(v?: number): string {
    const n = Number(v ?? 0);
    return new Intl.NumberFormat('vi-VN').format(n) + ' ₫';
  }

  onPageChange(page: number): void {
    this.pageIndex = page;
    this.search();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.pageIndex = 1;
    this.search();
  }

  search(): void {
    const req = new InvoiceFilterDtoPagedRequestDto();
    req.page = this.pageIndex;
    req.pageSize = this.pageSize;
    req.sortBy = '';
    req.sortOrder = '';
    req.filter = this.filter;

    this.loading = true;
    this.proxy.search(req).subscribe({
      next: (res: InvoiceDtoPagedResultDto) => {
        this.invoices = res.listItem ?? [];
        this.total = res.totalCount ?? 0;
        this.loading = false;
      },
      error: (err: any) => {
        this.loading = false;
        console.error('Invoices search failed', err);
        this.notification.error('Lỗi', 'Không thể tải danh sách hóa đơn.');
      },
    });
  }

  resetFilters(): void {
    this.filter = new InvoiceFilterDto();
    this.filter.contractId = undefined;
    this.filter.isOverdue = undefined;
    this.filter.status = undefined as any;
    this.pageIndex = 1;
    this.search();
  }

  openDetail(inv: InvoiceDto): void {
    if (!inv.id) return;
    this.router.navigate(['/main/invoices', inv.id]);
  }

  openCreate(): void {
    this.router.navigate(['/main/invoices/create']);
  }
}

