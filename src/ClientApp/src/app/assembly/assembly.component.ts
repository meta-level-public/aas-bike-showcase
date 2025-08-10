import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
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
  imports: [CommonModule, FormsModule, InputTextModule, ButtonModule, SelectModule, DialogModule],
})
export class AssemblyComponent implements OnInit {
  // Signals für den State
  items = signal<ConfiguredProduct[]>([]);
  selectedItem = signal<ConfiguredProduct | undefined>(undefined);
  newProduct = signal<ProducedProductRequest>({} as ProducedProductRequest);
  loading = signal<boolean>(false);
  currentAssemblyStep = signal<number>(0);
  showSchraubenDialog = signal<boolean>(false);
  teileStatus = signal<{guid: string, typeGlobalAssetId: string, instanceGlobalAssetId:string, instanceAasId: string, statusOk: boolean, bestandteilId: number}[]>([]);

  // Computed signals
  isLenkerStep = computed(() => {
    const selected = this.selectedItem();
    const currentStep = this.currentAssemblyStep();
    if (selected?.bestandteile && currentStep < selected.bestandteile.length) {
      const stepName = selected.bestandteile[currentStep].name.toLowerCase();
      return stepName.includes('lenker') || stepName.includes('handlebar');
    }
    return false;
  });

  allPartsValid = computed(() => {
    const product = this.newProduct();
    if (product.bestandteilRequests != null) {
      return this.teileStatus().every(status => status.statusOk === true);
    }
    return false;
  });

  constructor(private service: AssemblyService, private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.loadData();
  }

  async loadData() {
    const data = await this.service.getAllFertigteil();
    this.items.set(data);
  }

  typeSelected() {
    const selectedItem = this.selectedItem();
    if (selectedItem) {
      console.log('Selected type:', selectedItem);
    }
    this.teileStatus.set([]);
    this.currentAssemblyStep.set(0); // Reset assembly step when new type is selected
    if (selectedItem?.bestandteile != null) {
      this.newProduct.update(product => ({
        ...product,
        bestandteilRequests: [],
        configuredProductId: selectedItem.id ?? 0
      }));

      const newTeileStatus: {guid: string, typeGlobalAssetId: string, instanceGlobalAssetId:string, instanceAasId: string, statusOk: boolean, bestandteilId: number}[] = [];
      selectedItem.bestandteile.forEach((bestandteil) => {
        for (let i = 0; i < bestandteil.amount; i++) {
          newTeileStatus.push({
            guid: guid(),
            typeGlobalAssetId: bestandteil.katalogEintrag?.globalAssetId ?? '',
            instanceGlobalAssetId: '',
            instanceAasId: bestandteil.katalogEintrag?.aasId ?? '',
            statusOk: false,
            bestandteilId: bestandteil.id ?? 0
          });
        }
      });
      this.teileStatus.set(newTeileStatus);
    }
  }

  async findRohteilInstanz(event: any, item: ProductPart, guid: string) {
    if (event.target.value == '') return;
    try {
      this.loading.set(true);
      const katalogEintrag = await this.service.getRohteilInstanz(event.target.value);
      if (katalogEintrag == null) {
        this.notificationService.showMessageAlways('Rohteil Instanz nicht gefunden');
      } else {
        console.log(this.teileStatus());
        const currentTeileStatus = this.teileStatus();
        const teileStatusItem = currentTeileStatus.find((teileStatus) => teileStatus.typeGlobalAssetId == item.katalogEintrag?.globalAssetId && teileStatus.guid == guid);
        console.log(teileStatusItem);
        if (teileStatusItem == null) return;
        if (katalogEintrag.referencedType?.globalAssetId != item.katalogEintrag?.globalAssetId) {
          this.notificationService.showMessageAlways('Rohteil hat den falschen Typ!');
        } else {
          // Update the specific item in the array
          this.teileStatus.update(statuses =>
            statuses.map(status =>
              status.guid === guid ? {
                ...status,
                instanceGlobalAssetId: katalogEintrag.globalAssetId,
                statusOk: true,
                instanceAasId: katalogEintrag.aasId
              } : status
            )
          );
        }
      }
    } finally {
      this.loading.set(false);
    }
  }


  getParts(item: ProductPart) {
    return this.teileStatus().filter((teileStatus) => teileStatus.typeGlobalAssetId == item.katalogEintrag?.globalAssetId);
  }

  isTeilOk(item: ProductPart) {
    return this.teileStatus().filter((teileStatus) => teileStatus.typeGlobalAssetId == item.katalogEintrag?.globalAssetId).every((teileStatus) => teileStatus.statusOk);
  }

  nextAssemblyStep() {
    const selectedItem = this.selectedItem();
    const currentStep = this.currentAssemblyStep();
    if (selectedItem && currentStep < selectedItem.bestandteile.length - 1) {
      this.currentAssemblyStep.set(currentStep + 1);
    }
  }

  // Prüft, ob der aktuelle Montageschritt "Lenker" im Namen enthält - entfernt da bereits als computed signal definiert

  // Öffnet den Schrauben-Dialog
  openSchraubenDialog(): void {
    this.showSchraubenDialog.set(true);
  }

  // Schließt den Schrauben-Dialog
  closeSchraubenDialog(): void {
    this.showSchraubenDialog.set(false);
  }

  async saveProducedProduct() {
    const currentProduct = this.newProduct();
    if (currentProduct == null) return;

    const currentTeileStatus = this.teileStatus();
    const selectedItem = this.selectedItem();

    const bestandteilRequests = currentTeileStatus.map((teileStatus) => {
      const typePart = selectedItem?.bestandteile?.find((bestandteil) => bestandteil.katalogEintrag?.globalAssetId == teileStatus.typeGlobalAssetId);
      if (typePart == null) return null;
      return {
        amount: 1,
        usageDate: new Date(),
        globalAssetId: teileStatus.instanceGlobalAssetId,
      };
    }).filter(req => req !== null);

    const productToSave = {
      ...currentProduct,
      bestandteilRequests
    };

    try {
      this.loading.set(true);
      await this.service.createProduct(productToSave);
      this.notificationService.showMessageAlways(
        'Produkt erfolgreich erstellt'
      );
      this.newProduct.set({} as ProducedProductRequest);
    } finally {
      this.loading.set(false);
    }
  }
}
