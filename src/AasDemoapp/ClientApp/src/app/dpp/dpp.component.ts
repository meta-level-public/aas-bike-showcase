import { AssetAdministrationShell } from '@aas-core-works/aas-core3.0-typescript/types';
import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { NgxJsonViewerModule } from 'ngx-json-viewer';
import { ButtonModule } from 'primeng/button';
import { DataViewModule } from 'primeng/dataview';
import { PopoverModule } from 'primeng/popover';
import { TooltipModule } from 'primeng/tooltip';
import { UrlBase } from '../base/url-base';
import { ProducedProduct } from '../model/produced-product';
import { DppService } from './dpp.service';

@Component({
  selector: 'app-dpp',
  templateUrl: './dpp.component.html',
  styleUrl: './dpp.component.css',
  imports: [
    CommonModule,
    FormsModule,
    PopoverModule,
    DataViewModule,
    NgxJsonViewerModule,
    ButtonModule,
    TooltipModule,
    TranslateModule,
  ],
})
export class DppComponent extends UrlBase implements OnInit {
  shell: AssetAdministrationShell | null = null;
  productList: ProducedProduct[] = [];
  loading: boolean = false;

  constructor(private dppService: DppService) {
    super();
  }

  ngOnInit() {
    this.loadData();
  }

  async loadData() {
    this.productList = await this.dppService.getAll();
  }

  async loadShell(productId: number, op: any, event: any) {
    try {
      this.loading = true;
      this.shell = null;
      op.toggle(event);
      this.shell = await this.dppService.loadShell(productId);
    } finally {
      this.loading = false;
    }
  }

  async deleteProduct(id: number) {
    try {
      await this.dppService.deleteProduct(id);
      this.loadData();
    } catch (error) {
      console.error('Error deleting product:', error);
    }
  }

  toDate(dateString: string) {
    return new Date(dateString);
  }

  getQrCodeUrl(aasId: string): string {
    return `/api/QrCode/GenerateWithFrame?id=${encodeURIComponent(aasId)}`;
  }
}
