import { Component } from '@angular/core';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';

@Component({
  selector: 'app-members',
  standalone: true,
  templateUrl: './members.html',
  styleUrl: './members.scss',
  imports: [CardModule, DividerModule],
})
export class Members {}
