import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';

@Component({
  selector: 'app-projekt',
  standalone: true,
  templateUrl: './projekt.html',
  styleUrl: './projekt.scss',
  imports: [RouterLink, CardModule, ButtonModule, DividerModule, TranslateModule],
})
export class Projekt {}
