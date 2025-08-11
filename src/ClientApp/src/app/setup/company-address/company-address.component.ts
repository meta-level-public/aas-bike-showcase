import { CommonModule } from '@angular/common';
import { Component, input, OnChanges, OnDestroy, OnInit, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccordionModule } from 'primeng/accordion';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { ToggleButtonModule } from 'primeng/togglebutton';
import { Address } from '../../model/address';
import { LeafletMapComponent, MapLocation } from '../../shared/components/leaflet-map/leaflet-map.component';
import { formatAddressString, hasValidCoordinates } from '../../shared/utils/address.utils';
import { Setting } from '../setting';
import { SetupService } from '../setup.service';

@Component({
  selector: 'app-company-address',
  templateUrl: './company-address.component.html',
  styleUrl: './company-address.component.css',
  imports: [
    CommonModule,
    FormsModule,
    AccordionModule,
    ButtonModule,
    InputTextModule,
    ToastModule,
    ToggleButtonModule,
    LeafletMapComponent,
  ],
  providers: [MessageService]
})
export class CompanyAddressComponent implements OnInit, OnChanges, OnDestroy {
  // Input signals
  settings = input<Setting[]>([]);

  // Output events
  settingsUpdated = output<void>();

  // Address signals
  address = signal<Address>({
    name: '',
    vorname: '',
    strasse: '',
    plz: '',
    ort: '',
    land: '',
    lat: undefined,
    long: undefined
  });

  // Loading state for geocoding
  isGeocodingLoading = signal(false);

  // Map click-to-set mode
  mapClickMode = signal(false);

  // Debounce timer for geocoding
  private geocodingTimeout: number | null = null;

  constructor(
    private setupService: SetupService,
    private messageService: MessageService
  ) {}

  ngOnInit() {
    this.loadCompanyAddress();
  }

  ngOnChanges() {
    this.loadCompanyAddress();
  }

  ngOnDestroy() {
    // Clean up any pending geocoding timeout
    if (this.geocodingTimeout) {
      clearTimeout(this.geocodingTimeout);
      this.geocodingTimeout = null;
    }
  }

  loadCompanyAddress() {
    const currentSettings = this.settings();
    const addressSetting = currentSettings.find(s => s.name === 'CompanyAddress');

    if (addressSetting?.value) {
      try {
        const savedAddress = JSON.parse(addressSetting.value) as Address;
        this.address.set(savedAddress);
      } catch (error) {
        console.error('Error parsing company address:', error);
        this.resetAddress();
      }
    } else {
      this.resetAddress();
    }
  }

  resetAddress() {
    this.address.set({
      name: '',
      vorname: '',
      strasse: '',
      plz: '',
      ort: '',
      land: '',
      lat: undefined,
      long: undefined
    });
  }

  async saveAddress() {
    try {
      const currentAddress = this.address();
      const addressJson = JSON.stringify(currentAddress);

      const setting: Setting = {
        name: 'CompanyAddress',
        value: addressJson
      };

      await this.setupService.saveSetting(setting);

      this.messageService.add({
        severity: 'success',
        summary: 'Erfolg',
        detail: 'Firmenadresse erfolgreich gespeichert'
      });

      this.settingsUpdated.emit();
    } catch (error) {
      console.error('Error saving company address:', error);
      this.messageService.add({
        severity: 'error',
        summary: 'Fehler',
        detail: 'Fehler beim Speichern der Firmenadresse'
      });
    }
  }

  updateAddress(field: keyof Address, value: string | number | undefined) {
    const currentAddress = this.address();
    const updatedAddress = {
      ...currentAddress,
      [field]: value
    };

    this.address.set(updatedAddress);

    // Trigger geocoding for address fields (but not for coordinates or names)
    const addressFields: (keyof Address)[] = ['strasse', 'plz', 'ort', 'land'];
    if (addressFields.includes(field)) {
      this.scheduleGeocoding();
    }
  }

  private scheduleGeocoding() {
    // Clear existing timeout
    if (this.geocodingTimeout) {
      clearTimeout(this.geocodingTimeout);
    }

    // Schedule new geocoding after 1 second delay (debouncing)
    this.geocodingTimeout = window.setTimeout(() => {
      this.performGeocoding();
    }, 1000);
  }

  private async performGeocoding() {
    const addr = this.address();
    const addressString = formatAddressString(addr);

    // Only geocode if we have enough address information and no manual coordinates
    if (!addressString.trim() || addressString.length < 5) {
      return;
    }

    // Skip if user has manually set coordinates
    if (hasValidCoordinates(addr)) {
      return;
    }

    this.isGeocodingLoading.set(true);

    try {
      const encodedAddress = encodeURIComponent(addressString);
      const response = await fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodedAddress}&limit=1`);

      if (!response.ok) {
        throw new Error('Geocoding request failed');
      }

      const results = await response.json();

      if (results && results.length > 0) {
        const result = results[0];
        const lat = parseFloat(result.lat);
        const lng = parseFloat(result.lon);

        // Update address with coordinates
        const currentAddress = this.address();
        this.address.set({
          ...currentAddress,
          lat: lat,
          long: lng
        });

        this.messageService.add({
          severity: 'success',
          summary: 'Koordinaten gefunden',
          detail: `Koordinaten automatisch für "${addressString}" ermittelt`
        });
      }
    } catch (error) {
      console.error('Geocoding error:', error);
      // Don't show error message to user for failed geocoding as it's automatic
    } finally {
      this.isGeocodingLoading.set(false);
    }
  }

  // Method to manually clear coordinates (e.g., when user wants to reset them)
  clearCoordinates() {
    const currentAddress = this.address();
    this.address.set({
      ...currentAddress,
      lat: undefined,
      long: undefined
    });
  }

  // Method to manually trigger geocoding
  async geocodeAddress() {
    await this.performGeocoding();
  }

  // Helper methods for template
  getMapLocation(): MapLocation | undefined {
    const addr = this.address();
    if (hasValidCoordinates(addr)) {
      return {
        lat: addr.lat!,
        lng: addr.long!,
        address: formatAddressString(addr),
        title: addr.name || 'Firmenadresse'
      };
    }
    return undefined;
  }

  getFormattedAddress(): string {
    return formatAddressString(this.address());
  }

  hasCoordinates(): boolean {
    return hasValidCoordinates(this.address());
  }

  hasAddressData(): boolean {
    const addr = this.address();
    return !!(addr.strasse || addr.plz || addr.ort || addr.land);
  }

  // Handle map click events
  onMapLocationClicked(mapLocation: MapLocation) {
    // Set coordinates
    const currentAddress = this.address();
    let updatedAddress = {
      ...currentAddress,
      lat: mapLocation.lat,
      long: mapLocation.lng
    };

    // Try to parse and set address components from reverse geocoding
    if (mapLocation.address) {
      const parsedAddress = this.parseAddressFromReverseGeocode(mapLocation.address);
      updatedAddress = {
        ...updatedAddress,
        ...parsedAddress
      };
    }

    this.address.set(updatedAddress);

    // Show success message
    this.messageService.add({
      severity: 'success',
      summary: 'Position ausgewählt',
      detail: mapLocation.address ?
        `Adresse und Koordinaten von Karte übernommen` :
        `Koordinaten von Karte übernommen: ${mapLocation.lat.toFixed(6)}, ${mapLocation.lng.toFixed(6)}`
    });
  }

  // Parse address components from reverse geocoding result
  private parseAddressFromReverseGeocode(fullAddress: string): Partial<Address> {
    // Enhanced parser for better address component extraction
    const result: Partial<Address> = {};

    try {
      // Split by comma and try to extract components
      const parts = fullAddress.split(',').map(part => part.trim());

      console.log('Parsing address parts:', parts); // Debug log

      if (parts.length >= 2) {
        // Find street and house number - look for patterns like "Straßenname 123" or "123 Straßenname"
        let streetWithNumber = '';
        for (let i = 0; i < Math.min(3, parts.length); i++) {
          const part = parts[i];
          // Check if this part contains both letters and numbers (likely street with house number)
          if (/[a-zA-ZäöüÄÖÜß]/.test(part) && /\d/.test(part)) {
            streetWithNumber = part;
            break;
          }
        }

        if (streetWithNumber) {
          result.strasse = streetWithNumber;
        } else {
          // Fallback: if no combined street+number found, take the first part that has letters
          for (const part of parts.slice(0, 3)) {
            if (/[a-zA-ZäöüÄÖÜß]/.test(part) && part.length > 2) {
              result.strasse = part;
              break;
            }
          }
        }

        // Try to find postal code and city
        for (const part of parts) {
          // German postal code pattern (5 digits) - could be standalone or within text
          const zipMatch = part.match(/\b(\d{5})\b/);
          if (zipMatch) {
            result.plz = zipMatch[1];

            // City is usually in the same part or next part, after the postal code
            let cityPart = part.replace(zipMatch[1], '').trim();

            // Remove any leading/trailing punctuation or spaces
            cityPart = cityPart.replace(/^[,\s-]+|[,\s-]+$/g, '');

            if (cityPart && cityPart.length > 1) {
              result.ort = cityPart;
            } else {
              // Look in the next part for the city
              const currentIndex = parts.indexOf(part);
              if (currentIndex + 1 < parts.length) {
                const nextPart = parts[currentIndex + 1].trim();
                if (nextPart && nextPart.length > 1 && !/\d{5}/.test(nextPart)) {
                  result.ort = nextPart;
                }
              }
            }
            break;
          }
        }

        // Try to find country (usually last part, longer than 2 characters)
        const lastPart = parts[parts.length - 1];
        if (lastPart && lastPart.length > 2 && !/\d/.test(lastPart)) {
          result.land = lastPart;
        }

        // Alternative: look for common country names
        for (const part of parts.slice(-2)) { // Check last 2 parts
          const lowerPart = part.toLowerCase();
          if (lowerPart.includes('deutschland') || lowerPart.includes('germany') ||
              lowerPart === 'de' || lowerPart === 'deutschland') {
            result.land = part;
            break;
          }
        }
      }

      console.log('Parsed address result:', result); // Debug log

    } catch (error) {
      console.error('Error parsing address:', error);
    }

    return result;
  }
}
