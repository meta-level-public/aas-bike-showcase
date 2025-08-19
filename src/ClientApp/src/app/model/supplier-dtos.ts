export interface SupplierDto {
  id?: number;
  name: string;
  logo: string;
  remoteAasRepositoryUrl: string;
  remoteSmRepositoryUrl: string;
  remoteAasRegistryUrl: string;
  remoteSmRegistryUrl: string;
  remoteDiscoveryUrl: string;
  remoteCdRepositoryUrl: string;
}

export interface CreateSupplierDto {
  name: string;
  logo: string;
  remoteAasRepositoryUrl: string;
  remoteSmRepositoryUrl: string;
  remoteAasRegistryUrl: string;
  remoteSmRegistryUrl: string;
  remoteDiscoveryUrl: string;
  remoteCdRepositoryUrl: string;
}

export interface SupplierResponseDto {
  success: boolean;
  message: string;
  supplier?: SupplierDto;
  error?: string;
}
