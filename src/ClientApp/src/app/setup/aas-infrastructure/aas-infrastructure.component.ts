import { CommonModule } from '@angular/common';
import { Component, input, OnChanges, OnInit, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccordionModule } from 'primeng/accordion';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { HeaderParameter } from '../../model/header-parameter';
import { SecuritySetting } from '../../model/security-setting';
import { SecurityConfigurationComponent } from '../security-configuration/security-configuration.component';
import { Setting } from '../setting';
import { SetupService } from '../setup.service';

@Component({
  selector: 'app-aas-infrastructure',
  templateUrl: './aas-infrastructure.component.html',
  styleUrl: './aas-infrastructure.component.css',
  imports: [
    CommonModule,
    FormsModule,
    AccordionModule,
    ButtonModule,
    InputTextModule,
    SecurityConfigurationComponent,
  ],
})
export class AasInfrastructureComponent implements OnInit, OnChanges {
  // Input signals
  settings = input<Setting[]>([]);

  // Output events
  settingsUpdated = output<void>();

  // AAS Infrastructure URLs Signals
  aasRepositoryUrl = signal<string>('');
  aasRegistryUrl = signal<string>('');
  submodelRepositoryUrl = signal<string>('');
  submodelRegistryUrl = signal<string>('');
  discoveryUrl = signal<string>('');
  conceptDescriptionUrl = signal<string>('');

  // Security-related signals
  headerParameters = signal<HeaderParameter[]>([]);
  certificate = signal<string>('');
  certificatePassword = signal<string>('');

  constructor(private setupService: SetupService) {}

  ngOnInit() {
    this.loadAasUrls();
  }

  ngOnChanges() {
    this.loadAasUrls();
  }

  loadAasUrls() {
    const currentSettings = this.settings();

    this.aasRepositoryUrl.set(
      currentSettings.find((s) => s.name === 'AASRepositoryUrl')?.value || ''
    );
    this.aasRegistryUrl.set(
      currentSettings.find((s) => s.name === 'AASRegistryUrl')?.value || ''
    );
    this.submodelRepositoryUrl.set(
      currentSettings.find((s) => s.name === 'SubmodelRepositoryUrl')?.value || ''
    );
    this.submodelRegistryUrl.set(
      currentSettings.find((s) => s.name === 'SubmodelRegistryUrl')?.value || ''
    );
    this.discoveryUrl.set(
      currentSettings.find((s) => s.name === 'DiscoveryUrl')?.value || ''
    );
    this.conceptDescriptionUrl.set(
      currentSettings.find((s) => s.name === 'ConceptDescriptionUrl')?.value || ''
    );

    const security =
      currentSettings.find((s) => s.name === 'InfrastructureSecurity')?.value || '';
    if (security) {
      const parsedSecurity = JSON.parse(security) as SecuritySetting;
      this.certificate.set(parsedSecurity.certificate || '');
      this.certificatePassword.set(parsedSecurity.certificatePassword || '');
      this.headerParameters.set(parsedSecurity.headerParameters || []);
    }
  }

  async saveAasUrls() {
    const urlSettings: Setting[] = [
      { name: 'AASRepositoryUrl', value: this.aasRepositoryUrl() },
      { name: 'AASRegistryUrl', value: this.aasRegistryUrl() },
      { name: 'SubmodelRepositoryUrl', value: this.submodelRepositoryUrl() },
      { name: 'SubmodelRegistryUrl', value: this.submodelRegistryUrl() },
      { name: 'DiscoveryUrl', value: this.discoveryUrl() },
      { name: 'ConceptDescriptionUrl', value: this.conceptDescriptionUrl() },
      { name: 'InfrastructureSecurity', value: JSON.stringify(this.getSecuritySetting()) },
    ];

    for (const setting of urlSettings) {
      await this.setupService.saveSetting(setting);
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

  // Event handlers for security component
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
