import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { UrlBase } from '../base/url-base';
import { DiscoveryService } from './discovery.service';

@Component({
  selector: 'app-discovery-component',
  templateUrl: './discovery.component.html',
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    InputTextModule,
    ButtonModule,
  ],
})
export class DiscoveryComponent extends UrlBase {
  assetId: string = '';
  aasIds: string[] = [];

  constructor(private discoveryService: DiscoveryService) {
    super();
  }

  async queryLocal() {
    this.aasIds = await this.discoveryService.query(
      this.localRegistryUrl,
      this.assetId,
    );
  }
  async queryRemote() {
    this.aasIds = await this.discoveryService.query(
      this.remoteRegistryUrl,
      this.assetId,
    );
  }
}
