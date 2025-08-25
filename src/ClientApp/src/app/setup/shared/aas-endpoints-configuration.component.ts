import { CommonModule } from '@angular/common';
import { Component, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { InputTextModule } from 'primeng/inputtext';
import { HeaderParameter } from '../../model/header-parameter';
import { SecurityConfigurationComponent } from '../security-configuration/security-configuration.component';

@Component({
  selector: 'app-aas-endpoints-configuration',
  standalone: true,
  templateUrl: './aas-endpoints-configuration.component.html',
  styleUrl: './aas-endpoints-configuration.component.css',
  imports: [CommonModule, FormsModule, InputTextModule, SecurityConfigurationComponent],
})
export class AasEndpointsConfigurationComponent {
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
}
