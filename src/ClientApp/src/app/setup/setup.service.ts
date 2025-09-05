import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { Supplier } from '../model/supplier';
import { Setting } from './setting';

@Injectable({
  providedIn: 'root',
})
export class SetupService {
  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
  ) {}

  async saveSupplier(supplier: Supplier) {
    return lastValueFrom(
      this.http.post<Supplier>(`${this.baseUrl}api/supplier/add`, supplier),
    );
  }

  async updateSupplier(supplier: Supplier) {
    return lastValueFrom(
      this.http.put<any>(`${this.baseUrl}api/supplier/update`, supplier),
    );
  }

  async deleteSupplier(supplier: Supplier) {
    return lastValueFrom(
      this.http.delete(`${this.baseUrl}api/supplier/delete/${supplier.id}`),
    );
  }

  async getSuppliers() {
    return lastValueFrom(
      this.http.get<Supplier[]>(`${this.baseUrl}api/supplier/getAll`),
    );
  }

  async saveSetting(setting: any) {
    return lastValueFrom(
      this.http.post<Supplier>(`${this.baseUrl}api/setting/save`, setting),
    );
  }

  async getSettings() {
    return lastValueFrom(
      this.http.get<Setting[]>(`${this.baseUrl}api/setting/getAll`),
    );
  }
}
