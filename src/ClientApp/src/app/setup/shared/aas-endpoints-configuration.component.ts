import { CommonModule } from '@angular/common';
import { Component, input, output, ViewChild } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { HeaderParameter } from '../../model/header-parameter';
import { SecurityConfigurationComponent } from '../security-configuration/security-configuration.component';

@Component({
  selector: 'app-aas-endpoints-configuration',
  standalone: true,
  templateUrl: './aas-endpoints-configuration.component.html',
  styleUrl: './aas-endpoints-configuration.component.css',
  imports: [
    CommonModule,
    FormsModule,
    InputTextModule,
    ButtonModule,
    SecurityConfigurationComponent,
  ],
})
export class AasEndpointsConfigurationComponent {
  @ViewChild('securityCfg') securityCfg?: SecurityConfigurationComponent;

  // URL inputs
  aasRepositoryUrl = input<string>('');
  aasRegistryUrl = input<string>('');
  submodelRepositoryUrl = input<string>('');
  submodelRegistryUrl = input<string>('');
  discoveryUrl = input<string>('');
  conceptDescriptionUrl = input<string>('');
  showEndpoints = input<boolean>(true);

  // Security inputs
  headerParameters = input<HeaderParameter[]>([]);
  certificate = input<string>('');
  certificatePassword = input<string>('');
  securityLegend = input<string>('Sicherheit');
  showCertificate = input<boolean>(true);

  // URL outputs
  aasRepositoryUrlChange = output<string>();
  aasRegistryUrlChange = output<string>();
  submodelRepositoryUrlChange = output<string>();
  submodelRegistryUrlChange = output<string>();
  discoveryUrlChange = output<string>();
  conceptDescriptionUrlChange = output<string>();

  // Security outputs
  headerParametersChange = output<HeaderParameter[]>();
  certificateChange = output<string>();
  certificatePasswordChange = output<string>();

  // Copy current configuration (endpoints + security) to clipboard as JSON
  async copyConfigToClipboard() {
    try {
      const security = this.securityCfg?.getSecuritySetting() ?? {
        certificate: this.certificate(),
        certificatePassword: this.certificatePassword(),
        headerParameters: this.headerParameters(),
      };

      const payload = {
        _type: 'AasEndpointsConfiguration',
        _version: 1,
        aasRepositoryUrl: this.aasRepositoryUrl(),
        aasRegistryUrl: this.aasRegistryUrl(),
        submodelRepositoryUrl: this.submodelRepositoryUrl(),
        submodelRegistryUrl: this.submodelRegistryUrl(),
        discoveryUrl: this.discoveryUrl(),
        conceptDescriptionUrl: this.conceptDescriptionUrl(),
        security,
      };

      await navigator.clipboard.writeText(JSON.stringify(payload));
    } catch (err) {
      console.error('Clipboard copy failed', err);
      alert('Konnte die Konfiguration nicht in die Zwischenablage kopieren.');
    }
  }

  // Paste configuration JSON from clipboard and emit changes
  async pasteConfigFromClipboard() {
    try {
      const text = await navigator.clipboard.readText();
      if (!text) {
        alert('Zwischenablage ist leer.');
        return;
      }
      let data: any;
      try {
        data = JSON.parse(text);
      } catch {
        alert('Inhalt der Zwischenablage ist kein gültiges JSON.');
        return;
      }

      const hasAny =
        typeof data === 'object' &&
        data !== null &&
        ('aasRepositoryUrl' in data ||
          'aasRegistryUrl' in data ||
          'submodelRepositoryUrl' in data ||
          'submodelRegistryUrl' in data ||
          'discoveryUrl' in data ||
          'conceptDescriptionUrl' in data ||
          'security' in data);
      if (!hasAny) {
        alert(
          'Kein passendes Konfigurationsobjekt in der Zwischenablage gefunden.',
        );
        return;
      }

      if (data.aasRepositoryUrl !== undefined)
        this.aasRepositoryUrlChange.emit(String(data.aasRepositoryUrl));
      if (data.aasRegistryUrl !== undefined)
        this.aasRegistryUrlChange.emit(String(data.aasRegistryUrl));
      if (data.submodelRepositoryUrl !== undefined)
        this.submodelRepositoryUrlChange.emit(
          String(data.submodelRepositoryUrl),
        );
      if (data.submodelRegistryUrl !== undefined)
        this.submodelRegistryUrlChange.emit(String(data.submodelRegistryUrl));
      if (data.discoveryUrl !== undefined)
        this.discoveryUrlChange.emit(String(data.discoveryUrl));
      if (data.conceptDescriptionUrl !== undefined)
        this.conceptDescriptionUrlChange.emit(
          String(data.conceptDescriptionUrl),
        );

      const sec = data.security ?? {};
      if (sec.headerParameters !== undefined)
        this.headerParametersChange.emit(
          Array.isArray(sec.headerParameters) ? sec.headerParameters : [],
        );
      if (sec.certificate !== undefined)
        this.certificateChange.emit(String(sec.certificate ?? ''));
      if (sec.certificatePassword !== undefined)
        this.certificatePasswordChange.emit(
          String(sec.certificatePassword ?? ''),
        );
    } catch (err) {
      console.error('Clipboard paste failed', err);
      alert('Konnte keine Konfiguration aus der Zwischenablage einfügen.');
    }
  }
}
