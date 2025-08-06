import { ProductPartRequest } from './product-part-request';

export interface ProducedProductRequest {
  configuredProductId: number;

  bestandteilRequests: ProductPartRequest[];

  price: number;
}
