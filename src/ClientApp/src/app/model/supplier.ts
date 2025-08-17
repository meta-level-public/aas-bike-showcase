import { SecuritySetting } from "./security-setting";

export interface Supplier {
  id?: number;
  name: string;
  logo: string;
  remoteAasRepositoryUrl: string;
  remoteSmRepositoryUrl: string;
  remoteAasRegistryUrl: string;
  remoteSmRegistryUrl: string;
  remoteDiscoveryUrl: string;
  remoteCdRepositoryUrl: string;
  securitySetting: SecuritySetting;
}
