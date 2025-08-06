import { AssetAdministrationShell } from '@aas-core-works/aas-core3.0-typescript/types';
import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgxJsonViewerModule } from 'ngx-json-viewer';
import { ButtonModule } from 'primeng/button';
import { OverlayPanelModule } from 'primeng/overlaypanel';
import { TableModule } from 'primeng/table';
import { UrlBase } from '../base/url-base';
import { ProducedProduct } from '../model/produced-product';
import { DppService } from './dpp.service';

@Component({
  selector: 'app-dpp',
  templateUrl: './dpp.component.html',
  styleUrl: './dpp.component.css',
  imports: [CommonModule, FormsModule, OverlayPanelModule, TableModule, NgxJsonViewerModule, ButtonModule]
})
export class DppComponent  extends UrlBase implements OnInit {
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

  async loadShell(aasId: string, op: any, event: any) {
    try {
      this.loading = true;
      this.shell = null;
      op.toggle(event);
      this.shell = await this.dppService.loadShell(
        this.localRegistryUrl,
        aasId
      );
    } finally {
      this.loading = false;
    }
  }

  toDate(dateString: string) {
    return new Date(dateString);
  }
}
