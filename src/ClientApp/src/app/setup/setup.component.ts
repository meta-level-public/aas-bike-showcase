import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccordionModule } from 'primeng/accordion';
import { ConfirmationService } from 'primeng/api';
import { Supplier } from '../model/supplier';
import { AasInfrastructureComponent } from './aas-infrastructure/aas-infrastructure.component';
import { CompanyAddressComponent } from './company-address/company-address.component';
import { ErpConfigurationComponent } from './erp-configuration/erp-configuration.component';
import { Setting } from './setting';
import { SetupService } from './setup.service';
import { SupplierManagementComponent } from './supplier-management/supplier-management.component';

@Component({
  selector: 'app-setup',
  templateUrl: './setup.component.html',
  styleUrl: './setup.component.css',
  imports: [
    CommonModule,
    FormsModule,
    AccordionModule,
    ErpConfigurationComponent,
    AasInfrastructureComponent,
    SupplierManagementComponent,
    CompanyAddressComponent,
  ],
})
export class SetupComponent implements OnInit {
  // ERP-bezogene Signals
  selectedErp = signal<{ label: string; logo: string } | undefined>(undefined);

  // Supplier-bezogene Signals
  suppliers = signal<Supplier[]>([]);

  // Settings-bezogene Signals
  settings = signal<Setting[]>([]);

  constructor(
    private setupService: SetupService,
    private cdRef: ChangeDetectorRef,
    private confirmationService: ConfirmationService
  ) {}

  async ngOnInit() {
    await this.loadSuppliers();
    await this.loadSettings();

    const currentSettings = this.settings();
    const erpSetting = currentSettings.find((s) => s.name === 'ERP')?.value;
    const erpOptions = [
      { label: 'Meta SimplestErp', logo: 'ml.jpeg' },
      { label: 'SAP', logo: 'sap.png' },
      { label: 'ERP X', logo: 'erp.png' },
    ];
    const foundErp = erpOptions.find((e) => e.label === erpSetting);
    this.selectedErp.set(foundErp);
  }

  async loadSuppliers() {
    const suppliers = await this.setupService.getSuppliers();
    this.suppliers.set(suppliers);
  }

  async loadSettings() {
    const settings = await this.setupService.getSettings();
    this.settings.set(settings);
  }

  // Event handlers for child components
  onErpSelected(erp: { label: string; logo: string }) {
    this.selectedErp.set(erp);
  }

  async onErpSettingsSaved() {
    await this.loadSettings();
  }

  async onAasSettingsUpdated() {
    await this.loadSettings();
  }

  async onSuppliersUpdated() {
    await this.loadSuppliers();
  }

  async onCompanyAddressUpdated() {
    await this.loadSettings();
  }
}
