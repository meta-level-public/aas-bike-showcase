import { ProductPartRequest } from './product-part-request';

export interface ProducedProductRequest {
  configuredProductId: number;
  productionOrderId?: number;
  bestandteilRequests: ProductPartRequest[];
  price: number;
}
