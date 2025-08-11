import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { ConfigurationListService } from '../configuration-list/configuration-list.service';
import { ConfiguredProduct } from '../model/configured-product';
import { CreateProductionOrder, ProductionOrderResponse } from '../model/production-order';
import { ProductionOrderListService } from '../production-order-list/production-order-list.service';

@Component({
  selector: 'app-order-create',
  templateUrl: './order-create.component.html',
  styleUrl: './order-create.component.css',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    ToastModule
  ],
  providers: [MessageService]
})
export class OrderCreateComponent implements OnInit {
  product: ConfiguredProduct | null = null;
  orderForm: FormGroup;
  loading: boolean = false;
  productId: number | null = null;

  constructor(
    private fb: FormBuilder,
    private productionOrderService: ProductionOrderListService,
    private configurationService: ConfigurationListService,
    private messageService: MessageService,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.orderForm = this.fb.group({
      anzahl: [1, [Validators.required, Validators.min(1)]],
      // Adresse Felder
      name: [''],
      vorname: [''],
      strasse: [''],
      plz: [''],
      ort: [''],
      land: ['Deutschland']
    });
  }

  async ngOnInit() {
    // Produkt-ID aus Route-Parameter abrufen
    this.route.params.subscribe(params => {
      const id = params['id'];
      if (id) {
        this.productId = +id;
        this.loadProduct();
      } else {
        this.messageService.add({
          severity: 'error',
          summary: 'Fehler',
          detail: 'Keine Produkt-ID angegeben'
        });
        this.router.navigate(['/config/list']);
      }
    });
  }

  async loadProduct() {
    if (!this.productId) return;

    try {
      this.loading = true;
      const products = await this.configurationService.getAllFertigteil();
      this.product = products.find((p: ConfiguredProduct) => p.id === this.productId) || null;

      if (!this.product) {
        this.messageService.add({
          severity: 'error',
          summary: 'Fehler',
          detail: 'Produkt nicht gefunden'
        });
        this.router.navigate(['/config/list']);
      }
    } catch (error) {
      console.error('Error loading product:', error);
      this.messageService.add({
        severity: 'error',
        summary: 'Fehler',
        detail: 'Fehler beim Laden des Produkts'
      });
    } finally {
      this.loading = false;
    }
  }

  cancel() {
    this.router.navigate(['/config/list']);
  }

  async createOrder() {
    if (this.orderForm.valid && this.product?.id) {
      this.loading = true;

      try {
        const formValue = this.orderForm.value;

        // Nur Adresse hinzufügen wenn mindestens ein Adressfeld ausgefüllt ist
        const hasAddressInfo = formValue.name || formValue.vorname || formValue.strasse ||
                              formValue.plz || formValue.ort;

        const createOrderDto: CreateProductionOrder = {
          configuredProductId: this.product.id,
          anzahl: formValue.anzahl,
          address: hasAddressInfo ? {
            name: formValue.name,
            vorname: formValue.vorname,
            strasse: formValue.strasse,
            plz: formValue.plz,
            ort: formValue.ort,
            land: formValue.land
          } : undefined
        };

        const response = await this.createProductionOrder(createOrderDto);

        if (response.success) {
          this.messageService.add({
            severity: 'success',
            summary: 'Erfolgreich',
            detail: 'Bestellung wurde erfolgreich erstellt!'
          });

          // Nach erfolgreicher Erstellung zur Bestellliste navigieren
          setTimeout(() => {
            this.router.navigate(['/production-orders/list']);
          }, 1500);
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Fehler',
            detail: response.message || 'Fehler beim Erstellen der Bestellung'
          });
        }
      } catch (error) {
        console.error('Error creating order:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Fehler',
          detail: 'Unerwarteter Fehler beim Erstellen der Bestellung'
        });
      } finally {
        this.loading = false;
      }
    }
  }

  private async createProductionOrder(createDto: CreateProductionOrder): Promise<ProductionOrderResponse> {
    try {
      const response = await this.productionOrderService.createProductionOrder(createDto);
      return response;
    } catch (error) {
      return {
        success: false,
        message: 'Fehler beim Erstellen der Bestellung',
        error: error instanceof Error ? error.message : 'Unknown error'
      };
    }
  }

  getTotalPrice(): number {
    const anzahl = this.orderForm.get('anzahl')?.value || 1;
    return (this.product?.price || 0) * anzahl;
  }
}
