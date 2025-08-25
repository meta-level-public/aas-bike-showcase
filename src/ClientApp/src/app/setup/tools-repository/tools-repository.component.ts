import { CommonModule } from '@angular/common';
import { Component, OnChanges, OnInit, input, output, signal } from '@angular/core';
import { AccordionModule } from 'primeng/accordion';
import { ButtonModule } from 'primeng/button';
import { HeaderParameter } from '../../model/header-parameter';
import { SecuritySetting } from '../../model/security-setting';
import { Setting } from '../setting';
import { SetupService } from '../setup.service';
import { AasEndpointsConfigurationComponent } from '../shared/aas-endpoints-configuration.component';

@Component({
  selector: 'app-tools-repository',
  templateUrl: './tools-repository.component.html',
  styleUrl: './tools-repository.component.css',
  imports: [CommonModule, AccordionModule, ButtonModule, AasEndpointsConfigurationComponent],
})
export class ToolsRepositoryComponent implements OnInit, OnChanges {
  // Inputs/Outputs
  settings = input<Setting[]>([]);
  settingsUpdated = output<void>();

  // AAS-like endpoint and security signals (same fields as shared component)
  aasRepositoryUrl = signal<string>('');
  aasRegistryUrl = signal<string>('');
  submodelRepositoryUrl = signal<string>('');
  submodelRegistryUrl = signal<string>('');
  discoveryUrl = signal<string>('');
  conceptDescriptionUrl = signal<string>('');
  headerParameters = signal<HeaderParameter[]>([]);
  certificate = signal<string>('');
  certificatePassword = signal<string>('');

  constructor(private setupService: SetupService) {}

  ngOnInit(): void {
    this.loadFromSettings();
  }

  ngOnChanges(): void {
    this.loadFromSettings();
  }

  private loadFromSettings() {
    const current = this.settings();
    this.aasRepositoryUrl.set(current.find(s => s.name === 'ToolsAASRepositoryUrl')?.value || '');
    this.aasRegistryUrl.set(current.find(s => s.name === 'ToolsAASRegistryUrl')?.value || '');
    this.submodelRepositoryUrl.set(current.find(s => s.name === 'ToolsSubmodelRepositoryUrl')?.value || '');
    this.submodelRegistryUrl.set(current.find(s => s.name === 'ToolsSubmodelRegistryUrl')?.value || '');
    this.discoveryUrl.set(current.find(s => s.name === 'ToolsDiscoveryUrl')?.value || '');
    this.conceptDescriptionUrl.set(current.find(s => s.name === 'ToolsConceptDescriptionUrl')?.value || '');

    const security = current.find(s => s.name === 'ToolsRepositorySecurity')?.value || '';
    if (security) {
      try {
        const parsed = JSON.parse(security) as SecuritySetting;
        this.certificate.set(parsed.certificate || '');
        this.certificatePassword.set(parsed.certificatePassword || '');
        this.headerParameters.set(parsed.headerParameters || []);
      } catch {
        // ignore parse errors and keep defaults
      }
    }
  }

  async save() {
    const toSave: Setting[] = [
      { name: 'ToolsAASRepositoryUrl', value: this.aasRepositoryUrl() },
      { name: 'ToolsAASRegistryUrl', value: this.aasRegistryUrl() },
      { name: 'ToolsSubmodelRepositoryUrl', value: this.submodelRepositoryUrl() },
      { name: 'ToolsSubmodelRegistryUrl', value: this.submodelRegistryUrl() },
      { name: 'ToolsDiscoveryUrl', value: this.discoveryUrl() },
      { name: 'ToolsConceptDescriptionUrl', value: this.conceptDescriptionUrl() },
      { name: 'ToolsRepositorySecurity', value: JSON.stringify(this.getSecuritySetting()) },
    ];

    for (const s of toSave) {
      await this.setupService.saveSetting(s);
    }

    this.settingsUpdated.emit();
  }

  private getSecuritySetting(): SecuritySetting {
    const security = new SecuritySetting();
    security.certificate = this.certificate();
    security.certificatePassword = this.certificatePassword();
    security.headerParameters = this.headerParameters();
    return security;
  }

  // Security event handlers
  onHeaderParametersChange(headerParameters: HeaderParameter[]) {
    this.headerParameters.set(headerParameters);
  }

  onCertificateChange(certificate: string) {
    this.certificate.set(certificate);
  }

  onCertificatePasswordChange(password: string) {
    this.certificatePassword.set(password);
  }
}
