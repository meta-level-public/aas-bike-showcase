// Production Request DTOs
export interface ProducedProductRequest {
  id?: number;
  configuredProductId: number;
  bestandteilRequests: BestandteilRequest[];
}

export interface BestandteilRequest {
  globalAssetId: string;
  usageDate: Date;
  amount: number;
}

// Production Response DTOs
export interface ProductionResponse {
  success: boolean;
  message: string;
  producedProduct?: import('./produced-product').ProducedProduct;
  error?: string;
}

// HandoverDocumentation DTOs
export interface HandoverDocumentationResponse {
  success: boolean;
  message: string;
  submodel?: SubmodelSummary;
  error?: string;
}

export interface SubmodelSummary {
  id: string;
  idShort?: string;
  description?: string;
  version?: string;
  revision?: string;
  elementCount: number;
}
