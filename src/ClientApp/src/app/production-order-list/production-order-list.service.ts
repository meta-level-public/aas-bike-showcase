import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import {
  CreateProductionOrder,
  ProductionOrder,
  ProductionOrderResponse,
} from '../model/production-order';

@Injectable({
  providedIn: 'root',
})
export class ProductionOrderListService {
  private baseUrl = '/api/ProductionOrder';

  constructor(private http: HttpClient) {}

  async getAllProductionOrders(): Promise<ProductionOrder[]> {
    try {
      const response = await firstValueFrom(
        this.http.get<ProductionOrder[]>(`${this.baseUrl}/GetAll`),
      );
      return response || [];
    } catch (error) {
      console.error('Error fetching production orders:', error);
      return [];
    }
  }

  async getProductionOrderById(id: number): Promise<ProductionOrder | null> {
    try {
      const response = await firstValueFrom(
        this.http.get<ProductionOrder>(`${this.baseUrl}/GetById/${id}`),
      );
      return response;
    } catch (error) {
      console.error('Error fetching production order:', error);
      return null;
    }
  }

  async markProductionCompleted(id: number): Promise<ProductionOrderResponse> {
    try {
      const response = await firstValueFrom(
        this.http.post<ProductionOrderResponse>(
          `${this.baseUrl}/MarkProductionCompleted/${id}`,
          {},
        ),
      );
      return response;
    } catch (error) {
      console.error('Error marking production as completed:', error);
      return {
        success: false,
        message: 'Failed to mark production as completed',
        error: error instanceof Error ? error.message : 'Unknown error',
      };
    }
  }

  async markAsShipped(id: number): Promise<ProductionOrderResponse> {
    try {
      const response = await firstValueFrom(
        this.http.post<ProductionOrderResponse>(
          `${this.baseUrl}/MarkAsShipped/${id}`,
          {},
        ),
      );
      return response;
    } catch (error) {
      console.error('Error marking order as shipped:', error);
      return {
        success: false,
        message: 'Failed to mark order as shipped',
        error: error instanceof Error ? error.message : 'Unknown error',
      };
    }
  }

  async createProductionOrder(
    createDto: CreateProductionOrder,
  ): Promise<ProductionOrderResponse> {
    try {
      const response = await firstValueFrom(
        this.http.post<ProductionOrderResponse>(
          `${this.baseUrl}/Create`,
          createDto,
        ),
      );
      return response;
    } catch (error) {
      console.error('Error creating production order:', error);
      return {
        success: false,
        message: 'Failed to create production order',
        error: error instanceof Error ? error.message : 'Unknown error',
      };
    }
  }
}
