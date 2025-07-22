import { ConfiguredProduct } from './configured-product';
import { KatalogEintrag } from './katalog-eintrag';
import { ProductPart } from './product-part';

export interface ProducedProduct {
  id?: number;
  name: string;
  aasId: string;
  globalAssetId: string;

  configuredProduct?: ConfiguredProduct;

  bestandteile: ProductPart[];

  price: number;
}
