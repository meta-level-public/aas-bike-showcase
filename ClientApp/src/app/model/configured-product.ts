import { ProductPart } from './product-part';

export interface ConfiguredProduct {
  id?: number;
  name: string;
  aasId: string;
  globalAssetId: string;

  bestandteile?: ProductPart[];

  price: number;
}
