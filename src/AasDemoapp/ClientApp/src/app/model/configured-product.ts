import { ProductPart } from './product-part';

export interface ConfiguredProduct {
  id?: number;
  name: string;
  price: number;
  aasId?: string;
  globalAssetId?: string;
  bestandteile: ProductPart[];
  producedProductsCount: number;
}
