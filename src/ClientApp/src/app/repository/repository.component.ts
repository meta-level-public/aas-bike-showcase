import { AssetAdministrationShell } from '@aas-core-works/aas-core3.0-typescript/types';
import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NgxJsonViewerModule } from 'ngx-json-viewer';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PopoverModule } from 'primeng/popover';
import { TableModule } from 'primeng/table';
import { UrlBase } from '../base/url-base';
import { RepositoryService } from './repository.service';

@Component({
  selector: 'app-fetch-data',
  templateUrl: './repository.component.html',
  imports: [CommonModule, FormsModule, TableModule, PopoverModule, NgxJsonViewerModule, ButtonModule, InputTextModule]
})
export class RepositoryComponent extends UrlBase{

  shells: (AssetAdministrationShell | null)[] = [];
  isLocal = false;
  aasId = '';

  constructor(private repositoryService: RepositoryService) {
    super();
  }

  async loadLocalRegistry() {
    if (this.aasId === '') {
    this.shells = await this.repositoryService.loadShells(
      this.localRegistryUrl
    );
    } else {
      this.shells = await this.repositoryService.loadShell(
        this.localRegistryUrl, this.aasId
      );
    }
    this.isLocal = true;
  }

  async loadRemoteRegistry() {
    if (this.aasId === '') {
      this.shells = await this.repositoryService.loadShells(
        this.remoteRegistryUrl
      );
      } else {
        this.shells = await this.repositoryService.loadShell(
          this.remoteRegistryUrl, this.aasId
        );
      }
      this.isLocal = false;
  }

  import(aasId: string) {
    this.repositoryService.import(
      this.localRegistryUrl,
      this.remoteRegistryUrl,
      aasId
    );
  }
}
