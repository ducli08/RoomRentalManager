import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { forkJoin, of } from 'rxjs';
import {
  ContractApiService,
  CreateOrEditContractDto,
  SelectListItem,
  StatusContract,
} from '../../../shared/services';
import { SelectListItemService } from '../../../shared/get-select-list-item.service';
import { CategoryCacheService } from '../../../shared/category-cache.service';

@Component({
  selector: 'app-createcontracts',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NzFormModule, NzInputModule, NzSelectModule, NzDatePickerModule],
  templateUrl: './createcontracts.component.html',
})
export class CreateContractsComponent implements OnInit {
  createContractForm!: FormGroup;
  lstRoomRentals: SelectListItem[] = [];
  lstTenants: SelectListItem[] = [];
  lstStatusContracts: SelectListItem[] = [];
  @Output() saved = new EventEmitter<void>();

  controlRequestArray: Array<{
    label: string;
    key: string;
    type: string;
    options?: () => SelectListItem[];
    placeholder?: string;
    validators?: any[];
  }> = [];

  constructor(
    private fb: FormBuilder,
    private contractApi: ContractApiService,
    private getSelectListItem: SelectListItemService,
    private memoryCache: CategoryCacheService,
  ) {}

  ngOnInit(): void {
    forkJoin([
      this.memoryCache.get<SelectListItem[]>('roomRental')
        ? of(this.memoryCache.get<SelectListItem[]>('roomRental')!)
        : this.getSelectListItem.getSelectListItems('roomRental', ''),
      this.memoryCache.get<SelectListItem[]>('tenant')
        ? of(this.memoryCache.get<SelectListItem[]>('tenant')!)
        : this.getSelectListItem.getSelectListItems('tenant', ''),
      this.memoryCache.get<SelectListItem[]>('statusContract')
        ? of(this.memoryCache.get<SelectListItem[]>('statusContract')!)
        : this.getSelectListItem.getEnumSelectListItems('StatusContract'),
    ]).subscribe(([rooms, tenants, statuses]) => {
      this.lstRoomRentals = rooms ?? [];
      this.lstTenants = tenants ?? [];
      this.lstStatusContracts = statuses ?? [];
      this.initializeFormControls();
    });
  }

  initializeFormControls(): void {
    this.controlRequestArray = [
      { label: 'Phòng trọ', key: 'roomRentalId', type: 'select', options: () => this.lstRoomRentals, placeholder: 'Chọn phòng', validators: [Validators.required] },
      { label: 'Người thuê', key: 'tenantId', type: 'select', options: () => this.lstTenants, placeholder: 'Chọn người thuê', validators: [Validators.required] },
      { label: 'Ngày bắt đầu', key: 'startDate', type: 'date', placeholder: 'Chọn ngày bắt đầu', validators: [Validators.required] },
      { label: 'Ngày kết thúc', key: 'endDate', type: 'date', placeholder: 'Chọn ngày kết thúc', validators: [Validators.required] },
      { label: 'Tiền cọc', key: 'depositAmout', type: 'number', placeholder: 'Nhập tiền cọc', validators: [Validators.required] },
      { label: 'Tiền thuê hàng tháng', key: 'monthlyRent', type: 'number', placeholder: 'Nhập tiền thuê', validators: [Validators.required] },
      { label: 'Đơn giá điện (VND/kWh)', key: 'electricUnitPrice', type: 'number', placeholder: '4000', validators: [Validators.required] },
      { label: 'Đơn giá nước (VND/m³)', key: 'waterUnitPrice', type: 'number', placeholder: '30000', validators: [Validators.required] },
      { label: 'Tiền rác/năm', key: 'garbageFeePerYear', type: 'number', placeholder: '150000', validators: [Validators.required] },
      { label: 'Trạng thái', key: 'statusContract', type: 'select', options: () => this.lstStatusContracts, placeholder: 'Chọn trạng thái', validators: [Validators.required] },
    ];

    const formControls: { [key: string]: any } = {};
    this.controlRequestArray.forEach(control => {
      const defaultValue = control.key === 'electricUnitPrice' ? 4000
        : control.key === 'waterUnitPrice' ? 30000
        : control.key === 'garbageFeePerYear' ? 150000
        : null;
      formControls[control.key] = [defaultValue, control.validators || []];
    });
    this.createContractForm = this.fb.group(formControls);
  }

  onSubmit(): void {
    if (!this.createContractForm.valid) return;

    const raw = this.createContractForm.value;
    const dto = new CreateOrEditContractDto();
    dto.id = 0;
    dto.roomRentalId = Number(raw.roomRentalId);
    dto.tenantId = Number(raw.tenantId);
    dto.startDate = raw.startDate;
    dto.endDate = raw.endDate;
    dto.depositAmout = String(raw.depositAmout);
    dto.monthlyRent = String(raw.monthlyRent);
    dto.electricUnitPrice = String(raw.electricUnitPrice);
    dto.waterUnitPrice = String(raw.waterUnitPrice);
    dto.garbageFeePerYear = String(raw.garbageFeePerYear);
    dto.statusContract = Number(raw.statusContract) as StatusContract;

    this.contractApi.createOrEditContract(dto).subscribe({
      next: () => {
        this.createContractForm.reset();
        this.saved.emit();
      },
      error: (err) => console.error(err),
    });
  }
}
