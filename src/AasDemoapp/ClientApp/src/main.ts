import { enableProdMode, ErrorHandler, importProvidersFrom } from '@angular/core';

import { HttpClient, provideHttpClient } from '@angular/common/http';
import { bootstrapApplication, BrowserModule } from '@angular/platform-browser';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter } from '@angular/router';
import { TranslateLoader, TranslateModule } from '@ngx-translate/core';
import { ConfirmationService, MessageService } from 'primeng/api';
import { providePrimeNG } from 'primeng/config';
import { Observable } from 'rxjs';
import { AppComponent } from './app/app.component';
import { routes } from './app/app.routes';
import { ErrorHandlerService } from './app/error-handler.service';
import Theme from './app/theme'; // Import the Lara theme
import { environment } from './environments/environment';

// Custom TranslateLoader
export class CustomTranslateLoader implements TranslateLoader {
  constructor(private http: HttpClient) {}

  getTranslation(lang: string): Observable<any> {
    return this.http.get(`./assets/i18n/${lang}.json`);
  }
}

// Factory-Funktion für den CustomTranslateLoader
export function HttpLoaderFactory(http: HttpClient): CustomTranslateLoader {
  return new CustomTranslateLoader(http);
}

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
    importProvidersFrom(
      TranslateModule.forRoot({
        defaultLanguage: 'de',
        loader: {
          provide: TranslateLoader,
          useFactory: HttpLoaderFactory,
          deps: [HttpClient],
        },
      })
    ),
  ],
}).catch((err) => console.log(err));
