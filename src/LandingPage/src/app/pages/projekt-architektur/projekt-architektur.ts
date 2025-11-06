import { Component } from '@angular/core';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';

@Component({
  selector: 'app-projekt-architektur',
  standalone: true,
  templateUrl: './projekt-architektur.html',
  styleUrl: './projekt-architektur.scss',
  imports: [CardModule, DividerModule],
})
export class ProjektArchitektur {}
