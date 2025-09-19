import { CommonModule } from '@angular/common';
import { Component, input, OnChanges, OnInit, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { FieldsetModule } from 'primeng/fieldset';
import { FileUploadModule } from 'primeng/fileupload';
import { InputTextModule } from 'primeng/inputtext';
import { MessageModule } from 'primeng/message';
import { PasswordModule } from 'primeng/password';
import { HeaderParameter } from '../../model/header-parameter';
import { SecuritySetting } from '../../model/security-setting';

@Component({
  selector: 'app-security-configuration',
  templateUrl: './security-configuration.component.html',
  styleUrl: './security-configuration.component.css',
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    FieldsetModule,
    FileUploadModule,
    MessageModule,
    PasswordModule,
    InputTextModule,
  ],
})
export class SecurityConfigurationComponent implements OnInit, OnChanges {
  // Input signals
  headerParameters = input<HeaderParameter[]>([]);
  certificate = input<string>('');
  certificatePassword = input<string>('');
  legend = input<string>('Sicherheit');
  showCertificate = input<boolean>(true);

  // Output events
  headerParametersChange = output<HeaderParameter[]>();
  certificateChange = output<string>();
  certificatePasswordChange = output<string>();

  // Local working copies
  localHeaderParameters = signal<HeaderParameter[]>([]);
  localCertificate = signal<string>('');
  localCertificatePassword = signal<string>('');

  ngOnInit() {
    this.updateLocalValues();
  }

  ngOnChanges() {
    this.updateLocalValues();
  }

  private updateLocalValues() {
    this.localHeaderParameters.set([...this.headerParameters()]);
    this.localCertificate.set(this.certificate());
    this.localCertificatePassword.set(this.certificatePassword());
  }

  addHeaderRow() {
    const currentHeaders = this.localHeaderParameters();
    const newHeaders = [...currentHeaders, new HeaderParameter()];
    this.localHeaderParameters.set(newHeaders);
    this.headerParametersChange.emit(newHeaders);
  }

  removeHeaderRow(headerParameter: HeaderParameter) {
    const currentHeaders = this.localHeaderParameters();
    const newHeaders = currentHeaders.filter((x) => x !== headerParameter);
    this.localHeaderParameters.set(newHeaders);
    this.headerParametersChange.emit(newHeaders);
  }

  onHeaderParameterChange() {
    this.headerParametersChange.emit(this.localHeaderParameters());
  }

  async setCertificateFile(event: any) {
    if (event.files.length > 0) {
      const file = event.files[0];
      const reader = new FileReader();
      reader.onload = () => {
        const arrayBuffer = reader.result as ArrayBuffer;
        const binaryString = String.fromCharCode(...new Uint8Array(arrayBuffer));
        const text = window.btoa(binaryString);
        this.localCertificate.set(text);
        this.certificateChange.emit(text);
      };
      reader.readAsArrayBuffer(file);
    }
  }

  onCertificatePasswordChange(password: string) {
    this.localCertificatePassword.set(password);
    this.certificatePasswordChange.emit(password);
  }

  // Utility method to get security setting object
  getSecuritySetting(): SecuritySetting {
    const security = new SecuritySetting();
    security.certificate = this.localCertificate();
    security.certificatePassword = this.localCertificatePassword();
    security.headerParameters = this.localHeaderParameters();
    return security;
  }
}
