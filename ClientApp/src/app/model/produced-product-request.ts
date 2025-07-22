import { ConfiguredProduct } from './configured-product';
import { KatalogEintrag } from './katalog-eintrag';
import { ProductPart } from './product-part';
import { ProductPartRequest } from './product-part-request';

export interface ProducedProductRequest {
  configuredProductId: number;

  bestandteile: ProductPartRequest[];

  price: number;
}
