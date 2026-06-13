import { HttpClient } from '@angular/common/http';
import { Inject, Injectable, Optional } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from './service-proxies';
import {
    ContractDto,
    ContractDtoPagedResultDto,
    ContractFilterDtoPagedRequestDto,
    CreateOrEditContractDto,
} from './contract.models';

@Injectable({ providedIn: 'root' })
export class ContractApiService {
    private readonly baseUrl: string;

    constructor(
        private http: HttpClient,
        @Optional() @Inject(API_BASE_URL) baseUrl?: string
    ) {
        this.baseUrl = baseUrl ?? 'http://localhost:5233';
    }

    getAllContract(body: ContractFilterDtoPagedRequestDto): Observable<ContractDtoPagedResultDto> {
        return this.http.post<ContractDtoPagedResultDto>(`${this.baseUrl}/api/Contract/getAllContractAsync`, body);
    }

    getContractById(id: number): Observable<ContractDto> {
        return this.http.get<ContractDto>(`${this.baseUrl}/api/Contract/${id}`);
    }

    createOrEditContract(body: CreateOrEditContractDto): Observable<void> {
        return this.http.post<void>(`${this.baseUrl}/api/Contract/createOrEdit`, body);
    }

    deleteContract(id: number): Observable<void> {
        return this.http.delete<void>(`${this.baseUrl}/api/Contract/${id}`);
    }
}
