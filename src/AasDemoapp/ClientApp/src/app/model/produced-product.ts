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
  handoverDocumentationPdfUrl?: string;
  handoverDocumentationPdfBase64?: string;
  handoverDocumentationPdfFileName?: string;

  // Optional für erweiterte Anzeige
  configuredProduct?: ConfiguredProduct;
}
