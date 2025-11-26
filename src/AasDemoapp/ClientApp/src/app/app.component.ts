import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MessageService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { NotificationService } from './notification.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  imports: [
    CommonModule,
    FormsModule,
    ToastModule,
    ConfirmDialogModule,
    RouterModule,
    NavMenuComponent,
    TranslateModule,
  ],
})
export class AppComponent implements OnInit {
  constructor(
    private notificationService: NotificationService,
    private messageService: MessageService,
    private translate: TranslateService
  ) {
    // Verfügbare Sprachen festlegen
    this.translate.addLangs(['de', 'en']);

    // Standardsprache festlegen
    this.translate.setDefaultLang('de');

    // Browser-Sprache verwenden oder auf Deutsch fallback
    const browserLang = this.translate.getBrowserLang();
    const savedLang = localStorage.getItem('language');

    if (savedLang) {
      this.translate.use(savedLang);
    } else if (browserLang && ['de', 'en'].includes(browserLang)) {
      this.translate.use(browserLang);
    } else {
      this.translate.use('de');
    }
  }

  ngOnInit() {}

  onConfirm(): void {
    this.messageService.clear('errorDlg');
    this.notificationService.removeMessages();
  }
}
