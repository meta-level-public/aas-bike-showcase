// Konfigurator Request DTOs
export interface CreateConfiguredProduct {
  name: string;
  price: number;
  aasId?: string;
  globalAssetId?: string;
  bestandteile: CreateProductPart[];
}

export interface CreateProductPart {
  katalogEintragId: number;
  name: string;
  price: number;
  amount: number;
  usageDate: Date;
}

// Konfigurator Response DTOs
export interface ConfigurationResponse {
  success: boolean;
  message: string;
  configuredProduct?: import('./configured-product').ConfiguredProduct;
  error?: string;
}
