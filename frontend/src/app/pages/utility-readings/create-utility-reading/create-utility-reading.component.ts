import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzSelectModule } from 'ng-zorro-antd/select';
import { NzGridModule } from 'ng-zorro-antd/grid';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import {
  CreateOrEditUtilityReadingDto,
  SelectListItem,
  UtilityReadingApiService,
  UtilityReadingPrepareDto,
} from '../../../shared/services';
import { SelectListItemService } from '../../../shared/get-select-list-item.service';

@Component({
  selector: 'app-create-utility-reading',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NzFormModule, NzInputModule, NzSelectModule, NzGridModule, NzAlertModule],
  templateUrl: './create-utility-reading.component.html',
})
export class CreateUtilityReadingComponent implements OnInit {
  form!: FormGroup;
  lstContracts: SelectListItem[] = [];
  lstMonths: SelectListItem[] = Array.from({ length: 12 }, (_, i) => new SelectListItem({ value: String(i + 1), text: `Tháng ${i + 1}` }));
  lstYears: SelectListItem[] = [];
  prepare: UtilityReadingPrepareDto | null = null;
  electricUsage = 0;
  waterUsage = 0;
  @Output() saved = new EventEmitter<void>();

  constructor(
    private fb: FormBuilder,
    private utilityApi: UtilityReadingApiService,
    private getSelectListItem: SelectListItemService,
  ) {}

  ngOnInit(): void {
    const currentYear = new Date().getFullYear();
    this.lstYears = Array.from({ length: 5 }, (_, i) => {
      const y = currentYear - 2 + i;
      return new SelectListItem({ value: String(y), text: String(y) });
    });

    this.form = this.fb.group({
      contractId: [null, Validators.required],
      month: [new Date().getMonth() + 1, Validators.required],
      year: [currentYear, Validators.required],
      newElectricIndex: [null, Validators.required],
      newWaterIndex: [null, Validators.required],
    });

    this.loadActiveContracts();

    this.form.valueChanges.pipe(debounceTime(300), distinctUntilChanged()).subscribe(() => {
      this.loadPrepare();
      this.recalcUsage();
    });
  }

  loadActiveContracts(): void {
    this.getSelectListItem.getSelectListItems('activeContract', '').subscribe({
      next: (items) => {
        this.lstContracts = items ?? [];
      },
    });
  }

  loadPrepare(): void {
    const { contractId, month, year } = this.form.value;
    if (!contractId || !month || !year) {
      this.prepare = null;
      return;
    }
    this.utilityApi.prepare(Number(contractId), Number(month), Number(year)).subscribe({
      next: (p) => {
        this.prepare = p;
        this.recalcUsage();
      },
      error: () => { this.prepare = null; },
    });
  }

  recalcUsage(): void {
    const newE = Number(this.form.value.newElectricIndex) || 0;
    const newW = Number(this.form.value.newWaterIndex) || 0;
    const oldE = this.prepare?.oldElectricIndex ?? 0;
    const oldW = this.prepare?.oldWaterIndex ?? 0;
    this.electricUsage = Math.max(0, newE - oldE);
    this.waterUsage = Math.max(0, newW - oldW);
  }

  get canSubmit(): boolean {
    return this.form.valid && (this.prepare?.canSave ?? false);
  }

  onSubmit(): void {
    if (!this.canSubmit) return;
    const raw = this.form.value;
    const dto = new CreateOrEditUtilityReadingDto();
    dto.contractId = Number(raw.contractId);
    dto.month = Number(raw.month);
    dto.year = Number(raw.year);
    dto.newElectricIndex = Number(raw.newElectricIndex);
    dto.newWaterIndex = Number(raw.newWaterIndex);

    this.utilityApi.create(dto).subscribe({
      next: () => this.saved.emit(),
      error: (err) => console.error(err),
    });
  }
}
