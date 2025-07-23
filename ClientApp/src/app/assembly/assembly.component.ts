import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DropdownModule } from 'primeng/dropdown';
import { InputTextModule } from 'primeng/inputtext';
import { v4 as guid } from 'uuid';
import { ConfiguredProduct } from '../model/configured-product';
import { ProducedProductRequest } from '../model/produced-product-request';
import { ProductPart } from '../model/product-part';
import { NotificationService } from '../notification.service';
import { AssemblyService } from './assembly.service';
@Component({
  selector: 'app-assembly',
  templateUrl: './assembly.component.html',
  styleUrl: './assembly.component.css',
  imports: [CommonModule, FormsModule, InputTextModule, ButtonModule, DropdownModule],
})
export class AssemblyComponent implements OnInit {
  items: ConfiguredProduct[] = [];
  selectedItem: ConfiguredProduct | undefined;

  newProduct: ProducedProductRequest = {} as ProducedProductRequest;
  loading: boolean = false;

  teileStatus: {guid: string, typeGlobalAssetId: string, instanceGlobalAssetId:string, instanceAasId: string, statusOk: boolean, bestandteilId: number}[] = [];

  constructor(private service: AssemblyService, private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.loadData();
  }

  async loadData() {
    this.items = await this.service.getAllFertigteil();
  }

  typeSelected() {
    if (this.selectedItem) {
      console.log('Selected type:', this.selectedItem);
    }
    if (this.selectedItem?.bestandteile != null) {
      this.newProduct.bestandteile = [];
      this.newProduct.configuredProductId = this.selectedItem.id ?? 0;
      this.selectedItem.bestandteile.forEach((bestandteil) => {
        for (let i = 0; i < bestandteil.amount; i++) {
          this.teileStatus.push({guid: guid(), typeGlobalAssetId: bestandteil.katalogEintrag?.globalAssetId ?? '', instanceGlobalAssetId: '', instanceAasId: bestandteil.katalogEintrag?.aasId ?? '', statusOk: false, bestandteilId: bestandteil.id ?? 0});
        }
      });
    }
  }

  async findRohteilInstanz(event: any, item: ProductPart, guid: string) {
    if (event.target.value == '') return;
    try {
      this.loading = true;
      const katalogEintrag = await this.service.getRohteilInstanz(event.target.value);
      if (katalogEintrag == null) {
        this.notificationService.showMessageAlways('Rohteil Instanz nicht gefunden');
      } else {
        console.log(this.teileStatus);
        const teileStatus = this.teileStatus.find((teileStatus) => teileStatus.typeGlobalAssetId == item.katalogEintrag?.globalAssetId && teileStatus.guid == guid);
        console.log(teileStatus);
        if (teileStatus == null) return;
        if (katalogEintrag.referencedType?.globalAssetId != item.katalogEintrag?.globalAssetId) {
          this.notificationService.showMessageAlways('Rohteil hat den falschen Typ!');
        } else {
          teileStatus.instanceGlobalAssetId = katalogEintrag.globalAssetId;
          teileStatus.statusOk = true;
          teileStatus.instanceAasId = katalogEintrag.aasId;
        }
      }
    } finally {
      this.loading = false;
    }
  }


  getParts(item: ProductPart) {
    return this.teileStatus.filter((teileStatus) => teileStatus.typeGlobalAssetId == item.katalogEintrag?.globalAssetId);
  }

  isTeilOk(item: ProductPart) {
    return this.teileStatus.filter((teileStatus) => teileStatus.typeGlobalAssetId == item.katalogEintrag?.globalAssetId).every((teileStatus) => teileStatus.statusOk);
  }

  async saveProducedProduct() {
    if (this.newProduct == null) return;
    this.newProduct.bestandteile = [];
    console.log(this.teileStatus);
    console.log(this.selectedItem);
    this.teileStatus.forEach((teileStatus) => {
      const typePart = this.selectedItem?.bestandteile?.find((bestandteil) => bestandteil.katalogEintrag?.globalAssetId == teileStatus.typeGlobalAssetId);
      console.log(typePart);
      if (typePart == null) return;
      this.newProduct.bestandteile?.push({

        amount: 1,
        usageDate: new Date(),
        globalAssetId: teileStatus.instanceGlobalAssetId,
      });
    });

    try {
      this.loading = true;
      await this.service.createProduct(this.newProduct);
      this.notificationService.showMessageAlways(
        'Produkt erfolgreich erstellt'
        );
        this.newProduct = {} as ProducedProductRequest;
      } finally {
        this.loading = false;
      }

  }

  allPartsValid() {
    let valid = false;
    if (this.newProduct.bestandteile != null) {
      valid = this.teileStatus.every(
        (teileStatus) => teileStatus.statusOk == true
      );
    }
    return valid;
  }
}
