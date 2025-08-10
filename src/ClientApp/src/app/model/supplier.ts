import { SecuritySetting } from "./security-setting";

export interface Supplier {
  id?: number;
  name: string;
  logo: string;
  remoteRepositoryUrl: string;
  securitySetting: SecuritySetting;
}
