import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';

export interface ImpressumData {
  name: string;
  street: string;
  postalCode: string;
  city: string;
  country: string;
  email: string;
  phone?: string;
}

@Injectable({
  providedIn: 'root',
})
export class ImpressumService {
  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string
  ) {}

  async getImpressum(): Promise<ImpressumData> {
    return lastValueFrom(this.http.get<ImpressumData>(`${this.baseUrl}api/Impressum`));
  }
}
