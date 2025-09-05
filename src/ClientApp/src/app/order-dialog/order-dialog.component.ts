import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { ConfiguredProduct } from '../model/configured-product';
import { CreateProductionOrder, ProductionOrderResponse } from '../model/production-order';
import { ProductionOrderListService } from '../production-order-list/production-order-list.service';

@Component({
  selector: 'app-order-dialog',
  templateUrl: './order-dialog.component.html',
  styleUrl: './order-dialog.component.css',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DialogModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    ToastModule,
  ],
  providers: [MessageService],
})
export class OrderDialogComponent implements OnChanges {
  @Input() visible: boolean = false;
  @Input() product: ConfiguredProduct | null = null;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() orderCreated = new EventEmitter<void>();

  orderForm: FormGroup;
  loading: boolean = false;

  constructor(
    private fb: FormBuilder,
    private productionOrderService: ProductionOrderListService,
    private messageService: MessageService
  ) {
    this.orderForm = this.fb.group({
      anzahl: [1, [Validators.required, Validators.min(1)]],
      // Adresse Felder
      name: [''],
      vorname: [''],
      strasse: [''],
      plz: [''],
      ort: [''],
      land: ['Deutschland'],
    });
  }

  ngOnChanges() {
    if (this.visible && this.product) {
      this.orderForm.patchValue({
        anzahl: 1,
      });
    }
  }

  hideDialog() {
    this.visible = false;
    this.visibleChange.emit(false);
    this.orderForm.reset({
      anzahl: 1,
      land: 'Deutschland',
    });
  }

  async createOrder() {
    if (this.orderForm.valid && this.product?.id) {
      this.loading = true;

      try {
        const formValue = this.orderForm.value;

        // Nur Adresse hinzufügen wenn mindestens ein Adressfeld ausgefüllt ist
        const hasAddressInfo =
          formValue.name ||
          formValue.vorname ||
          formValue.strasse ||
          formValue.plz ||
          formValue.ort;

        const createOrderDto: CreateProductionOrder = {
          configuredProductId: this.product.id,
          anzahl: formValue.anzahl,
          address: hasAddressInfo
            ? {
                name: formValue.name,
                vorname: formValue.vorname,
                strasse: formValue.strasse,
                plz: formValue.plz,
                ort: formValue.ort,
                land: formValue.land,
              }
            : undefined,
        };

        const response = await this.createProductionOrder(createOrderDto);

        if (response.success) {
          this.messageService.add({
            severity: 'success',
            summary: 'Erfolgreich',
            detail: 'Bestellung wurde erfolgreich erstellt!',
          });
          this.orderCreated.emit();
          this.hideDialog();
        } else {
          this.messageService.add({
            severity: 'error',
            summary: 'Fehler',
            detail: response.message || 'Fehler beim Erstellen der Bestellung',
          });
        }
      } catch (error) {
        console.error('Error creating order:', error);
        this.messageService.add({
          severity: 'error',
          summary: 'Fehler',
          detail: 'Unerwarteter Fehler beim Erstellen der Bestellung',
        });
      } finally {
        this.loading = false;
      }
    }
  }

  private async createProductionOrder(
    createDto: CreateProductionOrder
  ): Promise<ProductionOrderResponse> {
    try {
      const response = await this.productionOrderService.createProductionOrder(createDto);
      return response;
    } catch (error) {
      return {
        success: false,
        message: 'Fehler beim Erstellen der Bestellung',
        error: error instanceof Error ? error.message : 'Unknown error',
      };
    }
  }

  getTotalPrice(): number {
    const anzahl = this.orderForm.get('anzahl')?.value || 1;
    return (this.product?.price || 0) * anzahl;
  }
}
