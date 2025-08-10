
import { CommonModule } from '@angular/common';
import {
    ChangeDetectorRef,
    Component,
    OnInit
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { PickListModule } from 'primeng/picklist';
import { ConfiguredProduct } from '../model/configured-product';
import { KatalogEintrag } from '../model/katalog-eintrag';
import { NotificationService } from '../notification.service';
import { ConfigurationCreateService } from './configuration-create.service';

@Component({
  selector: 'app-configuration-create',
  templateUrl: './configuration-create.component.html',
  styleUrl: './configuration-create.component.css',
  imports: [CommonModule, FormsModule, PickListModule, InputTextModule, ButtonModule]
})
export class ConfigurationCreateComponent implements OnInit {
  rohteile: KatalogEintrag[] = [];
  bestandteile: KatalogEintrag[] = [];

  newProduct: ConfiguredProduct = {} as ConfiguredProduct;
  loading: boolean = false;

  constructor(
    private service: ConfigurationCreateService,
    private cdRef: ChangeDetectorRef,
    private notificationService: NotificationService
  ) {}

  async ngOnInit() {
    this.rohteile = await this.service.getAllRohteil();
    this.newProduct.price = 0;
    this.cdRef.markForCheck();
  }

  // Neuen Rohteil zur Konfiguration hinzufügen
  addToConfiguration(rohteil: KatalogEintrag) {
    // Prüfen ob Teil bereits in der Konfiguration ist
    const existingTeil = this.bestandteile.find(b => b.id === rohteil.id);

    if (existingTeil) {
      // Wenn bereits vorhanden, Anzahl erhöhen
      existingTeil.amountToUse = (existingTeil.amountToUse || 1) + 1;
    } else {
      // Neues Teil hinzufügen mit Standardmenge 1
      const newTeil = { ...rohteil, amountToUse: 1 };
      this.bestandteile.push(newTeil);
    }

    this.calculatePrice();
    this.cdRef.markForCheck();
  }

  // Teil aus Konfiguration entfernen
  removeFromConfiguration(teil: KatalogEintrag) {
    const index = this.bestandteile.findIndex(b => b.id === teil.id);
    if (index > -1) {
      this.bestandteile.splice(index, 1);
      this.calculatePrice();
      this.cdRef.markForCheck();
    }
  }

  // Menge erhöhen
  increaseQuantity(teil: KatalogEintrag) {
    teil.amountToUse = (teil.amountToUse || 1) + 1;
    this.calculatePrice();
  }

  // Menge verringern
  decreaseQuantity(teil: KatalogEintrag) {
    if (teil.amountToUse > 1) {
      teil.amountToUse = teil.amountToUse - 1;
      this.calculatePrice();
    }
  }

  // Manuelle Mengenänderung
  onQuantityChange(teil: KatalogEintrag, newQuantity: number) {
    if (newQuantity < 1) {
      teil.amountToUse = 1;
    } else {
      teil.amountToUse = newQuantity;
    }
    this.calculatePrice();
  }

  // Komplette Konfiguration löschen
  clearConfiguration() {
    this.bestandteile = [];
    this.newProduct.name = '';
    this.calculatePrice();
    this.cdRef.markForCheck();
  }

  // TrackBy-Funktion für bessere Performance bei *ngFor
  trackByFn(index: number, item: KatalogEintrag): any {
    return item.id;
  }

  async createProduct() {
    if (this.newProduct == null) return;
    this.newProduct.bestandteile = [];
    this.bestandteile.forEach((bestandteil) => {
      this.newProduct.bestandteile?.push({
        id: undefined,
        katalogEintragId: bestandteil.id,
        name: bestandteil.name,
        price: bestandteil.price,
        amount: bestandteil.amountToUse,
        usageDate: new Date(),
      });
    });

    try {
      this.loading = true;
      await this.service.createProduct(this.newProduct);
      this.notificationService.showMessageAlways(
        'Konfiguration erfolgreich erstellt'
        );
        this.newProduct = {} as ConfiguredProduct;
        this.bestandteile = [];
        this.calculatePrice();
      } finally {
        this.loading = false;
      }
    }

    calculatePrice() {
      let price = 0;
      this.bestandteile.forEach((bestandteil) => {
        const amount = bestandteil.amountToUse || 1;
        price += bestandteil.price * amount;
      });

      this.newProduct.price = Math.round(price * 1.3 * 100) / 100; // bisschen was draufschlagen ;)
      console.log('Bestandteile:', this.bestandteile);
      console.log('Gesamtpreis:', this.newProduct.price);
  }

  valid() {
    return (this.newProduct.name != null &&
      this.newProduct.name.length > 0 &&
      this.bestandteile.length > 0 &&
      this.bestandteile.every(b => (b.amountToUse || 1) > 0)
    );
  }
}
