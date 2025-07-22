import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { KatalogEintrag } from '../model/katalog-eintrag';
import { ConfiguredProduct } from '../model/configured-product';

@Injectable({
  providedIn: 'root',
})
export class ConfigurationCreateService {
  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {}

  async getAllRohteil() {
    return lastValueFrom(
      this.http.get<KatalogEintrag[]>(`${this.baseUrl}api/katalog/getAllRohteil`)
    );
  }

  createProduct(newProduct: ConfiguredProduct) {
    return lastValueFrom(
      this.http.post<ConfiguredProduct>(
        `${this.baseUrl}api/konfigurator/createProduct`,
        newProduct
      )
    );
  }
}
