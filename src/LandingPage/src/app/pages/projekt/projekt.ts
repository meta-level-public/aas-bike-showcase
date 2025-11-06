import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';

@Component({
  selector: 'app-projekt',
  standalone: true,
  templateUrl: './projekt.html',
  styleUrl: './projekt.scss',
  imports: [RouterLink, CardModule, ButtonModule, DividerModule],
})
export class Projekt {}
