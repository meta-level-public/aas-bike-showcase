import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import {
  HandoverDocumentationResponse,
  ProducedProductRequest,
  ProductionResponse,
} from '../model/production-dtos';

@Injectable({
  providedIn: 'root',
})
export class ProductionService {
  private baseUrl = '/api/production';

  constructor(private http: HttpClient) {}

  createProduct(request: ProducedProductRequest): Observable<ProductionResponse> {
    return this.http.post<ProductionResponse>(`${this.baseUrl}/CreateProduct`, request);
  }

  testHandoverDocumentation(): Observable<HandoverDocumentationResponse> {
    return this.http.get<HandoverDocumentationResponse>(
      `${this.baseUrl}/TestHandoverDocumentation`
    );
  }
}
