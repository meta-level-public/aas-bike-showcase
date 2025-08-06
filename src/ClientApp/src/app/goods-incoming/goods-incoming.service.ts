import { HttpClient, HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { KatalogEintrag } from '../model/katalog-eintrag';
import { RohteilLookupResult } from './rohteil-lookup-result';

@Injectable({
  providedIn: 'root',
})
export class GoodsIncomingService {
  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {}

  async lookupRohteil(globalAssetId: string) {
    const params = new HttpParams().set('instanzGlobalAssetId', globalAssetId);
    return lastValueFrom(
      this.http.get<RohteilLookupResult>(
        `${this.baseUrl}api/katalog/lookupRohteil`,
        { params }
      )
    );
  }

  async getRandomRohteil() {
    return lastValueFrom(
      this.http.get<{id: string}>(
        `${this.baseUrl}api/katalog/getRandomRohteil`,
      )
    );
  }

  createRohteilInstanz(newKatalogEintrag: KatalogEintrag) {
    return lastValueFrom(
      this.http.post<KatalogEintrag>(
        `${this.baseUrl}api/katalog/importRohteilInstanz`,
        newKatalogEintrag
      )
    );
  }
}
