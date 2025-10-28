import * as aas from '@aas-core-works/aas-core3.0-typescript';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { ConfiguredProduct } from '../model/configured-product';
import { KatalogEintrag } from '../model/katalog-eintrag';
import { ProducedProductRequest } from '../model/produced-product-request';
import { ProductionResponseDto } from '../model/production-response-dto';

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

  async createProduct(newProduct: ProducedProductRequest) {
    const response = await lastValueFrom(
      this.http.post<ProductionResponseDto>(
        `${this.baseUrl}api/production/createProduct`,
        newProduct
      )
    );
    return response.producedProduct!;
  }

  async downloadPdf(url: string): Promise<Blob> {
    return lastValueFrom(
      this.http.get(url, {
        responseType: 'blob',
      })
    );
  }

  async isToolRequired(partId: number) {
    const params = new HttpParams().set('partId', partId);
    return lastValueFrom(
      this.http.get<boolean>(`${this.baseUrl}api/production/isToolRequired`, {
        params,
      })
    );
  }

  async getAssemblySubmodel(partId: number) {
    try {
      const params = new HttpParams().set('partId', partId);
      // Get raw JSON string for the submodel
      const smString = await lastValueFrom(
        this.http.get<string>(`${this.baseUrl}api/production/getassemblypropertiessubmodel`, {
          params,
          responseType: 'text' as 'json',
        })
      );

      if (!smString) return null;

      // Parse and deserialize to AAS Submodel
      const json = JSON.parse(smString);
      const result = aas.jsonization.submodelFromJsonable(json);

      return result.value;
    } catch (e) {
      console.error('getAssemblySubmodel failed', e);
      return null;
    }
  }

  async initializeTool(aasId: string, propertyIdShortPath: string, propertyValue: string) {
    const toolData = { aasId, propertyIdShortPath, propertyValue };

    return lastValueFrom(
      this.http.post<boolean>(`${this.baseUrl}api/production/initializeTool`, toolData)
    );
  }

  async getToolData(aasId: string) {
    const params = new HttpParams().set('aasId', aasId);
    const smString = await lastValueFrom(
      this.http.get<string>(`${this.baseUrl}api/production/getToolData`, {
        params,
        responseType: 'text' as 'json',
      })
    );

    if (!smString) return null;

    // Parse and deserialize to AAS Submodel
    const json = JSON.parse(smString);
    const result = aas.jsonization.submodelFromJsonable(json);

    return result.value;
  }
}
