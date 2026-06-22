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
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NzModalModule, NzModalService } from 'ng-zorro-antd/modal';
import { NzNotificationService } from 'ng-zorro-antd/notification';
import { forkJoin, of, take } from 'rxjs';
import {
  ContractApiService,
  ContractDto,
  ContractFilterDto,
  ContractFilterDtoPagedRequestDto,
  SelectListItem,
  StatusContract,
} from '../../shared/services';
import { SelectListItemService } from '../../shared/get-select-list-item.service';
import { CategoryCacheService } from '../../shared/category-cache.service';
import { CreateContractsComponent } from './createcontracts/createcontracts.component';
import { EditContractsComponent } from './editcontracts/editcontracts.component';
import { ViewContractsComponent } from './viewcontracts/viewcontracts.component';

@Component({
  selector: 'app-contracts',
  standalone: true,
  imports: [
    CommonModule, FormsModule, NzTableModule, NzButtonModule, NzIconModule,
    NzFormModule, NzSelectModule, NzInputModule, NzGridModule, NzDatePickerModule, NzModalModule,
  ],
  templateUrl: './contracts.component.html',
  styleUrls: ['./contracts.component.css'],
})
export class ContractsComponent implements OnInit {
  rolePermissions: string[] = [];
  loading = false;
  contracts: readonly ContractDto[] = [];
  contractFilterDto: ContractFilterDto = new ContractFilterDto();
  contractRequestDto: ContractFilterDtoPagedRequestDto = new ContractFilterDtoPagedRequestDto();
  total = 0;
  pageIndex = 1;
  pageSize = 10;
  lstRoomRentals: SelectListItem[] = [];
  lstTenants: SelectListItem[] = [];
  lstStatusContracts: SelectListItem[] = [];
  filterPerRows: Array<Array<{
    label: string;
    key: keyof ContractFilterDto;
    type: string;
    options?: () => SelectListItem[];
    placeholder?: string;
  }>> = [];
  controlRequestArray: Array<{
    label: string;
    key: keyof ContractFilterDto;
    type: string;
    options?: () => SelectListItem[];
    placeholder?: string;
  }> = [];

  constructor(
    private contractApi: ContractApiService,
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

    this.contractFilterDto.roomRentalId = undefined;
    this.contractFilterDto.tenantId = undefined;
    this.contractFilterDto.statusContract = undefined;
    this.contractFilterDto.creatorUser = '';
    this.contractRequestDto.filter = this.contractFilterDto;
    this.contractRequestDto.page = this.pageIndex;
    this.contractRequestDto.pageSize = this.pageSize;
    this.contractRequestDto.sortBy = '';
    this.contractRequestDto.sortOrder = '';

    this.controlRequestArray = [
      { label: 'Phòng trọ', key: 'roomRentalId', type: 'select', options: () => this.lstRoomRentals, placeholder: 'Chọn phòng' },
      { label: 'Người thuê', key: 'tenantId', type: 'select', options: () => this.lstTenants, placeholder: 'Chọn người thuê' },
      { label: 'Trạng thái', key: 'statusContract', type: 'select', options: () => this.lstStatusContracts, placeholder: 'Chọn trạng thái' },
    ];
    this.filterPerRows = this.chunkArray(this.controlRequestArray, 3);

    const cachedRoomRentals = this.memoryCache.get<SelectListItem[]>('roomRental');
    const cachedTenants = this.memoryCache.get<SelectListItem[]>('tenant');
    const cachedStatus = this.memoryCache.get<SelectListItem[]>('statusContract');

    forkJoin([
      cachedRoomRentals ? of(cachedRoomRentals) : this.getSelectListItem.getSelectListItems('roomRental', ''),
      cachedTenants ? of(cachedTenants) : this.getSelectListItem.getSelectListItems('tenant', ''),
      cachedStatus ? of(cachedStatus) : this.getSelectListItem.getEnumSelectListItems('StatusContract'),
    ]).subscribe(([rooms, tenants, statuses]) => {
      this.lstRoomRentals = rooms ?? [];
      this.lstTenants = tenants ?? [];
      this.lstStatusContracts = statuses ?? [];
      if (!cachedRoomRentals) this.memoryCache.set('roomRental', this.lstRoomRentals);
      if (!cachedTenants) this.memoryCache.set('tenant', this.lstTenants);
      if (!cachedStatus) this.memoryCache.set('statusContract', this.lstStatusContracts);
    });

    this.getAllContracts();
  }

  getAllContracts(): void {
    const filterToSend = new ContractFilterDto();
    Object.assign(filterToSend, this.contractFilterDto);
    if (filterToSend.roomRentalId !== undefined) filterToSend.roomRentalId = Number(filterToSend.roomRentalId) as any;
    if (filterToSend.tenantId !== undefined) filterToSend.tenantId = Number(filterToSend.tenantId) as any;
    if (filterToSend.statusContract !== undefined) filterToSend.statusContract = Number(filterToSend.statusContract) as any;

    this.contractRequestDto.filter = filterToSend;
    this.contractRequestDto.page = this.pageIndex;
    this.contractRequestDto.pageSize = this.pageSize;

    this.loading = true;
    this.contractApi.getAllContract(this.contractRequestDto).subscribe({
      next: (response) => {
        this.contracts = response.listItem ?? [];
        this.total = response.totalCount ?? 0;
        this.loading = false;
      },
      error: (err) => {
        console.error(err);
        this.loading = false;
        this.notification.error('Lỗi', 'Không thể tải danh sách hợp đồng.');
      },
    });
  }

  onPageChange(page: number): void {
    this.pageIndex = page;
    this.getAllContracts();
  }

  onPageSizeChange(pageSize: number): void {
    this.pageSize = pageSize;
    this.pageIndex = 1;
    this.getAllContracts();
  }

  chunkArray<T>(array: T[], chunkSize: number): T[][] {
    const result: T[][] = [];
    for (let i = 0; i < array.length; i += chunkSize) {
      result.push(array.slice(i, i + chunkSize));
    }
    return result;
  }

  getStatusText(value?: StatusContract): string {
    const found = this.lstStatusContracts.find(item => Number(item.value) === Number(value));
    return found?.text ?? '';
  }

  openCreateModal(): void {
    const modal = this.modalService.create({
      nzTitle: 'Tạo hợp đồng mới',
      nzContent: CreateContractsComponent,
      nzWidth: '640px',
      nzStyle: { height: '75vh' },
      nzBodyStyle: { overflow: 'auto', maxHeight: 'calc(75vh - 55px)' },
      nzFooter: [
        { label: 'Hủy', onClick: () => modal.close() },
        {
          label: 'Lưu',
          type: 'primary',
          disabled: (ci) => !ci?.createContractForm.valid,
          onClick: (ci) => ci?.onSubmit(),
        },
      ],
    });

    const content = modal.getContentComponent() as CreateContractsComponent | null;
    content?.saved.pipe(take(1)).subscribe(() => {
      this.getAllContracts();
      this.notification.success('Thành công', 'Hợp đồng đã được tạo.');
      modal.close();
    });
  }

  openEditModal(contract: ContractDto): void {
    const modal = this.modalService.create({
      nzTitle: 'Chỉnh sửa hợp đồng',
      nzContent: EditContractsComponent,
      nzData: { contractData: contract },
      nzWidth: '640px',
      nzStyle: { height: '75vh' },
      nzBodyStyle: { overflow: 'auto', maxHeight: 'calc(75vh - 55px)' },
      nzFooter: [
        { label: 'Hủy', onClick: () => modal.close() },
        {
          label: 'Lưu',
          type: 'primary',
          disabled: (ci) => !ci?.editContractForm.valid,
          onClick: (ci) => ci?.onSubmit(),
        },
      ],
    });

    modal.afterOpen.pipe(take(1)).subscribe(() => {
      const edit = modal.getContentComponent() as EditContractsComponent | undefined;
      edit?.saved.pipe(take(1)).subscribe(() => {
        this.getAllContracts();
        this.notification.success('Thành công', 'Hợp đồng đã được cập nhật.');
        modal.close();
      });
    });
  }

  openViewModal(contract: ContractDto): void {
    this.modalService.create({
      nzTitle: 'Chi tiết hợp đồng',
      nzContent: ViewContractsComponent,
      nzData: { contractData: contract },
      nzWidth: '640px',
      nzFooter: [{ label: 'Đóng', type: 'primary', onClick: () => this.modalService.closeAll() }],
    });
  }

  openDeleteModal(id: number): void {
    this.modalService.confirm({
      nzTitle: 'Xóa hợp đồng',
      nzContent: 'Bạn có chắc chắn muốn xóa hợp đồng này?',
      nzOkText: 'Xóa',
      nzOkDanger: true,
      nzOnOk: () => this.contractApi.deleteContract(id).subscribe({
        next: () => {
          this.getAllContracts();
          this.notification.success('Thành công', 'Hợp đồng đã được xóa.');
        },
        error: (err) => {
          const msg = err?.error?.message ?? 'Không thể xóa hợp đồng.';
          this.notification.error('Lỗi', msg);
        },
      }),
      nzCancelText: 'Hủy',
    });
  }

  hasPermission(permission: string): boolean {
    return this.rolePermissions.includes(permission);
  }
}
