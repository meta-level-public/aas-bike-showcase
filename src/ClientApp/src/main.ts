import { enableProdMode, ErrorHandler, importProvidersFrom } from '@angular/core';

import { provideHttpClient } from '@angular/common/http';
import { bootstrapApplication, BrowserModule } from '@angular/platform-browser';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { ConfirmationService, MessageService } from 'primeng/api';
import { providePrimeNG } from 'primeng/config';
import { AppComponent } from './app/app.component';
import { routes } from './app/app.routes';
import { ErrorHandlerService } from './app/error-handler.service';
import Theme from './app/theme'; // Import the Lara theme
import { environment } from './environments/environment';

export function getBaseUrl() {
  return document.getElementsByTagName('base')[0].href;
}

if (environment.production) {
  enableProdMode();
}

bootstrapApplication(AppComponent, {
  providers: [
    { provide: 'BASE_URL', useFactory: getBaseUrl, deps: [] },
    importProvidersFrom(BrowserModule),
    MessageService,
    ConfirmationService,
    { provide: ErrorHandler, useClass: ErrorHandlerService },
    provideAnimations(),
    provideHttpClient(),
    provideRouter(routes),
    providePrimeNG({
      theme: {
        preset: Theme,
        options: {
          darkModeSelector: '.my-app-dark',
        },
      },
    }),
  ],
}).catch((err) => console.log(err));
