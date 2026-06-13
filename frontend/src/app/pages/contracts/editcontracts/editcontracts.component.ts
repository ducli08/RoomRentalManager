import { Component, EventEmitter, Inject, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzDatePickerModule } from 'ng-zorro-antd/date-picker';
import { NZ_MODAL_DATA } from 'ng-zorro-antd/modal';
import { forkJoin, of } from 'rxjs';
import {
  ContractApiService,
  ContractDto,
  CreateOrEditContractDto,
  SelectListItem,
  StatusContract,
} from '../../../shared/services';
import { SelectListItemService } from '../../../shared/get-select-list-item.service';
import { CategoryCacheService } from '../../../shared/category-cache.service';

@Component({
  selector: 'app-editcontracts',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NzFormModule, NzInputModule, NzSelectModule, NzDatePickerModule],
  templateUrl: './editcontracts.component.html',
})
export class EditContractsComponent implements OnInit {
  editContractForm!: FormGroup;
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
    @Inject(NZ_MODAL_DATA) public data: { contractData: ContractDto },
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
      this.patchForm(this.data.contractData);
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
      { label: 'Trạng thái', key: 'statusContract', type: 'select', options: () => this.lstStatusContracts, placeholder: 'Chọn trạng thái', validators: [Validators.required] },
    ];

    const formControls: { [key: string]: any } = { id: [''] };
    this.controlRequestArray.forEach(control => {
      formControls[control.key] = [null, control.validators || []];
    });
    this.editContractForm = this.fb.group(formControls);
  }

  patchForm(contract: ContractDto): void {
    this.editContractForm.patchValue({
      id: contract.id,
      roomRentalId: contract.roomRentalId?.toString(),
      tenantId: contract.tenantId?.toString(),
      startDate: contract.startDate ? new Date(contract.startDate) : null,
      endDate: contract.endDate ? new Date(contract.endDate) : null,
      depositAmout: contract.depositAmout,
      monthlyRent: contract.monthlyRent,
      statusContract: contract.statusContract?.toString(),
    });
  }

  onSubmit(): void {
    if (!this.editContractForm.valid) return;

    const raw = this.editContractForm.value;
    const dto = new CreateOrEditContractDto();
    dto.id = Number(raw.id);
    dto.roomRentalId = Number(raw.roomRentalId);
    dto.tenantId = Number(raw.tenantId);
    dto.startDate = raw.startDate;
    dto.endDate = raw.endDate;
    dto.depositAmout = String(raw.depositAmout);
    dto.monthlyRent = String(raw.monthlyRent);
    dto.statusContract = Number(raw.statusContract) as StatusContract;

    this.contractApi.createOrEditContract(dto).subscribe({
      next: () => this.saved.emit(),
      error: (err) => console.error(err),
    });
  }
}
