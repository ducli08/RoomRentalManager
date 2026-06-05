import { Routes } from '@angular/router';
import { MainLayoutComponent } from './main-layout.component';
import { AuthGuard } from '../../shared/auth-guard.services';

export const MAIN_ROUTES: Routes = [
  {
    path: '',
    component: MainLayoutComponent,
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'roomrentals' },
        { path: 'roomrentals', loadChildren: () => import('../../pages/roomrentals/roomrentals.routes').then(m => m.ROOMRENTALS_ROUTES), canActivate:[AuthGuard] },
        { path: 'my-invoices', loadChildren: () => import('../../pages/my-invoices/my-invoices.routes').then(m => m.MY_INVOICES_ROUTES), canActivate:[AuthGuard] },
        { path: 'invoices', loadChildren: () => import('../../pages/invoices/invoices.routes').then(m => m.INVOICES_ROUTES), canActivate:[AuthGuard] },
        { path: 'payment-submissions', loadChildren: () => import('../../pages/payment-submissions/payment-submissions.routes').then(m => m.PAYMENT_SUBMISSIONS_ROUTES), canActivate:[AuthGuard] },
        { path: 'users', loadChildren: () => import('../../pages/users/users.routes').then(m => m.USERS_ROUTES), canActivate:[AuthGuard] },
        { path: 'rolegroups', loadChildren: () => import('../../pages/rolegroups/rolegroups.routes').then(m => m.ROLEGROUP_ROUTES), canActivate:[AuthGuard] },
    ]
  }
];