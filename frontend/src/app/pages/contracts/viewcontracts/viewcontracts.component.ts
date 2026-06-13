import { Component, Inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NzDescriptionsModule } from 'ng-zorro-antd/descriptions';
import { NZ_MODAL_DATA } from 'ng-zorro-antd/modal';
import { ContractDto, SelectListItem } from '../../../shared/services';
import { SelectListItemService } from '../../../shared/get-select-list-item.service';

@Component({
  selector: 'app-viewcontracts',
  standalone: true,
  imports: [CommonModule, NzDescriptionsModule],
  templateUrl: './viewcontracts.component.html',
})
export class ViewContractsComponent implements OnInit {
  lstStatusContracts: SelectListItem[] = [];

  constructor(
    @Inject(NZ_MODAL_DATA) public data: { contractData: ContractDto },
    private getSelectListItem: SelectListItemService,
  ) {}

  ngOnInit(): void {
    this.getSelectListItem.getEnumSelectListItems('StatusContract').subscribe(items => {
      this.lstStatusContracts = items ?? [];
    });
  }

  get contract(): ContractDto {
    return this.data.contractData;
  }

  getStatusText(value?: number): string {
    const found = this.lstStatusContracts.find(item => Number(item.value) === Number(value));
    return found?.text ?? '';
  }
}
