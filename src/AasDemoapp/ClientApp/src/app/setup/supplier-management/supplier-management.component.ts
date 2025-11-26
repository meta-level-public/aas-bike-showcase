import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { AccordionModule } from 'primeng/accordion';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DataViewModule } from 'primeng/dataview';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { HeaderParameter } from '../../model/header-parameter';
import { Supplier } from '../../model/supplier';
// removed: SecurityConfigurationComponent (now handled by shared component)
import { SetupService } from '../setup.service';
import { AasEndpointsConfigurationComponent } from '../shared/aas-endpoints-configuration.component';

@Component({
  selector: 'app-supplier-management',
  templateUrl: './supplier-management.component.html',
  styleUrl: './supplier-management.component.css',
  imports: [
    CommonModule,
    FormsModule,
    AccordionModule,
    DataViewModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    TranslateModule,
    AasEndpointsConfigurationComponent,
  ],
})
export class SupplierManagementComponent {
  // Input signals
  suppliers = input<Supplier[]>([]);

  // Output events
  suppliersUpdated = output<void>();

  // Local state
  addSupplierDialogVisible = signal<boolean>(false);
  editSupplierDialogVisible = signal<boolean>(false);
  newSupplier = signal<Supplier | undefined>(undefined);
  supplierToEdit = signal<Supplier | undefined>(undefined);

  constructor(
    private setupService: SetupService,
    private cdRef: ChangeDetectorRef,
    private confirmationService: ConfirmationService
  ) {}

  showAddSupplierDialog() {
    this.newSupplier.set({
      name: '',
      logo: '',
      remoteAasRepositoryUrl: '',
      remoteSmRepositoryUrl: '',
      remoteAasRegistryUrl: '',
      remoteSmRegistryUrl: '',
      remoteDiscoveryUrl: '',
      remoteCdRepositoryUrl: '',
      securitySetting: {
        certificate: '',
        certificatePassword: '',
        headerParameters: [],
      },
    } as Supplier);
    this.addSupplierDialogVisible.set(true);
  }

  showEditSupplierDialog(supplier: Supplier) {
    // Create a copy of the supplier to avoid modifying the original
    this.supplierToEdit.set({
      ...supplier,
      securitySetting: {
        ...supplier.securitySetting,
      },
    });
    this.editSupplierDialogVisible.set(true);
  }

  async saveSupplier() {
    const currentSupplier = this.newSupplier();
    if (currentSupplier === undefined) return;

    await this.setupService.saveSupplier(currentSupplier);
    this.addSupplierDialogVisible.set(false);
    this.suppliersUpdated.emit();
    this.cdRef.markForCheck();
  }

  async saveEditSupplier() {
    const currentSupplier = this.supplierToEdit();
    if (currentSupplier === undefined) return;

    await this.setupService.updateSupplier(currentSupplier);
    this.editSupplierDialogVisible.set(false);
    this.suppliersUpdated.emit();
    this.cdRef.markForCheck();
  }

  editSupplier(supplier: Supplier) {
    this.showEditSupplierDialog(supplier);
  }

  async deleteSupplier(supplier: Supplier) {
    this.confirmationService.confirm({
      message: `Soll der Lieferant ${supplier.name} wirklich gelöscht werden?`,
      accept: async () => {
        await this.setupService.deleteSupplier(supplier);
        this.suppliersUpdated.emit();
        this.cdRef.markForCheck();
      },
    });
  }

  // Helper methods for two-way binding in template
  updateSupplierName(name: string) {
    this.newSupplier.update((s) => (s ? { ...s, name } : s));
  }

  updateSupplierLogo(logo: string) {
    this.newSupplier.update((s) => (s ? { ...s, logo } : s));
  }

  updateSupplierAasRepositoryUrl(remoteAasRepositoryUrl: string) {
    this.newSupplier.update((s) => (s ? { ...s, remoteAasRepositoryUrl } : s));
  }

  updateSupplierSmRepositoryUrl(remoteSmRepositoryUrl: string) {
    this.newSupplier.update((s) => (s ? { ...s, remoteSmRepositoryUrl } : s));
  }

  updateSupplierAasRegistryUrl(remoteAasRegistryUrl: string) {
    this.newSupplier.update((s) => (s ? { ...s, remoteAasRegistryUrl } : s));
  }

  updateSupplierSmRegistryUrl(remoteSmRegistryUrl: string) {
    this.newSupplier.update((s) => (s ? { ...s, remoteSmRegistryUrl } : s));
  }

  updateSupplierDiscoveryUrl(remoteDiscoveryUrl: string) {
    this.newSupplier.update((s) => (s ? { ...s, remoteDiscoveryUrl } : s));
  }

  updateSupplierCdRepositoryUrl(remoteCdRepositoryUrl: string) {
    this.newSupplier.update((s) => (s ? { ...s, remoteCdRepositoryUrl } : s));
  }

  // Helper methods for editing supplier
  updateEditSupplierName(name: string) {
    this.supplierToEdit.update((s) => (s ? { ...s, name } : s));
  }

  updateEditSupplierLogo(logo: string) {
    this.supplierToEdit.update((s) => (s ? { ...s, logo } : s));
  }

  updateEditSupplierAasRepositoryUrl(remoteAasRepositoryUrl: string) {
    this.supplierToEdit.update((s) => (s ? { ...s, remoteAasRepositoryUrl } : s));
  }

  updateEditSupplierSmRepositoryUrl(remoteSmRepositoryUrl: string) {
    this.supplierToEdit.update((s) => (s ? { ...s, remoteSmRepositoryUrl } : s));
  }

  updateEditSupplierAasRegistryUrl(remoteAasRegistryUrl: string) {
    this.supplierToEdit.update((s) => (s ? { ...s, remoteAasRegistryUrl } : s));
  }

  updateEditSupplierSmRegistryUrl(remoteSmRegistryUrl: string) {
    this.supplierToEdit.update((s) => (s ? { ...s, remoteSmRegistryUrl } : s));
  }

  updateEditSupplierDiscoveryUrl(remoteDiscoveryUrl: string) {
    this.supplierToEdit.update((s) => (s ? { ...s, remoteDiscoveryUrl } : s));
  }

  updateEditSupplierCdRepositoryUrl(remoteCdRepositoryUrl: string) {
    this.supplierToEdit.update((s) => (s ? { ...s, remoteCdRepositoryUrl } : s));
  }

  // Security configuration for suppliers
  onSupplierSecurityHeadersChange(headerParameters: HeaderParameter[]) {
    // Update the security setting on the supplier object
    this.newSupplier.update((supplier) => {
      if (supplier && supplier.securitySetting) {
        return {
          ...supplier,
          securitySetting: {
            ...supplier.securitySetting,
            headerParameters: headerParameters,
          },
        };
      }
      return supplier;
    });
    console.log('Supplier security headers changed:', headerParameters);
  }

  onSupplierCertificateChange(certificate: string) {
    this.newSupplier.update((supplier) => {
      if (supplier && supplier.securitySetting) {
        return {
          ...supplier,
          securitySetting: {
            ...supplier.securitySetting,
            certificate: certificate,
          },
        };
      }
      return supplier;
    });
    console.log('Supplier certificate changed:', certificate);
  }

  onSupplierCertificatePasswordChange(certificatePassword: string) {
    this.newSupplier.update((supplier) => {
      if (supplier && supplier.securitySetting) {
        return {
          ...supplier,
          securitySetting: {
            ...supplier.securitySetting,
            certificatePassword: certificatePassword,
          },
        };
      }
      return supplier;
    });
    console.log('Supplier certificate password changed');
  }

  // Security configuration for editing suppliers
  onEditSupplierSecurityHeadersChange(headerParameters: HeaderParameter[]) {
    this.supplierToEdit.update((supplier) => {
      if (supplier && supplier.securitySetting) {
        return {
          ...supplier,
          securitySetting: {
            ...supplier.securitySetting,
            headerParameters: headerParameters,
          },
        };
      }
      return supplier;
    });
    console.log('Edit supplier security headers changed:', headerParameters);
  }

  onEditSupplierCertificateChange(certificate: string) {
    this.supplierToEdit.update((supplier) => {
      if (supplier && supplier.securitySetting) {
        return {
          ...supplier,
          securitySetting: {
            ...supplier.securitySetting,
            certificate: certificate,
          },
        };
      }
      return supplier;
    });
    console.log('Edit supplier certificate changed:', certificate);
  }

  onEditSupplierCertificatePasswordChange(certificatePassword: string) {
    this.supplierToEdit.update((supplier) => {
      if (supplier && supplier.securitySetting) {
        return {
          ...supplier,
          securitySetting: {
            ...supplier.securitySetting,
            certificatePassword: certificatePassword,
          },
        };
      }
      return supplier;
    });
    console.log('Edit supplier certificate password changed');
  }
}
