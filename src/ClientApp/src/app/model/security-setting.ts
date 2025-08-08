import { HeaderParameter } from './header-parameter';

export class SecuritySetting {
  certificate = '';
  certificatePassword = '';
  headerParameters: HeaderParameter[] = [];
}
