import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { CardModule } from 'primeng/card';
import { ImpressumData, ImpressumService } from './impressum.service';

@Component({
  selector: 'app-impressum',
  templateUrl: './impressum.component.html',
  styleUrl: './impressum.component.scss',
  imports: [CommonModule, CardModule],
})
export class ImpressumComponent implements OnInit {
  impressumData: ImpressumData | null = null;
  loading = true;

  constructor(private impressumService: ImpressumService) {}

  async ngOnInit() {
    try {
      this.impressumData = await this.impressumService.getImpressum();
    } catch (error) {
      console.error('Fehler beim Laden der Impressumsdaten:', error);
    } finally {
      this.loading = false;
    }
  }
}
