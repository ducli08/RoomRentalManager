import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NzTableModule } from 'ng-zorro-antd/table';
import { NzButtonModule } from 'ng-zorro-antd/button';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzGridModule } from 'ng-zorro-antd/grid';
import { NzModalModule, NzModalService } from 'ng-zorro-antd/modal';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { forkJoin, of, take } from 'rxjs';
import {
  SelectListItem,
  UtilityReadingApiService,
  UtilityReadingDto,
  UtilityReadingFilterDto,
  UtilityReadingFilterDtoPagedRequestDto,
  UtilityReadingStatus,
} from '../../shared/services';
import { SelectListItemService } from '../../shared/get-select-list-item.service';
import { CategoryCacheService } from '../../shared/category-cache.service';
import { utilityReadingStatusLabel } from '../../shared/utility-reading-status-label';
import { CreateUtilityReadingComponent } from './create-utility-reading/create-utility-reading.component';
import { EditUtilityReadingComponent } from './edit-utility-reading/edit-utility-reading.component';
import { ViewUtilityReadingComponent } from './view-utility-reading/view-utility-reading.component';

@Component({
  selector: 'app-utility-readings',
  standalone: true,
  imports: [
    CommonModule, FormsModule, NzTableModule, NzButtonModule, NzIconModule,
    NzFormModule, NzSelectModule, NzInputModule, NzModalModule, NzGridModule,
  ],
  templateUrl: './utility-readings.component.html',
  styleUrls: ['./utility-readings.component.css'],
})
export class UtilityReadingsComponent implements OnInit {
  rolePermissions: string[] = [];
  loading = false;
  readings: readonly UtilityReadingDto[] = [];
  filterDto: UtilityReadingFilterDto = new UtilityReadingFilterDto();
  requestDto: UtilityReadingFilterDtoPagedRequestDto = new UtilityReadingFilterDtoPagedRequestDto();
  total = 0;
  pageIndex = 1;
  pageSize = 10;
  lstRoomRentals: SelectListItem[] = [];
  lstTenants: SelectListItem[] = [];
  lstMonths: SelectListItem[] = Array.from({ length: 12 }, (_, i) => new SelectListItem({ value: String(i + 1), text: `Tháng ${i + 1}` }));
  lstYears: SelectListItem[] = [];
  lstStatus: SelectListItem[] = [
    new SelectListItem({ value: String(UtilityReadingStatus.Confirmed), text: 'Đã xác nhận' }),
    new SelectListItem({ value: String(UtilityReadingStatus.InvoiceGenerated), text: 'Đã tạo hóa đơn' }),
  ];

  constructor(
    private utilityApi: UtilityReadingApiService,
    private getSelectListItem: SelectListItemService,
    private modalService: NzModalService,
    private memoryCache: CategoryCacheService,
    private notification: NzNotificationService,
  ) {}

  ngOnInit(): void {
    const perms = localStorage.getItem('role_group_permissions');
    try {
      this.rolePermissions = perms ? JSON.parse(perms) : [];
    } catch {
      this.rolePermissions = [];
    }

    const currentYear = new Date().getFullYear();
    this.lstYears = Array.from({ length: 10 }, (_, i) => {
      const y = currentYear - 5 + i;
      return new SelectListItem({ value: String(y), text: String(y) });
    });

    this.requestDto.filter = this.filterDto;
    this.requestDto.page = this.pageIndex;
    this.requestDto.pageSize = this.pageSize;
    this.requestDto.sortBy = '';
    this.requestDto.sortOrder = '';

    forkJoin([
      this.memoryCache.get<SelectListItem[]>('roomRental') ? of(this.memoryCache.get<SelectListItem[]>('roomRental')!) : this.getSelectListItem.getSelectListItems('roomRental', ''),
      this.memoryCache.get<SelectListItem[]>('tenant') ? of(this.memoryCache.get<SelectListItem[]>('tenant')!) : this.getSelectListItem.getSelectListItems('tenant', ''),
    ]).subscribe(([rooms, tenants]) => {
      this.lstRoomRentals = rooms ?? [];
      this.lstTenants = tenants ?? [];
    });

    this.loadData();
  }

  loadData(): void {
    const filterToSend = new UtilityReadingFilterDto();
    Object.assign(filterToSend, this.filterDto);
    if (filterToSend.month !== undefined) filterToSend.month = Number(filterToSend.month) as any;
    if (filterToSend.year !== undefined) filterToSend.year = Number(filterToSend.year) as any;
    if (filterToSend.contractId !== undefined) filterToSend.contractId = Number(filterToSend.contractId) as any;
    if (filterToSend.roomRentalId !== undefined) filterToSend.roomRentalId = Number(filterToSend.roomRentalId) as any;
    if (filterToSend.tenantId !== undefined) filterToSend.tenantId = Number(filterToSend.tenantId) as any;
    if (filterToSend.status !== undefined) filterToSend.status = Number(filterToSend.status) as any;

    this.requestDto.filter = filterToSend;
    this.requestDto.page = this.pageIndex;
    this.requestDto.pageSize = this.pageSize;
    this.requestDto.sortBy = '';
    this.requestDto.sortOrder = '';

    this.loading = true;
    this.utilityApi.search(this.requestDto).subscribe({
      next: (res) => {
        this.readings = res.listItem ?? [];
        this.total = res.totalCount ?? 0;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.notification.error('Lỗi', 'Không thể tải danh sách chỉ số.');
      },
    });
  }

  onPageChange(page: number): void {
    this.pageIndex = page;
    this.loadData();
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.pageIndex = 1;
    this.loadData();
  }

  getStatusText(status?: UtilityReadingStatus): string {
    return utilityReadingStatusLabel(status);
  }

  exportExcel(): void {
    const filterToSend = new UtilityReadingFilterDto();
    Object.assign(filterToSend, this.filterDto);
    this.utilityApi.exportExcel(filterToSend).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `utility-readings-${Date.now()}.xlsx`;
        a.click();
        URL.revokeObjectURL(url);
      },
      error: () => this.notification.error('Lỗi', 'Không thể xuất Excel.'),
    });
  }

  openCreateModal(): void {
    const modal = this.modalService.create({
      nzTitle: 'Nhập chỉ số điện nước',
      nzContent: CreateUtilityReadingComponent,
      nzWidth: '720px',
      nzStyle: { height: '80vh' },
      nzBodyStyle: { overflow: 'auto', maxHeight: 'calc(80vh - 55px)' },
      nzFooter: [
        { label: 'Hủy', onClick: () => modal.close() },
        {
          label: 'Lưu',
          type: 'primary',
          disabled: (ci) => !ci?.canSubmit,
          onClick: (ci) => ci?.onSubmit(),
        },
      ],
    });

    const content = modal.getContentComponent() as CreateUtilityReadingComponent | null;
    content?.saved.pipe(take(1)).subscribe(() => {
      this.loadData();
      this.notification.success('Thành công', 'Đã lưu chỉ số và tạo hóa đơn.');
      modal.close();
    });
  }

  openEditModal(reading: UtilityReadingDto): void {
    if (reading.isLockedByPayment) {
      this.notification.warning('Cảnh báo', 'Không thể sửa vì hóa đơn đã có thanh toán.');
      return;
    }

    const modal = this.modalService.create({
      nzTitle: 'Sửa chỉ số điện nước',
      nzContent: EditUtilityReadingComponent,
      nzData: { reading },
      nzWidth: '720px',
      nzFooter: [
        { label: 'Hủy', onClick: () => modal.close() },
        {
          label: 'Lưu',
          type: 'primary',
          disabled: (ci) => !ci?.canSubmit,
          onClick: (ci) => ci?.onSubmit(),
        },
      ],
    });

    modal.afterOpen.pipe(take(1)).subscribe(() => {
      const edit = modal.getContentComponent() as EditUtilityReadingComponent | undefined;
      edit?.saved.pipe(take(1)).subscribe(() => {
        this.loadData();
        this.notification.success('Thành công', 'Đã cập nhật chỉ số và hóa đơn.');
        modal.close();
      });
    });
  }

  openViewModal(reading: UtilityReadingDto): void {
    this.modalService.create({
      nzTitle: 'Chi tiết chỉ số',
      nzContent: ViewUtilityReadingComponent,
      nzData: { reading },
      nzWidth: '640px',
      nzFooter: [{ label: 'Đóng', type: 'primary', onClick: () => this.modalService.closeAll() }],
    });
  }

  hasPermission(permission: string): boolean {
    return this.rolePermissions.includes(permission);
  }
}
