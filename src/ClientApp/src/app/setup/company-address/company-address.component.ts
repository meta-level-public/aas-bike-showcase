import { CommonModule } from '@angular/common';
import { Component, input, OnChanges, OnInit, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccordionModule } from 'primeng/accordion';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { Address } from '../../model/address';
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
  ],
  providers: [MessageService]
})
export class CompanyAddressComponent implements OnInit, OnChanges {
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
    this.address.set({
      ...currentAddress,
      [field]: value
    });
  }
}
