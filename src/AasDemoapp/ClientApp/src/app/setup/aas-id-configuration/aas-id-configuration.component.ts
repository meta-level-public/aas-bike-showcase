import { CommonModule } from '@angular/common';
import { Component, input, OnChanges, OnInit, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { AccordionModule } from 'primeng/accordion';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ToastModule } from 'primeng/toast';
import { Setting } from '../setting';
import { SetupService } from '../setup.service';

@Component({
  selector: 'app-aas-id-configuration',
  templateUrl: './aas-id-configuration.component.html',
  styleUrl: './aas-id-configuration.component.css',
  imports: [
    CommonModule,
    FormsModule,
    AccordionModule,
    ButtonModule,
    InputTextModule,
    ToastModule,
    TranslateModule,
  ],
  providers: [MessageService],
})
export class AasIdConfigurationComponent implements OnInit, OnChanges {
  // Input signals
  settings = input<Setting[]>([]);

  // Output events
  settingsUpdated = output<void>();

  // ID Prefix signal
  aasIdPrefix = signal<string>('https://oi4-nextbike.de');

  // Loading state
  isSaving = signal(false);

  constructor(
    private setupService: SetupService,
    private messageService: MessageService
  ) {}

  ngOnInit() {
    this.loadAasIdPrefix();
  }

  ngOnChanges() {
    this.loadAasIdPrefix();
  }

  loadAasIdPrefix() {
    const currentSettings = this.settings();
    const prefixSetting = currentSettings.find((s) => s.name === 'AasIdPrefix');

    if (prefixSetting?.value) {
      this.aasIdPrefix.set(prefixSetting.value);
    } else {
      this.aasIdPrefix.set('https://oi4-nextbike.de');
    }
  }

  updatePrefix(value: string) {
    this.aasIdPrefix.set(value);
  }

  async saveAasIdPrefix() {
    const prefix = this.aasIdPrefix();

    // Validate URL format
    if (!prefix || prefix.trim() === '') {
      this.messageService.add({
        severity: 'error',
        summary: 'Fehler',
        detail: 'Der ID-Präfix darf nicht leer sein.',
      });
      return;
    }

    // Simple URL validation
    try {
      new URL(prefix);
    } catch {
      this.messageService.add({
        severity: 'warn',
        summary: 'Warnung',
        detail: 'Der eingegebene Präfix ist keine gültige URL. Bitte überprüfen Sie die Eingabe.',
      });
      // Allow to continue anyway
    }

    this.isSaving.set(true);

    try {
      const setting: Setting = {
        name: 'AasIdPrefix',
        value: prefix.trim(),
      };

      await this.setupService.saveSetting(setting);

      this.messageService.add({
        severity: 'success',
        summary: 'Erfolg',
        detail: 'AAS ID-Präfix wurde erfolgreich gespeichert.',
      });

      this.settingsUpdated.emit();
    } catch (error) {
      console.error('Error saving AAS ID prefix:', error);
      this.messageService.add({
        severity: 'error',
        summary: 'Fehler',
        detail: 'AAS ID-Präfix konnte nicht gespeichert werden.',
      });
    } finally {
      this.isSaving.set(false);
    }
  }

  resetToDefault() {
    this.aasIdPrefix.set('https://oi4-nextbike.de');
  }
}
