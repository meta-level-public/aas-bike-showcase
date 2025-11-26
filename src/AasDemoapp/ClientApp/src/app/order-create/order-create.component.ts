import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { InputNumberModule } from 'primeng/inputnumber';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { ToggleButtonModule } from 'primeng/togglebutton';
import { ConfigurationListService } from '../configuration-list/configuration-list.service';
import { Address } from '../model/address';
import { ConfiguredProduct } from '../model/configured-product';
import { CreateProductionOrder, ProductionOrderResponse } from '../model/production-order';
import { ProductionOrderListService } from '../production-order-list/production-order-list.service';
import {
  LeafletMapComponent,
  MapLocation,
} from '../shared/components/leaflet-map/leaflet-map.component';
import { formatAddressString, hasValidCoordinates } from '../shared/utils/address.utils';

@Component({
  selector: 'app-order-create',
  templateUrl: './order-create.component.html',
  styleUrl: './order-create.component.css',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    CardModule,
    ButtonModule,
    InputTextModule,
    InputNumberModule,
    ToastModule,
    ToggleButtonModule,
    LeafletMapComponent,
    TranslateModule,
  ],
  providers: [MessageService],
})
export class OrderCreateComponent implements OnInit, OnDestroy {
  product: ConfiguredProduct | null = null;
  orderForm: FormGroup;
  loading: boolean = false;
  productId: number | null = null;

  // Map properties
  mapLocation: MapLocation | undefined;
  currentAddress: string = '';

  // Geocoding properties
  isGeocodingLoading: boolean = false;
  mapClickMode: boolean = false;
  private geocodingTimeout: number | null = null;

  constructor(
    private fb: FormBuilder,
    private productionOrderService: ProductionOrderListService,
    private configurationService: ConfigurationListService,
    private messageService: MessageService,
    private route: ActivatedRoute,
    private router: Router,
    private translate: TranslateService
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
      lat: [undefined],
      long: [undefined],
    });
  }

  ngOnDestroy() {
    // Clean up any pending geocoding timeout
    if (this.geocodingTimeout) {
      clearTimeout(this.geocodingTimeout);
      this.geocodingTimeout = null;
    }
  }

  async ngOnInit() {
    // Produkt-ID aus Route-Parameter abrufen
    this.route.params.subscribe((params) => {
      const id = params['id'];
      if (id) {
        this.productId = +id;
        this.loadProduct();
      } else {
        this.messageService.add({
          severity: 'error',
          summary: this.translate.instant('order.create.errorTitle'),
          detail: this.translate.instant('order.create.errorNoProductId'),
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
          summary: this.translate.instant('order.create.errorTitle'),
          detail: this.translate.instant('order.create.errorProductNotFound'),
        });
        this.router.navigate(['/config/list']);
      }
    } catch (error) {
      console.error('Error loading product:', error);
      this.messageService.add({
        severity: 'error',
        summary: this.translate.instant('order.create.errorTitle'),
        detail: this.translate.instant('order.create.errorLoadingProduct'),
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
                lat: formValue.lat,
                long: formValue.long,
              }
            : undefined,
        };

        const response = await this.createProductionOrder(createOrderDto);

        if (response.success) {
          this.messageService.add({
            severity: 'success',
            summary: this.translate.instant('order.create.successTitle'),
            detail: this.translate.instant('order.create.successOrderCreated'),
          });

          // Nach erfolgreicher Erstellung zur Bestellliste navigieren
          setTimeout(() => {
            this.router.navigate(['/production-orders/list']);
          }, 1500);
        } else {
          this.messageService.add({
            severity: 'error',
            summary: this.translate.instant('order.create.errorTitle'),
            detail: response.message || this.translate.instant('order.create.errorCreatingOrder'),
          });
        }
      } catch (error) {
        console.error('Error creating order:', error);
        this.messageService.add({
          severity: 'error',
          summary: this.translate.instant('order.create.errorTitle'),
          detail: this.translate.instant('order.create.errorUnexpected'),
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
        message: this.translate.instant('order.create.errorCreatingOrder'),
        error: error instanceof Error ? error.message : 'Unknown error',
      };
    }
  }

  getTotalPrice(): number {
    const anzahl = this.orderForm.get('anzahl')?.value || 1;
    return (this.product?.price || 0) * anzahl;
  }

  onMapLocationClicked(location: MapLocation) {
    // Die Position für die Karte setzen
    this.mapLocation = location;

    // Formular mit Koordinaten aktualisieren
    this.orderForm.patchValue({
      lat: location.lat,
      long: location.lng,
    });

    // Direktes Reverse Geocoding für bessere Adressdaten
    this.performReverseGeocoding(location.lat, location.lng);
  }

  onMapClickModeChange(enabled: boolean) {
    this.mapClickMode = enabled;
  }

  toggleMapClickMode() {
    this.mapClickMode = !this.mapClickMode;
  }

  private async performReverseGeocoding(lat: number, lng: number) {
    this.isGeocodingLoading = true;

    try {
      const response = await fetch(
        `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&zoom=18&addressdetails=1`
      );
      const data = await response.json();

      if (data && data.address) {
        const address = data.address;
        // Formular mit strukturierten Adressdaten füllen
        this.orderForm.patchValue({
          strasse: this.buildStreetAddress(address),
          plz: address.postcode || '',
          ort: address.city || address.town || address.village || address.municipality || '',
          land: address.country || 'Deutschland',
          lat: lat,
          long: lng,
        });

        // Aktualisiere auch die currentAddress für die Karte
        this.currentAddress = data.display_name || '';

        // Erfolgs-Nachricht
        this.messageService.add({
          severity: 'success',
          summary: this.translate.instant('order.create.addressFound'),
          detail: this.translate.instant('order.create.addressAutoFilled'),
        });
      }
    } catch (error) {
      console.error('Reverse geocoding error:', error);
      this.messageService.add({
        severity: 'warn',
        summary: this.translate.instant('order.create.addressTitle'),
        detail: this.translate.instant('order.create.addressNotFound'),
      });
    } finally {
      this.isGeocodingLoading = false;
    }
  }
  private buildStreetAddress(address: any): string {
    // Verschiedene mögliche Straßen-Felder kombinieren
    const streetName = address.road || address.street || address.pedestrian || '';
    const houseNumber = address.house_number || '';

    if (streetName && houseNumber) {
      return `${streetName} ${houseNumber}`;
    } else if (streetName) {
      return streetName;
    }

    return '';
  }

  // Geocoding basierend auf Adressfeldern
  private scheduleGeocoding() {
    // Clear existing timeout
    if (this.geocodingTimeout) {
      clearTimeout(this.geocodingTimeout);
    }

    // Schedule new geocoding after 1 second delay (debouncing)
    this.geocodingTimeout = window.setTimeout(() => {
      this.performForwardGeocoding();
    }, 1000);
  }

  private async performForwardGeocoding() {
    const formValue = this.orderForm.value;
    const addr: Address = {
      name: formValue.name || '',
      vorname: formValue.vorname || '',
      strasse: formValue.strasse || '',
      plz: formValue.plz || '',
      ort: formValue.ort || '',
      land: formValue.land || '',
      lat: formValue.lat,
      long: formValue.long,
    };

    const addressString = formatAddressString(addr);

    // Only geocode if we have enough address information and no manual coordinates
    if (!addressString.trim() || addressString.length < 5) {
      return;
    }

    // Skip if user has manually set coordinates
    if (hasValidCoordinates(addr)) {
      // felder nochmal befüllen!
      formValue.lat = addr.lat;
      formValue.long = addr.long;
      this.orderForm.patchValue(formValue);
      return;
    }

    this.isGeocodingLoading = true;

    try {
      const encodedAddress = encodeURIComponent(addressString);
      const response = await fetch(
        `https://nominatim.openstreetmap.org/search?format=json&q=${encodedAddress}&limit=1`
      );

      if (!response.ok) {
        throw new Error('Geocoding request failed');
      }

      const results = await response.json();

      if (results && results.length > 0) {
        const result = results[0];
        const lat = parseFloat(result.lat);
        const lng = parseFloat(result.lon);

        // Update form with coordinates
        this.orderForm.patchValue({
          lat: lat,
          long: lng,
        });

        // Update map location
        this.mapLocation = {
          lat: lat,
          lng: lng,
          address: addressString,
          title: this.translate.instant('order.create.deliveryAddress'),
        };

        this.messageService.add({
          severity: 'success',
          summary: this.translate.instant('order.create.coordinatesFound'),
          detail: this.translate.instant('order.create.coordinatesAutoForAddress', {
            address: addressString,
          }),
        });
      }
    } catch (error) {
      console.error('Geocoding error:', error);
    } finally {
      this.isGeocodingLoading = false;
    }
  }

  // Method to manually clear coordinates
  clearCoordinates() {
    this.orderForm.patchValue({
      lat: undefined,
      long: undefined,
    });
    this.mapLocation = undefined;
  }

  // Method to manually trigger geocoding
  async geocodeAddress() {
    await this.performForwardGeocoding();
  }

  // Helper methods for template
  getMapLocation(): MapLocation | undefined {
    const formValue = this.orderForm.value;
    if (
      formValue.lat !== undefined &&
      formValue.long !== undefined &&
      !isNaN(formValue.lat) &&
      !isNaN(formValue.long)
    ) {
      return {
        lat: formValue.lat,
        lng: formValue.long,
        address: this.getFormattedAddress(),
        title: this.translate.instant('order.create.deliveryAddress'),
      };
    }
    return undefined;
  }

  getFormattedAddress(): string {
    const formValue = this.orderForm.value;
    const addr: Address = {
      name: formValue.name || '',
      vorname: formValue.vorname || '',
      strasse: formValue.strasse || '',
      plz: formValue.plz || '',
      ort: formValue.ort || '',
      land: formValue.land || '',
      lat: formValue.lat,
      long: formValue.long,
    };
    return formatAddressString(addr);
  }

  hasCoordinates(): boolean {
    const formValue = this.orderForm.value;
    return (
      formValue.lat !== undefined &&
      formValue.long !== undefined &&
      !isNaN(formValue.lat) &&
      !isNaN(formValue.long)
    );
  }

  hasAddressData(): boolean {
    const formValue = this.orderForm.value;
    return !!(formValue.strasse || formValue.plz || formValue.ort || formValue.land);
  }

  // Method to handle address field changes and trigger geocoding
  onAddressFieldChange() {
    this.scheduleGeocoding();
  }

  async searchAddress() {
    const formValue = this.orderForm.value;
    const addressString =
      `${formValue.strasse}, ${formValue.plz} ${formValue.ort}, ${formValue.land}`.trim();

    if (!addressString || addressString === ', , Deutschland') {
      this.messageService.add({
        severity: 'warn',
        summary: this.translate.instant('order.create.warningTitle'),
        detail: this.translate.instant('order.create.warningEnterAddress'),
      });
      return;
    }

    // Die Adresse an die Karte weiterleiten
    this.currentAddress = addressString;
  }
}
