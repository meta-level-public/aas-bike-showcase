import { Component } from '@angular/core';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';

@Component({
  selector: 'app-dpp',
  standalone: true,
  templateUrl: './dpp.html',
  styleUrl: './dpp.scss',
  imports: [CardModule, DividerModule],
})
export class Dpp {}
