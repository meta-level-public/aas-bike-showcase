import { HttpClient, HttpParams } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { lastValueFrom } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') private baseUrl: string,
  ) {}

  async countAas() {
    return lastValueFrom(
      this.http.get<number>(
        `${this.baseUrl}api/Dashboard/GetCountContainedShells`,
      ),
    );
  }
  async countUpdateable() {
    return lastValueFrom(
      this.http.get<number>(
        `${this.baseUrl}api/Dashboard/GetCountAvailableUpdateCount`,
      ),
    );
  }
  async countConfiguredProducts() {
    return lastValueFrom(
      this.http.get<number>(`${this.baseUrl}api/Dashboard/GetCountProducts`),
    );
  }
  async countProducedProducts() {
    return lastValueFrom(
      this.http.get<number>(`${this.baseUrl}api/Dashboard/GetCountProduced`),
    );
  }
}
