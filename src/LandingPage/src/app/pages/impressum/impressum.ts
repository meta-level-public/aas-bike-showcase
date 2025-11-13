import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { CardModule } from 'primeng/card';
import { ImpressumConfig } from './impressum.model';
import { ImpressumService } from './impressum.service';

@Component({
  selector: 'app-impressum',
  standalone: true,
  templateUrl: './impressum.html',
  styleUrl: './impressum.scss',
  imports: [CommonModule, CardModule],
})
export class Impressum implements OnInit {
  config: ImpressumConfig | null = null;
  loading = true;
  error = false;

  constructor(private impressumService: ImpressumService) {}

  ngOnInit(): void {
    this.impressumService.getConfig().subscribe({
      next: (data) => {
        this.config = data;
        this.loading = false;
      },
      error: (err) => {
        console.error('Fehler beim Laden der Impressumsdaten:', err);
        this.error = true;
        this.loading = false;
      },
    });
  }
}
