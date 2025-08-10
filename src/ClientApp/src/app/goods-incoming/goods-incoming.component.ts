import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { CatalogItemComponent } from '../catalog-list/catalog-item/catalog-item.component';
import { KatalogEintrag } from '../model/katalog-eintrag';
import { NotificationService } from '../notification.service';
import { GoodsIncomingService } from './goods-incoming.service';

@Component({
  selector: 'app-goods-incoming',
  templateUrl: './goods-incoming.component.html',
  styleUrl: './goods-incoming.component.css',
  imports: [CommonModule, FormsModule, InputTextModule, CatalogItemComponent, ButtonModule]
})
export class GoodsIncomingComponent {
  newKatalogEintrag: KatalogEintrag = {} as KatalogEintrag;
  parentRohteil: KatalogEintrag | undefined;
  loading: boolean = false;
  importImageUrl: string = '';
  loaded: boolean = false;

  // LKW Animation Properties
  showTruck: boolean = false;
  truckPosition: string = 'start';
  showBox: boolean = false;
  boxText: string = 'Lieferung #001';
  isBoxClickable: boolean = false;

  constructor(
    private goodsIncomingService: GoodsIncomingService,
    private notificationService: NotificationService
  ) {}

  async lookup() {
    try {
      this.loading = true;
      this.importImageUrl = '';
      this.loaded = false;
      // Typ des Rohteils suchen
      // aas des Lieferanten suchen
      const res = await this.goodsIncomingService.lookupRohteil(
        this.newKatalogEintrag.globalAssetId
      );

      this.parentRohteil = res.typeKatalogEintrag;
      this.newKatalogEintrag.aasId = res.aasId;
      this.newKatalogEintrag.price = this.parentRohteil.price;
      this.newKatalogEintrag.name = this.parentRohteil.name;
      this.newKatalogEintrag.kategorie = this.parentRohteil.kategorie;
      this.newKatalogEintrag.referencedType = this.parentRohteil;
      this.newKatalogEintrag.remoteRepositoryUrl =
        this.parentRohteil.remoteRepositoryUrl;

      this.importImageUrl =
        this.parentRohteil.remoteRepositoryUrl +
        '/shells/' +
        btoa(this.newKatalogEintrag.aasId) +
        '/asset-information/thumbnail';

      this.loaded = true;
    } finally {
      this.loading = false;
    }
  }

  async createRohteilInstanz() {
    if (this.parentRohteil == null) return;


    try {
      this.loading = true;

      await this.goodsIncomingService.createRohteilInstanz(
        this.newKatalogEintrag
      );
      this.notificationService.showMessageAlways(
        'Wareneingang erfolgreich gebucht'
      );
      this.newKatalogEintrag = {} as KatalogEintrag;
      this.parentRohteil = undefined;
      this.loaded = false;
      // Erst die Lieferung abschließen
      this.completeDelivery();
    } finally {
      this.loading = false;
    }
  }

  // LKW Animation Methods
  async startDelivery() {
    console.log('Start delivery animation');

    const randomRohteil = await this.goodsIncomingService.getRandomRohteil();

    this.boxText = randomRohteil.id;

    this.showTruck = true;
    this.showBox = true;
    this.truckPosition = 'moving';
    this.isBoxClickable = false;

    // LKW fährt zur Mitte und stoppt nach 2 Sekunden
    setTimeout(() => {
      this.truckPosition = 'center';
      this.isBoxClickable = true;
    }, 2000);
  }

  onBoxClick() {
    if (this.isBoxClickable) {
      // Text in das Input-Feld schreiben
      this.newKatalogEintrag.globalAssetId = this.boxText;
      this.lookup();
    }
  }

  completeDelivery() {
    // Kiste entfernen und LKW weiterfahren lassen
    this.showBox = false;
    this.isBoxClickable = false;
    this.truckPosition = 'leaving';

    // Nach Animation zurücksetzen
    setTimeout(() => {
      this.showTruck = false;
      this.truckPosition = 'start';
    }, 2000);
  }

  resetDelivery() {
    // Alle Animationszustände zurücksetzen
    this.showTruck = false;
    this.showBox = false;
    this.truckPosition = 'start';
    this.isBoxClickable = false;

    // Form-Daten zurücksetzen
    this.newKatalogEintrag = {} as KatalogEintrag;
    this.parentRohteil = undefined;
    this.loaded = false;
    this.importImageUrl = '';

  }
}
