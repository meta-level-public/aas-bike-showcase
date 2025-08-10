import { CommonModule } from '@angular/common';
import { Component, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AccordionModule } from 'primeng/accordion';
import { ButtonModule } from 'primeng/button';
import { SelectButtonModule } from 'primeng/selectbutton';
import { Setting } from '../setting';
import { SetupService } from '../setup.service';

@Component({
  selector: 'app-erp-configuration',
  templateUrl: './erp-configuration.component.html',
  styleUrl: './erp-configuration.component.css',
  imports: [
    CommonModule,
    FormsModule,
    AccordionModule,
    SelectButtonModule,
    ButtonModule,
  ],
})
export class ErpConfigurationComponent {
  // Input signals
  selectedErp = input<{ label: string; logo: string } | undefined>();

  // Output events
  erpSelected = output<{ label: string; logo: string }>();
  settingsSaved = output<void>();

  // Local state
  erpOptions = signal<{ label: string; logo: string }[]>([
    { label: 'Meta SimplestErp', logo: 'ml.jpeg' },
    { label: 'SAP', logo: 'sap.png' },
    { label: 'ERP X', logo: 'erp.png' },
  ]);

  constructor(private setupService: SetupService) {}

  onErpSelectionChange(erp: { label: string; logo: string }) {
    this.erpSelected.emit(erp);
  }

  async saveSettings() {
    const selectedErp = this.selectedErp();
    if (!selectedErp) return;

    const setting: Setting = {
      name: 'ERP',
      value: selectedErp.label,
    };

    await this.setupService.saveSetting(setting);
    this.settingsSaved.emit();
  }
}
