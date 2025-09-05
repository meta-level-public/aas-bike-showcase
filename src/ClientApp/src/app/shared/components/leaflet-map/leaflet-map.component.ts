import { CommonModule } from '@angular/common';
import {
  AfterViewInit,
  Component,
  ElementRef,
  OnDestroy,
  ViewChild,
  effect,
  input,
  output,
  signal,
} from '@angular/core';
import * as L from 'leaflet';

export interface MapLocation {
  lat: number;
  lng: number;
  address?: string;
  title?: string;
}

@Component({
  selector: 'app-leaflet-map',
  templateUrl: './leaflet-map.component.html',
  styleUrl: './leaflet-map.component.css',
  imports: [CommonModule],
})
export class LeafletMapComponent implements AfterViewInit, OnDestroy {
  @ViewChild('mapContainer', { static: true })
  mapContainer!: ElementRef<HTMLDivElement>;

  // Input signals
  location = input<MapLocation>();
  address = input<string>();
  height = input<string>('300px');
  width = input<string>('100%');
  zoom = input<number>(13);
  enableClickToSet = input<boolean>(false);

  // Output events
  locationClicked = output<MapLocation>();

  // Private properties
  private map: L.Map | null = null;
  private marker: L.Marker | null = null;
  private clickHandler: ((e: L.LeafletMouseEvent) => void) | null = null;
  private lastLocation: { lat: number; lng: number } | null = null;
  private lastAddress: string | null = null;

  // Public properties
  isLoading = signal(false);

  constructor() {
    // React to location or address changes
    effect(() => {
      const loc = this.location();
      const addr = this.address();

      // Check if location has actually changed
      if (
        loc &&
        loc.lat !== null &&
        loc.lat !== undefined &&
        loc.lng !== null &&
        loc.lng !== undefined &&
        !isNaN(loc.lat) &&
        !isNaN(loc.lng)
      ) {
        // Only update if coordinates have actually changed
        if (
          !this.lastLocation ||
          this.lastLocation.lat !== loc.lat ||
          this.lastLocation.lng !== loc.lng
        ) {
          this.lastLocation = { lat: loc.lat, lng: loc.lng };
          this.lastAddress = null; // Clear last address since we have coordinates
          this.updateMapWithCoordinates(loc);
        }
      } else if (addr && addr.trim() && addr !== this.lastAddress) {
        this.lastAddress = addr;
        this.lastLocation = null; // Clear last location since we have new address
        this.geocodeAndUpdateMap(addr);
      }
    });

    // React to enableClickToSet changes
    effect(() => {
      const clickEnabled = this.enableClickToSet();
      this.updateClickHandler(clickEnabled);
    });
  }

  ngAfterViewInit() {
    // Wait for the view to be fully initialized
    setTimeout(() => {
      this.initMap();
    }, 0);
  }

  ngOnDestroy() {
    if (this.map && this.clickHandler) {
      this.map.off('click', this.clickHandler);
      this.clickHandler = null;
    }

    if (this.map) {
      this.map.remove();
      this.map = null;
    }

    // Clear cached values
    this.lastLocation = null;
    this.lastAddress = null;
  }

  private initMap() {
    // Ensure the container has proper dimensions
    this.mapContainer.nativeElement.style.height = this.height();
    this.mapContainer.nativeElement.style.width = this.width();

    // Initialize map with default view
    const defaultLocation: MapLocation = {
      lat: 52.520008,
      lng: 13.404954,
    };

    this.map = L.map(this.mapContainer.nativeElement, {
      preferCanvas: true,
      zoomControl: true,
    }).setView([defaultLocation.lat, defaultLocation.lng], this.zoom());

    // Add tile layer with better options
    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      maxZoom: 19,
      attribution: '© OpenStreetMap contributors',
      crossOrigin: true,
    }).addTo(this.map);

    // Force map to invalidate size multiple times to ensure proper rendering
    setTimeout(() => {
      this.map?.invalidateSize(true);
    }, 100);

    setTimeout(() => {
      this.map?.invalidateSize(true);
    }, 300);

    setTimeout(() => {
      this.map?.invalidateSize(true);
    }, 500);

    // Add event listeners for zoom and resize events to fix coordinate issues
    this.map.on('zoomend', () => {
      setTimeout(() => {
        this.map?.invalidateSize(true);
      }, 100);
    });

    this.map.on('resize', () => {
      setTimeout(() => {
        this.map?.invalidateSize(true);
      }, 50);
    });

    // Add event listener for when the map finishes loading tiles
    this.map.on('load', () => {
      setTimeout(() => {
        this.map?.invalidateSize(true);
      }, 100);
    });

    // Check for initial location or address
    const initialLocation = this.location();
    const initialAddress = this.address();

    if (initialLocation) {
      this.updateMapWithCoordinates(initialLocation);
    } else if (initialAddress) {
      this.geocodeAndUpdateMap(initialAddress);
    }

    // Set up click handler after map initialization
    this.updateClickHandler(this.enableClickToSet());
  }

  // Update click handler based on enableClickToSet
  private updateClickHandler(enabled: boolean) {
    if (!this.map) {
      return;
    }

    console.log('Updating click handler, enabled:', enabled); // Debug-Log
    console.log('Map container element:', this.mapContainer?.nativeElement); // Debug-Log

    // Remove existing click handler
    if (this.clickHandler) {
      this.map.off('click', this.clickHandler);
      this.clickHandler = null;
      console.log('Removed existing click handler'); // Debug-Log
    }

    // Add new click handler if enabled
    if (enabled) {
      // Test: Simple click handler first
      console.log('Adding simple test click handler'); // Debug-Log

      this.clickHandler = (e: L.LeafletMouseEvent) => {
        // Add small delay to ensure map is fully rendered after zoom
        setTimeout(() => {
          this.handleMapClick(e.latlng.lat, e.latlng.lng);
        }, 50);
      };

      // Add the handler to the map
      this.map.on('click', this.clickHandler);
      console.log('Click handler registered on map'); // Debug-Log

      // Change cursor to indicate clickable map - try multiple approaches
      if (this.mapContainer?.nativeElement) {
        this.mapContainer.nativeElement.style.cursor = 'crosshair !important';
        this.mapContainer.nativeElement.style.setProperty(
          'cursor',
          'crosshair',
          'important',
        );

        // Also try to set cursor on the map div itself
        const mapDiv =
          this.mapContainer.nativeElement.querySelector('.leaflet-container');
        if (mapDiv) {
          (mapDiv as HTMLElement).style.cursor = 'crosshair !important';
          (mapDiv as HTMLElement).style.setProperty(
            'cursor',
            'crosshair',
            'important',
          );
        }
      }
    } else {
      // Reset cursor
      if (this.mapContainer?.nativeElement) {
        this.mapContainer.nativeElement.style.cursor = '';
        this.mapContainer.nativeElement.style.removeProperty('cursor');

        const mapDiv =
          this.mapContainer.nativeElement.querySelector('.leaflet-container');
        if (mapDiv) {
          (mapDiv as HTMLElement).style.cursor = '';
          (mapDiv as HTMLElement).style.removeProperty('cursor');
        }
      }
    }
  }

  private updateMapWithCoordinates(location: MapLocation) {
    if (!this.map) {
      return;
    }

    if (!location) {
      return;
    }

    if (
      location.lat === null ||
      location.lat === undefined ||
      location.lng === null ||
      location.lng === undefined ||
      isNaN(location.lat) ||
      isNaN(location.lng)
    ) {
      return;
    }

    // Force invalidate size before updating coordinates
    this.map.invalidateSize(true);

    // Remove existing marker
    if (this.marker) {
      this.map.removeLayer(this.marker);
    }

    // Create custom marker icon (fix for default marker icon issue)
    const customIcon = L.icon({
      iconUrl:
        'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon.png',
      iconRetinaUrl:
        'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-icon-2x.png',
      shadowUrl:
        'https://cdnjs.cloudflare.com/ajax/libs/leaflet/1.7.1/images/marker-shadow.png',
      iconSize: [25, 41],
      iconAnchor: [12, 41],
      popupAnchor: [1, -34],
      shadowSize: [41, 41],
    });

    // Add new marker
    this.marker = L.marker([location.lat, location.lng], {
      icon: customIcon,
    }).addTo(this.map);

    // Add popup if address or title is provided
    const popupContent =
      location.title || location.address || `${location.lat}, ${location.lng}`;
    this.marker.bindPopup(popupContent);

    // Center map on location
    this.map.setView([location.lat, location.lng], this.zoom());

    // Ensure map is properly rendered after coordinate update
    setTimeout(() => {
      this.map?.invalidateSize(true);
    }, 100);
  }

  private async geocodeAndUpdateMap(address: string) {
    if (!address.trim()) return;

    this.isLoading.set(true);

    try {
      // Use Nominatim for geocoding (OpenStreetMap's geocoding service)
      const encodedAddress = encodeURIComponent(address);
      const response = await fetch(
        `https://nominatim.openstreetmap.org/search?format=json&q=${encodedAddress}&limit=1`,
      );

      if (!response.ok) {
        throw new Error('Geocoding request failed');
      }

      const results = await response.json();

      if (results && results.length > 0) {
        const result = results[0];
        const location: MapLocation = {
          lat: parseFloat(result.lat),
          lng: parseFloat(result.lon),
          address: address,
          title: result.display_name,
        };

        this.updateMapWithCoordinates(location);
      } else {
        console.warn('No geocoding results found for address:', address);
      }
    } catch (error) {
      console.error('Geocoding error:', error);
    } finally {
      this.isLoading.set(false);
    }
  }

  // Public method to manually center map on coordinates
  centerOnLocation(lat: number, lng: number, zoom?: number) {
    if (!this.map) return;

    const zoomLevel = zoom || this.zoom();
    this.map.setView([lat, lng], zoomLevel);
  }

  // Public method to fit map to show all markers (useful for multiple markers in future)
  fitToMarkers() {
    if (!this.map || !this.marker) return;

    const group = new L.FeatureGroup([this.marker]);
    this.map.fitBounds(group.getBounds().pad(0.1));
  }

  // Public method to refresh map size (useful when container size changes)
  refreshMapSize() {
    if (!this.map) return;

    setTimeout(() => {
      this.map?.invalidateSize(true);
    }, 100);
  }

  // Handle map click events
  private async handleMapClick(lat: number, lng: number) {
    if (!this.enableClickToSet()) {
      return;
    }

    // Ensure coordinates are valid
    if (!lat || !lng || isNaN(lat) || isNaN(lng)) {
      return;
    }

    // Invalidate map size to ensure correct coordinate calculation
    this.map?.invalidateSize(true);

    // Update marker position immediately
    const tempLocation: MapLocation = {
      lat: Number(lat),
      lng: Number(lng),
      title: 'Ausgewählte Position',
    };

    this.updateMapWithCoordinates(tempLocation);

    // Start reverse geocoding
    this.isLoading.set(true);

    try {
      const address = await this.reverseGeocode(lat, lng);
      const locationWithAddress: MapLocation = {
        lat: Number(lat),
        lng: Number(lng),
        address,
        title: address || 'Ausgewählte Position',
      };

      // Update marker with address info
      this.updateMapWithCoordinates(locationWithAddress);

      // Emit the event
      this.locationClicked.emit(locationWithAddress);
    } catch (error) {
      console.error('Reverse geocoding error:', error);
      // Still emit the location even if reverse geocoding fails
      this.locationClicked.emit(tempLocation);
    } finally {
      this.isLoading.set(false);
    }
  }

  // Reverse geocoding function
  private async reverseGeocode(
    lat: number,
    lng: number,
  ): Promise<string | undefined> {
    try {
      const response = await fetch(
        `https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lng}&zoom=18&addressdetails=1`,
      );

      if (!response.ok) {
        throw new Error('Reverse geocoding request failed');
      }

      const result = await response.json();

      if (result && result.display_name) {
        return result.display_name;
      }
    } catch (error) {
      console.error('Reverse geocoding failed:', error);
    }

    return undefined;
  }
}
