
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
    this.cdRef.markForCheck();
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
      } finally {
        this.loading = false;
      }
    }

    calculatePrice() {
      let price = 0;
      this.bestandteile.forEach((bestandteil) => {
        price += bestandteil.price;
      });

      this.newProduct.price = price * 1.3; // bisschen was draufschlagen ;)
      console.log(this.bestandteile);
  }
}
