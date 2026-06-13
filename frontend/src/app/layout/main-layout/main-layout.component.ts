import { Component, OnInit, OnDestroy, Optional, Inject } from '@angular/core';
import { Router, RouterLink, RouterOutlet } from '@angular/router';
import { NzIconModule } from 'ng-zorro-antd/icon';
import { NzLayoutModule } from 'ng-zorro-antd/layout';
import { NzMenuModule } from 'ng-zorro-antd/menu';
import { CommonModule } from '@angular/common';
import { NzAvatarModule } from 'ng-zorro-antd/avatar';
import { Subscription } from 'rxjs';
import { AuthService } from '../../shared/auth.service';
import { resolveAssetUrl } from '../../shared/resolve-asset-url';
import { API_BASE_URL } from '../../shared/service-proxies';

@Component({
  selector: 'app-main-layout',
  imports: [CommonModule, RouterLink, RouterOutlet, NzIconModule, NzLayoutModule, NzMenuModule, NzAvatarModule],
  templateUrl: './main-layout.component.html',
  styleUrls: ['./main-layout.component.css']
})
export class MainLayoutComponent implements OnInit, OnDestroy {
  showUserMenu = false;
  showRoomRentalMenu = false;
  showContractMenu = false;
  showRoleGroupMenu = false;
  showInvoicesMenu = false;
  showMyInvoicesMenu = false;
  showPaymentSubmissionsMenu = false;
  isCollapsed = false;
  userName = 'User';
  avatarUrl?: string;
  private sub?: Subscription;
  baseUrl?: string;
  constructor(private router: Router, private authService: AuthService, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
    this.baseUrl = baseUrl;
  }

  ngOnInit(): void {
    this.showUserMenu = this.hasResourcePermission('User');
    this.showRoomRentalMenu = this.hasResourcePermission('RoomRental');
    this.showContractMenu = this.hasResourcePermission('Contract');
    this.showRoleGroupMenu = this.hasResourcePermission('RoleGroups');
    // Try common resource/controller names; backend permission naming can vary.
    this.showInvoicesMenu = this.hasResourcePermission('Invoices') || this.hasResourcePermission('Invoice');
    this.showMyInvoicesMenu = this.hasResourcePermission('MyInvoices') || this.hasResourcePermission('MyInvoice');
    this.showPaymentSubmissionsMenu = this.hasResourcePermission('PaymentSubmissions') || this.hasResourcePermission('PaymentSubmission');
    this.sub = this.authService.currentUser$.subscribe(u => {
      if (u) {
        this.userName = u.username || 'User';
        this.avatarUrl = u.avatarUrl;
      } else {
        this.userName = 'User';
        this.avatarUrl = undefined;
      }
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  private getPermissions(): string[] {
    try {
      const raw = localStorage.getItem('role_group_permissions');
      if (!raw) return [];
      const parsed = JSON.parse(raw);
      // parsed could be array of strings or array of objects with a name/property
      if (Array.isArray(parsed)) {
        return parsed.map(p => {
          if (!p) return '';
          if (typeof p === 'string') return p;
          // try common keys if object
          if (typeof p === 'object') {
            return (p.permission || p.name || p.code || '').toString();
          }
          return p.toString();
        }).filter(Boolean);
      }
      // if single string
      if (typeof parsed === 'string') return [parsed];
    } catch {
      // fall through
    }
    return [];
  }

  private hasResourcePermission(resource: string): boolean {
    const perms = this.getPermissions();
    const prefix = resource.toLowerCase() + '.';
    return perms.some(p => {
      const s = (p || '').toLowerCase();
      if (s === resource.toLowerCase() || s.startsWith(prefix)) {
        return true;
      }
      // Fallback: some backends use permission names like `can_view_invoice`,
      // `invoice_read`, `my_invoices`, etc. Use a contains check to avoid
      // hiding menus when naming conventions differ.
      const r = resource.toLowerCase();
      const rSingular = r.endsWith('s') ? r.slice(0, -1) : r;
      return s.includes(r) || s.includes(rSingular);
    });
  }

  onLogout(): void{
    this.authService.logout();
  }

  /** Ảnh từ API (path tương đối), Cloudinary/Google (URL đầy đủ), … */
  avatarSrc(): string | undefined {
    if (!this.avatarUrl) {
      return undefined;
    }
    const url = resolveAssetUrl(this.baseUrl, this.avatarUrl);
    return url || undefined;
  }
}