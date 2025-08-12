import { ConfiguredProduct } from './configured-product';

// Configuration Response DTO interface
export interface ConfigurationResponseDto {
  success: boolean;
  message: string;
  configuredProduct?: ConfiguredProduct;
  error?: string;
}
