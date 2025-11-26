import { Component } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';

@Component({
  selector: 'app-projekt-architektur',
  standalone: true,
  templateUrl: './projekt-architektur.html',
  styleUrl: './projekt-architektur.scss',
  imports: [CardModule, DividerModule, TranslateModule],
})
export class ProjektArchitektur {}
