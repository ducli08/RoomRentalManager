import { HttpClient } from '@angular/common/http';
import { Inject, Injectable, Optional } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from './service-proxies';
import {
  CreateOrEditUtilityReadingDto,
  UtilityReadingDto,
  UtilityReadingFilterDto,
  UtilityReadingFilterDtoPagedRequestDto,
  UtilityReadingDtoPagedResultDto,
  UtilityReadingPrepareDto,
} from './utility-reading.models';

@Injectable({ providedIn: 'root' })
export class UtilityReadingApiService {
  private baseUrl: string;

  constructor(private http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
    this.baseUrl = baseUrl ?? '';
  }

  search(body: UtilityReadingFilterDtoPagedRequestDto): Observable<UtilityReadingDtoPagedResultDto> {
    return this.http.post<UtilityReadingDtoPagedResultDto>(`${this.baseUrl}/api/utility-readings/search`, body);
  }

  getById(id: number): Observable<UtilityReadingDto> {
    return this.http.get<UtilityReadingDto>(`${this.baseUrl}/api/utility-readings/${id}`);
  }

  prepare(contractId: number, month: number, year: number, utilityReadingId?: number): Observable<UtilityReadingPrepareDto> {
    let url = `${this.baseUrl}/api/utility-readings/prepare?contractId=${contractId}&month=${month}&year=${year}`;
    if (utilityReadingId) url += `&utilityReadingId=${utilityReadingId}`;
    return this.http.get<UtilityReadingPrepareDto>(url);
  }

  create(body: CreateOrEditUtilityReadingDto): Observable<UtilityReadingDto> {
    return this.http.post<UtilityReadingDto>(`${this.baseUrl}/api/utility-readings`, body);
  }

  update(id: number, body: CreateOrEditUtilityReadingDto): Observable<UtilityReadingDto> {
    return this.http.put<UtilityReadingDto>(`${this.baseUrl}/api/utility-readings/${id}`, body);
  }

  exportExcel(filter: UtilityReadingFilterDto): Observable<Blob> {
    return this.http.post(`${this.baseUrl}/api/utility-readings/export`, filter, { responseType: 'blob' });
  }
}
