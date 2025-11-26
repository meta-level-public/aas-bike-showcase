import { Component } from '@angular/core';
import { TranslateModule } from '@ngx-translate/core';
import { CardModule } from 'primeng/card';
import { DividerModule } from 'primeng/divider';

@Component({
  selector: 'app-members',
  standalone: true,
  templateUrl: './members.html',
  styleUrl: './members.scss',
  imports: [CardModule, DividerModule, TranslateModule],
})
export class Members {}
