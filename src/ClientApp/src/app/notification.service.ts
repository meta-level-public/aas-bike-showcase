import { Injectable } from '@angular/core';
import { Message, MessageService } from 'primeng/api';

@Injectable({
  providedIn: 'root',
})
export class NotificationService {
  messages: ErrorMessage[] = [];

  constructor(private messageService: MessageService) {}

  showMessageAlways(
    message: string | string[],
    title: string = '',
    severity: 'success' | 'info' | 'warn' | 'error' = 'info',
    permanent: boolean = true,
    life: number = 5000,
    additionalInfo: string | null = null
  ) {
    this.showMessage(message, title, severity, permanent, life, additionalInfo, true);
  }

  showMessage(
    message: string | string[],
    title: string = '',
    severity: 'success' | 'info' | 'warn' | 'error' = 'info',
    permanent: boolean = true,
    life: number = 5000,
    additionalInfo: string | null = null,
    force: boolean = false
  ): void {
    let resultMessage = '';
    if (additionalInfo != null && additionalInfo !== '' && additionalInfo !== 'None') {
      resultMessage += '\nInfo: ' + additionalInfo + '\n';
    }
    if (Array.isArray(message)) {
      resultMessage += message.map((m) => m).join('\n');
    } else {
      resultMessage += message;
    }
    if (title !== '') title = title;
    if (this.addMessage(resultMessage, title, force)) {
      if (permanent) {
        this.messageService.add({
          key: 'errorDlg',
          sticky: true,
          severity: severity,
          summary: title,
          detail: resultMessage,
        });
      } else
        this.messageService.add({
          severity: severity,
          summary: title,
          detail: resultMessage,
          life,
        });
    }
  }

  removeMessage(message: string, title: string) {
    this.messages = this.messages.filter((m) => m.message !== message && m.title === title);
  }
  removeMessages() {
    this.messages = [];
  }

  private addMessage(message: string, title: string, force = false) {
    const errorMessage: ErrorMessage = { message, title };
    if (!this.messages.some((m) => m.message === message && m.title === title) || force) {
      this.messages.push(errorMessage);
      return true;
    }
    return false;
  }
}

export interface ErrorMessage {
  message: string;
  title: string;
}
