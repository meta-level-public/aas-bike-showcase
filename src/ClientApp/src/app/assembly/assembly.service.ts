import { HttpClient, HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { ConfiguredProduct } from '../model/configured-product';
import { KatalogEintrag } from '../model/katalog-eintrag';
import { ProducedProduct } from '../model/produced-product';
import { ProducedProductRequest } from '../model/produced-product-request';

@Injectable({
  providedIn: 'root',
})
export class AssemblyService {
  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {}

  async getAllFertigteil() {
    return lastValueFrom(
      this.http.get<ConfiguredProduct[]>(`${this.baseUrl}api/Konfigurator/getAll`)
    );
  }
  async getRohteilInstanz(globalAssetId: string) {
    const params = new HttpParams().set('globalAssetId', globalAssetId);
    return lastValueFrom(
      this.http.get<KatalogEintrag>(`${this.baseUrl}api/Katalog/GetRohteilInstanz`, { params })
    );
  }

  createProduct(newProduct: ProducedProductRequest) {
    return lastValueFrom(
      this.http.post<ProducedProduct>(
        `${this.baseUrl}api/production/createProduct`,
        newProduct
      )
    );
  }
}
