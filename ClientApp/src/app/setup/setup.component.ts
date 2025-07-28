import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccordionModule } from 'primeng/accordion';
import { ConfirmationService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DataViewModule } from 'primeng/dataview';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectButtonModule } from 'primeng/selectbutton';
import { Supplier } from '../model/supplier';
import { Setting } from './setting';
import { SetupService } from './setup.service';

@Component({
  selector: 'app-setup',
  templateUrl: './setup.component.html',
  styleUrl: './setup.component.css',
  imports: [CommonModule, FormsModule, AccordionModule, SelectButtonModule, DataViewModule, ButtonModule, DialogModule, InputTextModule]
})
export class SetupComponent implements OnInit {
  erpOptions: { label: string; logo: string }[] = [];
  selectedErp: { label: string; logo: string } | undefined;

  suppliers: any[] = [];
  addSupplierDialogVisible: boolean = false;
  newSupplier: Supplier | undefined;
  settings: Setting[] = [];

  // AAS Infrastructure URLs
  aasRepositoryUrl: string = '';
  aasRegistryUrl: string = '';
  submodelRepositoryUrl: string = '';
  submodelRegistryUrl: string = '';
  discoveryUrl: string = '';
  conceptDescriptionUrl: string = '';

  constructor(
    private setupService: SetupService,
    private cdRef: ChangeDetectorRef,
    private confirmationService: ConfirmationService
  ) {
    this.erpOptions = [
      { label: 'Meta SimplestErp', logo: 'ml.jpeg' },
      { label: 'SAP', logo: 'sap.png' },
      { label: 'ERP X', logo: 'erp.png' },
    ];
  }

  async ngOnInit() {
    this.loadSuppliers();
    await this.loadSettings();
    this.selectedErp = this.erpOptions.find(
      (e) => e.label === this.settings.find((s) => s.name === 'ERP')?.value
    );
    this.loadAasUrls();
  }

  async loadSuppliers() {
    this.suppliers = await this.setupService.getSuppliers();
  }

  async loadSettings() {
    this.settings = await this.setupService.getSettings();
  }

  loadAasUrls() {
    this.aasRepositoryUrl = this.settings.find((s) => s.name === 'AASRepositoryUrl')?.value || '';
    this.aasRegistryUrl = this.settings.find((s) => s.name === 'AASRegistryUrl')?.value || '';
    this.submodelRepositoryUrl = this.settings.find((s) => s.name === 'SubmodelRepositoryUrl')?.value || '';
    this.submodelRegistryUrl = this.settings.find((s) => s.name === 'SubmodelRegistryUrl')?.value || '';
    this.discoveryUrl = this.settings.find((s) => s.name === 'DiscoveryUrl')?.value || '';
    this.conceptDescriptionUrl = this.settings.find((s) => s.name === 'ConceptDescriptionUrl')?.value || '';
  }

  showAddSupplierDialog() {
    this.newSupplier = {} as Supplier;
    this.addSupplierDialogVisible = true;
  }

  async saveSupplier() {
    if (this.newSupplier === undefined) return;
    const newSuppl = await this.setupService.saveSupplier(this.newSupplier);
    this.suppliers.push(newSuppl);
    this.addSupplierDialogVisible = false;
    this.cdRef.markForCheck();
  }

  async deleteSupplier(supplier: Supplier) {
    this.confirmationService.confirm({
      message: `Soll der Lieferant ${supplier.name} wirklich gelöscht werden?`,
      accept: async () => {
        await this.setupService.deleteSupplier(supplier);
        this.suppliers = this.suppliers.filter((s) => s.id !== supplier.id);
        this.cdRef.markForCheck();
      },
    });
  }

  async saveSettings() {
    const setting: Setting = {
      name: 'ERP',
      value: this.selectedErp?.label || '',
    };
    await this.setupService.saveSetting(setting);
  }

  async saveAasUrls() {
    const urlSettings: Setting[] = [
      { name: 'AASRepositoryUrl', value: this.aasRepositoryUrl },
      { name: 'AASRegistryUrl', value: this.aasRegistryUrl },
      { name: 'SubmodelRepositoryUrl', value: this.submodelRepositoryUrl },
      { name: 'SubmodelRegistryUrl', value: this.submodelRegistryUrl },
      { name: 'DiscoveryUrl', value: this.discoveryUrl },
      { name: 'ConceptDescriptionUrl', value: this.conceptDescriptionUrl }
    ];

    for (const setting of urlSettings) {
      await this.setupService.saveSetting(setting);
    }

    // Update local settings array
    await this.loadSettings();
  }
}
