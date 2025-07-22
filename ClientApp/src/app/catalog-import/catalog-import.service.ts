import { HttpClient, HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';
import { KatalogEintrag } from '../model/katalog-eintrag';

@Injectable({
  providedIn: 'root',
})
export class CatalogImportService {
  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {}

  save(newKatalogEintrag: KatalogEintrag) {
    return lastValueFrom(
      this.http.post<KatalogEintrag>(
        `${this.baseUrl}api/katalog/importRohteilTyp`,
        newKatalogEintrag
      )
    );
  }
}
