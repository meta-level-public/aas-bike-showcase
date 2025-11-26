import { Component, OnInit } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { Navigation } from './components/navigation/navigation';

@Component({
  selector: 'app-root',
  templateUrl: './app.html',
  standalone: true,
  styleUrl: './app.scss',
  imports: [RouterOutlet, RouterLink, Navigation, TranslateModule],
})
export class App implements OnInit {
  constructor(private translate: TranslateService) {
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
}
