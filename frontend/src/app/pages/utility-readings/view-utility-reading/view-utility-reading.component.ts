import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NzDescriptionsModule } from 'ng-zorro-antd/descriptions';
import { NZ_MODAL_DATA } from 'ng-zorro-antd/modal';
import { UtilityReadingDto } from '../../../shared/services';
import { utilityReadingStatusLabel } from '../../../shared/utility-reading-status-label';
import { formatBillingPeriod } from '../../../shared/billing-period-format';

@Component({
  selector: 'app-view-utility-reading',
  standalone: true,
  imports: [CommonModule, NzDescriptionsModule],
  templateUrl: './view-utility-reading.component.html',
})
export class ViewUtilityReadingComponent implements OnInit {
  reading!: UtilityReadingDto;

  constructor(@Inject(NZ_MODAL_DATA) public data: { reading: UtilityReadingDto }) {}

  ngOnInit(): void {
    this.reading = this.data.reading;
  }

  getStatusText(): string {
    return utilityReadingStatusLabel(this.reading.status);
  }

  formatBillingPeriod = formatBillingPeriod;
}
