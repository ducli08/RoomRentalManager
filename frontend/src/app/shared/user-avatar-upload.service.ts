import { Inject, Injectable, Optional } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { API_BASE_URL, FileParameter } from './services';
import { UploadAvatarResult } from './upload-avatar-result';

@Injectable({ providedIn: 'root' })
export class UserAvatarUploadService {
  constructor(
    private http: HttpClient,
    @Optional() @Inject(API_BASE_URL) private baseUrl?: string
  ) {}

  uploadAvatar(files: FileParameter[]): Observable<UploadAvatarResult> {
    const url = (this.baseUrl ?? 'https://localhost:7246') + `/api/User/uploadAvatar`;
    const form = new FormData();
    for (const f of files) {
      if (!f?.data) continue;
      form.append('avatar', f.data as any, f.fileName ?? 'avatar');
    }
    return this.http.post<any>(url, form).pipe(
      map((res) => {
        const paths = Array.isArray(res?.paths) ? res.paths.map((x: any) => String(x)) : [];
        const publicIds = Array.isArray(res?.publicIds) ? res.publicIds.map((x: any) => String(x)) : [];
        return { paths, publicIds } as UploadAvatarResult;
      })
    );
  }
}

