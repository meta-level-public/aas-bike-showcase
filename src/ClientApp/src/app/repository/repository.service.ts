import { HttpClient, HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import * as aas from '@aas-core-works/aas-core3.0-typescript';
import { AssetAdministrationShell } from '@aas-core-works/aas-core3.0-typescript/types';

@Injectable({
  providedIn: 'root',
})
export class RepositoryService {
  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {}

  async loadShells(registryUrl: string) {
    const params = new HttpParams().set(
      'registryUrl',
      encodeURIComponent(`${registryUrl}`)
    );
    const result = await lastValueFrom(
      this.http.get<any>(`${this.baseUrl}api/proxy/shells`, { params })
    );

    if (result.result != null) {
      let mappedDtos = result.result.map((dto: any) => {
        const instanceOrErrorPlain =
          aas.jsonization.assetAdministrationShellFromJsonable(dto);
        if (instanceOrErrorPlain.value != null) {
          return instanceOrErrorPlain.value;
        } else {
          console.error(instanceOrErrorPlain.error);
        }
        return null;
      });

      return mappedDtos.filter((dto: AssetAdministrationShell) => dto != null);
    } else {
      let mappedDtos = result.map((dto: any) => {
        const instanceOrErrorPlain =
          aas.jsonization.assetAdministrationShellFromJsonable(dto);
        if (instanceOrErrorPlain.value != null) {
          return instanceOrErrorPlain.value;
        } else {
          console.error(instanceOrErrorPlain.error);
        }
        return null;
      });

      return mappedDtos.filter((dto: AssetAdministrationShell) => dto != null);
    }
  }

  async loadShell(registryUrl: string, aasId: string) {
    const params = new HttpParams()
      .set('registryUrl', encodeURIComponent(`${registryUrl}`))
      .set('aasId', encodeURIComponent(`${aasId}`));
    const result = await lastValueFrom(
      this.http.get<any>(`${this.baseUrl}api/proxy/shell`, { params })
    );

    const instanceOrErrorPlain =
      aas.jsonization.assetAdministrationShellFromJsonable(result);
    if (instanceOrErrorPlain.value != null) {
      return [instanceOrErrorPlain.value];
    } else {
      console.error(instanceOrErrorPlain.error);
    }

    return [];
  }

  async import(
    localRegistryUrl: string,
    remoteRegistryUrl: string,
    id: string
  ) {
    const params = new HttpParams()
      .set('localRegistryUrl', encodeURIComponent(`${localRegistryUrl}`))
      .set('remoteRegistryUrl', encodeURIComponent(`${remoteRegistryUrl}`))
      .set('id', encodeURIComponent(id));
    const result = await lastValueFrom(
      this.http.get<any>(`${this.baseUrl}api/proxy/import`, { params })
    );

    return true;
  }
}
