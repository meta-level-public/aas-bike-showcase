export interface SupplierDto {
  id?: number;
  name: string;
  logo: string;
  remoteRepositoryUrl: string;
}

export interface CreateSupplierDto {
  name: string;
  logo: string;
  remoteRepositoryUrl: string;
}

export interface SupplierResponseDto {
  success: boolean;
  message: string;
  supplier?: SupplierDto;
  error?: string;
}
