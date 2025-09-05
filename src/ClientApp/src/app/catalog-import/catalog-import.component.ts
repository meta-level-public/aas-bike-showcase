import { AssetAdministrationShell } from '@aas-core-works/aas-core3.0-typescript/types';
import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgxJsonViewerModule } from 'ngx-json-viewer';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PopoverModule } from 'primeng/popover';
import { TableModule } from 'primeng/table';
import { Kategorie } from '../catalog-list/kategorie';
import { InventoryStatus } from '../model/inventory-status';
import { KatalogEintrag } from '../model/katalog-eintrag';
import { Supplier } from '../model/supplier';
import { NotificationService } from '../notification.service';
import { RepositoryService } from '../repository/repository.service';
import { SetupService } from '../setup/setup.service';
import { CatalogImportService } from './catalog-import.service';

@Component({
  selector: 'app-catalog-import',
  templateUrl: './catalog-import.component.html',
  styleUrl: './catalog-import.component.css',
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    InputTextModule,
    ButtonModule,
    PopoverModule,
    NgxJsonViewerModule,
  ],
})
export class CatalogImportComponent implements OnInit {
  kategorieOptions: Kategorie[] = [];
  aasId: string = '';
  shells: any;
  newKatalogEintrag: KatalogEintrag = {} as KatalogEintrag;
  currentRepositoryUrl: string = '';

  suppliers: Supplier[] = [];
  loading: boolean = false;

  constructor(
    private catalogService: CatalogImportService,
    private repositoryService: RepositoryService,
    private setupService: SetupService,
    private notificationService: NotificationService
  ) {
    this.kategorieOptions = Object.values(Kategorie);
  }

  ngOnInit(): void {
    this.loadSuppliers();
  }

  async loadSuppliers() {
    this.suppliers = await this.setupService.getSuppliers();
  }

  async loadRegistry(url: string) {
    this.currentRepositoryUrl = url;
    try {
      if (this.aasId === '') {
        this.loading = true;
        this.shells = await this.repositoryService.loadShells(url);
      } else {
        this.shells = await this.repositoryService.loadShell(url, this.aasId);
      }
    } finally {
      this.loading = false;
    }
  }

  apply(shell: AssetAdministrationShell) {
    this.newKatalogEintrag.aasId = shell.id;
    this.newKatalogEintrag.globalAssetId = shell.assetInformation?.globalAssetId ?? '';
    this.newKatalogEintrag.name = shell.idShort ?? shell.id;
  }

  async save() {
    try {
      this.loading = true;
      this.newKatalogEintrag.remoteRepositoryUrl = this.currentRepositoryUrl;
      this.newKatalogEintrag.inventoryStatus = InventoryStatus.OUTOFSTOCK;
      this.newKatalogEintrag.price = Number(
        String(this.newKatalogEintrag.price as any).replace(',', '.')
      );
      this.newKatalogEintrag.supplier =
        this.suppliers.find((s) => s.remoteAasRepositoryUrl === this.currentRepositoryUrl) ??
        ({} as Supplier);
      await this.catalogService.save(this.newKatalogEintrag);
      this.notificationService.showMessageAlways('Eintrag erfolgreich gespeichert');
      this.newKatalogEintrag = {} as KatalogEintrag;
    } finally {
      this.loading = false;
    }
  }
}
