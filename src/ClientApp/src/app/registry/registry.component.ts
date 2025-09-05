import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { TableModule } from 'primeng/table';
import { UrlBase } from '../base/url-base';
import { RegistryService } from './registry.service';

@Component({
  selector: 'app-registry-component',
  templateUrl: './registry.component.html',
  imports: [
    CommonModule,
    FormsModule,
    TableModule,
    ButtonModule,
    InputTextModule,
  ],
})
export class RegistryComponent extends UrlBase {
  aasId: string = '';
  descriptors: any[] = [];

  constructor(private registryService: RegistryService) {
    super();
  }

  async queryLocal() {
    this.descriptors = [];
    this.descriptors = await this.registryService.loadShells(
      this.localRegistryUrl,
      this.aasId,
    );
  }
  async queryRemote() {
    this.descriptors = [];
    this.descriptors = await this.registryService.loadShells(
      this.remoteRegistryUrl,
      this.aasId,
    );
  }

  getSubmodels(descriptor: any) {
    return descriptor.submodels?.map((submodel: any) =>
      submodel?.keys?.map((key: any) => key.value),
    );
  }

  getSpecificAssetIds(descriptor: any) {
    return descriptor.assetInformation?.specificAssetIds?.map(
      (a: any) => a.name + ': ' + a.value,
    );
  }
}
