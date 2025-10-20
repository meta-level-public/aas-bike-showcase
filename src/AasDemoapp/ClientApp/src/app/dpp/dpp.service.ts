import * as aas from '@aas-core-works/aas-core3.0-typescript';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { ProducedProduct } from '../model/produced-product';

@Injectable({
  providedIn: 'root',
})
export class DppService {
  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {}

  async loadShell(productId: number) {
    const params = new HttpParams().set('idProducedProduct', productId.toString());
    const result = await lastValueFrom(
      this.http.get<any>(`${this.baseUrl}api/dpp/getshell`, { params })
    );

    const instanceOrErrorPlain = aas.jsonization.assetAdministrationShellFromJsonable(result);
    if (instanceOrErrorPlain.value != null) {
      return instanceOrErrorPlain.value;
    } else {
      console.error(instanceOrErrorPlain.error);
    }

    return null;
  }

  async getAll() {
    return lastValueFrom(this.http.get<ProducedProduct[]>(`${this.baseUrl}api/dpp/getAll`));
  }

  deleteProduct(id: number) {
    return lastValueFrom(this.http.delete(`${this.baseUrl}api/dpp/delete/${id}`));
  }
}
