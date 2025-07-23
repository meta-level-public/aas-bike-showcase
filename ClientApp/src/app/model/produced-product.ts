import { ConfiguredProduct } from './configured-product';
import { ProductPart } from './product-part';

export interface ProducedProduct {
  id?: number;
  configuredProductId: number;
  configuredProductName: string;
  aasId: string;
  globalAssetId: string;
  productionDate: Date;
  bestandteile: ProductPart[];

  // Optional für erweiterte Anzeige
  configuredProduct?: ConfiguredProduct;
}
