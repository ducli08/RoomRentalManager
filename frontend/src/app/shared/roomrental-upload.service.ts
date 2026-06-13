import { Inject, Injectable, Optional } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map, Observable } from 'rxjs';
import { API_BASE_URL, FileParameter } from './services';

@Injectable({ providedIn: 'root' })
export class RoomRentalUploadService {
  constructor(
    private http: HttpClient,
    @Optional() @Inject(API_BASE_URL) private baseUrl?: string
  ) {}

  uploadImageDescription(files: FileParameter[]): Observable<string[]> {
    const url = (this.baseUrl ?? 'http://localhost:5233') + `/api/RoomRental/uploadImageDescription`;

    const form = new FormData();
    for (const f of files) {
      if (!f?.data) continue;
      form.append('uploadImages', f.data as any, f.fileName ?? 'uploadImages');
    }

    return this.http.post<any>(url, form).pipe(
      map((res) => {
        const paths = res?.paths;
        if (Array.isArray(paths)) return paths.map((x) => String(x));
        if (Array.isArray(res)) return res.map((x) => String(x));
        return [];
      })
    );
  }
}

