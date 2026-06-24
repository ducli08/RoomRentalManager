import { Component, EventEmitter, Inject, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { NzFormModule } from 'ng-zorro-antd/form';
import { NzInputModule } from 'ng-zorro-antd/input';
import { NzAlertModule } from 'ng-zorro-antd/alert';
import { NZ_MODAL_DATA } from 'ng-zorro-antd/modal';
import { debounceTime, distinctUntilChanged } from 'rxjs';
import {
  CreateOrEditUtilityReadingDto,
  UtilityReadingApiService,
  UtilityReadingDto,
  UtilityReadingPrepareDto,
} from '../../../shared/services';

@Component({
  selector: 'app-edit-utility-reading',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NzFormModule, NzInputModule, NzAlertModule],
  templateUrl: './edit-utility-reading.component.html',
})
export class EditUtilityReadingComponent implements OnInit {
  form!: FormGroup;
  prepare: UtilityReadingPrepareDto | null = null;
  electricUsage = 0;
  @Output() saved = new EventEmitter<void>();

  constructor(
    private fb: FormBuilder,
    private utilityApi: UtilityReadingApiService,
    @Inject(NZ_MODAL_DATA) public data: { reading: UtilityReadingDto },
  ) {}

  ngOnInit(): void {
    const r = this.data.reading;
    this.form = this.fb.group({
      newElectricIndex: [r.newElectricIndex, Validators.required],
    });

    this.loadPrepare();

    this.form.valueChanges.pipe(debounceTime(300), distinctUntilChanged()).subscribe(() => {
      this.recalcUsage();
    });
  }

  loadPrepare(): void {
    const r = this.data.reading;
    if (!r.contractId || !r.month || !r.year || !r.id) return;
    this.utilityApi.prepare(r.contractId, r.month, r.year, r.id).subscribe({
      next: (p) => {
        this.prepare = p;
        this.recalcUsage();
      },
    });
  }

  recalcUsage(): void {
    const newE = Number(this.form.value.newElectricIndex) || 0;
    const oldE = this.prepare?.oldElectricIndex ?? this.data.reading.oldElectricIndex ?? 0;
    this.electricUsage = Math.max(0, newE - oldE);
  }

  get canSubmit(): boolean {
    return this.form.valid && !this.data.reading.isLockedByPayment && (this.prepare?.canSave ?? true);
  }

  onSubmit(): void {
    if (!this.canSubmit || !this.data.reading.id) return;
    const dto = new CreateOrEditUtilityReadingDto();
    dto.id = this.data.reading.id;
    dto.contractId = this.data.reading.contractId;
    dto.month = this.data.reading.month;
    dto.year = this.data.reading.year;
    dto.newElectricIndex = Number(this.form.value.newElectricIndex);

    this.utilityApi.update(this.data.reading.id, dto).subscribe({
      next: () => this.saved.emit(),
      error: (err) => console.error(err),
    });
  }
}
