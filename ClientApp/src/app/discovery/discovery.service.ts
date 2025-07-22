import { HttpClient, HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class DiscoveryService {
  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {}

  async query(registryUrl: string, assetId: string) {
    const params = new HttpParams()
      .set('registryUrl', encodeURIComponent(`${registryUrl}`))
      .set('assetId', assetId);
    return lastValueFrom(
      this.http.get<string[]>(`${this.baseUrl}api/proxy/discovery`, { params })
    );
  }
}
