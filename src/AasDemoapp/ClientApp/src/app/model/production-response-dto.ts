import { KatalogEintrag } from './katalog-eintrag';

// ProductPart DTO interface
export interface ProductPartDto {
  id?: number;
  katalogEintragId?: number;
  katalogEintrag?: KatalogEintrag;
  name: string;
  price: number;
  amount: number;
  usageDate: Date;
  image?: string;
}

// ProducedProduct DTO interface
export interface ProducedProductDto {
  id?: number;
  configuredProductId: number;
  configuredProductName: string;
  bestandteile: ProductPartDto[];
  aasId: string;
  globalAssetId: string;
  productionDate: Date;
  handoverDocumentationPdfUrl?: string;
  handoverDocumentationPdfBase64?: string;
  handoverDocumentationPdfFileName?: string;
}

// Production Response DTO interface
export interface ProductionResponseDto {
  success: boolean;
  message: string;
  producedProduct?: ProducedProductDto;
  error?: string;
}
