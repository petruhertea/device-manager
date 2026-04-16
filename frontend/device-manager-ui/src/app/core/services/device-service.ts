import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Device, CreateDevice, UpdateDevice } from '../models/device.model';
import { environment } from '../../../environments/environment';

export interface GenerateDescriptionRequest {
  name:            string;
  manufacturer:    string;
  type:            string;
  operatingSystem: string;
  processor:       string;
  ramAmount:       number;
}

export interface GenerateDescriptionResponse {
  description: string;
}

@Injectable({ providedIn: 'root' })
export class DeviceService {
  private readonly http    = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/devices`;

  getAll(): Observable<Device[]> {
    return this.http.get<Device[]>(this.baseUrl);
  }

  getById(id: number): Observable<Device> {
    return this.http.get<Device>(`${this.baseUrl}/${id}`);
  }

  search(query: string): Observable<Device[]> {
    return this.http.get<Device[]>(`${this.baseUrl}/search`, {
      params: { q: query }
    });
  }

  create(device: CreateDevice): Observable<Device> {
    return this.http.post<Device>(this.baseUrl, device);
  }

  update(id: number, device: UpdateDevice): Observable<Device> {
    return this.http.put<Device>(`${this.baseUrl}/${id}`, device);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  generateDescription(req: GenerateDescriptionRequest): Observable<GenerateDescriptionResponse> {
    return this.http.post<GenerateDescriptionResponse>(
      `${this.baseUrl}/generate-description`,
      req
    );
  }
}
