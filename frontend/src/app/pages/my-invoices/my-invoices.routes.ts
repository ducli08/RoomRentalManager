import { Routes } from '@angular/router';
import { MyInvoicesComponent } from './my-invoices.component';
import { MyInvoiceDetailComponent } from './my-invoice-detail/my-invoice-detail.component';

export const MY_INVOICES_ROUTES: Routes = [
  { path: '', component: MyInvoicesComponent },
  { path: ':id', component: MyInvoiceDetailComponent },
];

