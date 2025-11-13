import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ImpressumConfig } from './impressum.model';

@Injectable({
  providedIn: 'root',
})
export class ImpressumService {
  constructor(private http: HttpClient) {}

  getConfig(): Observable<ImpressumConfig> {
    return this.http.get<ImpressumConfig>('/assets/config/impressum.json');
  }
}
