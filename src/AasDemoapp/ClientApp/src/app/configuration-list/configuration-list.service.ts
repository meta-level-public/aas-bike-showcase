import { HttpClient, HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { ConfiguredProduct } from '../model/configured-product';

@Injectable({
  providedIn: 'root',
})
export class ConfigurationListService {
  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {}

  async getAllFertigteil() {
    return lastValueFrom(
      this.http.get<ConfiguredProduct[]>(`${this.baseUrl}api/Konfigurator/getAll`)
    );
  }

  async deleteItem(id: number) {
    const params = new HttpParams().set('id', id);
    return lastValueFrom(this.http.delete(`${this.baseUrl}api/Konfigurator/delete`, { params }));
  }
}
