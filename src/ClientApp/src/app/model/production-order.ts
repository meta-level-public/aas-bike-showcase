// ProductionOrder DTOs
export interface ProductionOrder {
  id?: number;
  configuredProductId: number;
  configuredProductName: string;
  anzahl: number;
  address?: Address;
  createdAt: Date;
  updatedAt?: Date;
  fertigstellungsDatum?: Date;
  produktionAbgeschlossen: boolean;
  versandt: boolean;
  versandDatum?: Date;
}

export interface Address {
  id?: number;
  name?: string;
  vorname?: string;
  strasse?: string;
  plz?: string;
  ort?: string;
  land?: string;
  lat?: number;
  long?: number;
}

export interface CreateProductionOrder {
  configuredProductId: number;
  anzahl: number;
  address?: Address;
}

export interface ProductionOrderResponse {
  success: boolean;
  message: string;
  productionOrder?: ProductionOrder;
  error?: string;
}

// Enums für die Filterung
export enum ProductionOrderStatus {
  PENDING_PRODUCTION = 'PENDING_PRODUCTION',
  READY_FOR_SHIPPING = 'READY_FOR_SHIPPING',
  COMPLETED = 'COMPLETED',
}
