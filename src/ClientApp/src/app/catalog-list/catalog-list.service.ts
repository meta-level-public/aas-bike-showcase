import { HttpClient, HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { KatalogEintrag } from '../model/katalog-eintrag';

@Injectable({
  providedIn: 'root',
})
export class CatalogListService {
  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {}

  async getAllRohteil() {
    return lastValueFrom(
      this.http.get<KatalogEintrag[]>(`${this.baseUrl}api/katalog/getAllRohteil`)
    );
  }

  async deleteItem(id: number) {
    const params = new HttpParams().set('id', id);
    return lastValueFrom(this.http.delete(`${this.baseUrl}api/Katalog/delete`, { params }));
  }

  save(newKatalogEintrag: KatalogEintrag) {
    return lastValueFrom(
      this.http.post<KatalogEintrag>(`${this.baseUrl}api/katalog/import`, newKatalogEintrag)
    );
  }
}
