import { Component } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';

@Component({
  selector: 'app-dpp',
  standalone: true,
  templateUrl: './dpp.html',
  styleUrl: './dpp.scss',
  imports: [CardModule, DividerModule, TranslateModule],
})
export class Dpp {}
