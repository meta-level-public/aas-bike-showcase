import { AssetAdministrationShell } from '@aas-core-works/aas-core3.0-typescript/types';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class RegistryService {

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {}

  async loadShells(registryUrl: string, aasId: string) {
    const params = new HttpParams().set(
      'registryUrl',
      encodeURIComponent(`${registryUrl}`)
    ).set('aasId', aasId);
    const result = await lastValueFrom(
      this.http.get<any>(`${this.baseUrl}api/proxy/registry`, { params })
    );

    if (result.result != null) {
      return result.result;
    } else {
      return result;
    }
  }

}
