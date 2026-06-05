import { Routes } from '@angular/router';
import { InvoicesComponent } from './invoices.component';
import { InvoiceFormComponent } from './invoice-form/invoice-form.component';
import { InvoiceDetailComponent } from './invoice-detail/invoice-detail.component';

export const INVOICES_ROUTES: Routes = [
  { path: '', component: InvoicesComponent },
  { path: 'create', component: InvoiceFormComponent },
  { path: ':id/edit', component: InvoiceFormComponent },
  { path: ':id', component: InvoiceDetailComponent },
];

