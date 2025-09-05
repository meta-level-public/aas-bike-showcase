import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ToastModule } from 'primeng/toast';
import { NavMenuComponent } from './nav-menu/nav-menu.component';
import { NotificationService } from './notification.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  imports: [
    CommonModule,
    FormsModule,
    ToastModule,
    ConfirmDialogModule,
    RouterModule,
    NavMenuComponent,
  ],
})
export class AppComponent {
  constructor(
    private notificationService: NotificationService,
    private messageService: MessageService,
  ) {}

  onConfirm(): void {
    this.messageService.clear('errorDlg');
    this.notificationService.removeMessages();
  }
}
