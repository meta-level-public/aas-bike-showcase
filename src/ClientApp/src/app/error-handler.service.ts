import { HttpErrorResponse } from '@angular/common/http';
import { ErrorHandler, Injectable, isDevMode, NgZone } from '@angular/core';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class ErrorHandlerService implements ErrorHandler {
  constructor(
    private notificationService: NotificationService,
    private zone: NgZone
  ) {}
  handleError(error: any): void {
    this.zone.run(() => {
      if (error === true) {
        // ignorieren - was auch immer das ist
      } else if (error.rejection instanceof HttpErrorResponse || error instanceof HttpErrorResponse) {
        let msg = '';
        if (isDevMode()) msg = this.messageFormatter(`${error.message ?? error} - ${error.stack ?? ''}`);
        else msg = this.messageFormatter(`${error.message} ?? ''`);
        this.notificationService.showMessage(msg, 'UNHANDLED_EXCEPTION', 'error', true);
        // eslint-disable-next-line no-console
        if (isDevMode()) console.error(error);
      } else if (error.error instanceof ErrorEvent || error instanceof TypeError) {
        // client-side error
        const errorMessage = `Error: ${error.message}`;
        this.notificationService.showMessage(errorMessage, 'CLIENTSIDE_ERROR_HEADLINE', 'error', true);
        // eslint-disable-next-line no-console
        if (isDevMode()) console.error(error);
      } else {
        // eslint-disable-next-line no-console
        if (isDevMode()) console.error(error);
      }
    });
  }

  messageFormatter(message: string): string {
    return message.replace(/&quot;/gm, '"');
  }
}
