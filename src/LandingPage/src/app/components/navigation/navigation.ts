import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MenuItem } from 'primeng/api';
import { MenubarModule } from 'primeng/menubar';

@Component({
  selector: 'app-navigation',
  standalone: true,
  templateUrl: './navigation.html',
  styleUrl: './navigation.scss',
  imports: [CommonModule, MenubarModule, RouterLink, TranslateModule],
})
export class Navigation implements OnInit {
  menuItems: MenuItem[] = [];
  currentLang: string = 'de';

  constructor(private translate: TranslateService) {
    this.currentLang = this.translate.currentLang || 'de';

    // Subscribe to language changes to update menu
    this.translate.onLangChange.subscribe(() => {
      this.currentLang = this.translate.currentLang;
      this.updateMenuItems();
    });
  }

  ngOnInit() {
    this.updateMenuItems();
  }

  updateMenuItems() {
    this.translate
      .get([
        'nav.home',
        'nav.dpp',
        'nav.project',
        'nav.projectOverview',
        'nav.projectArchitecture',
        'nav.members',
        'nav.imprint',
      ])
      .subscribe((translations) => {
        this.menuItems = [
          {
            label: translations['nav.home'],
            icon: 'pi pi-home',
            routerLink: '/',
            title: translations['nav.home'],
          },
          {
            label: translations['nav.dpp'],
            icon: 'pi pi-book',
            routerLink: '/dpp',
          },
          {
            label: translations['nav.project'],
            icon: 'pi pi-briefcase',
            items: [
              {
                label: translations['nav.projectOverview'],
                icon: 'pi pi-info-circle',
                routerLink: '/projekt',
              },
              {
                label: translations['nav.projectArchitecture'],
                icon: 'pi pi-sitemap',
                routerLink: '/projekt/architektur',
              },
            ],
          },
          {
            label: translations['nav.members'],
            icon: 'pi pi-users',
            routerLink: '/members',
          },
          {
            label: translations['nav.imprint'],
            icon: 'pi pi-file',
            routerLink: '/impressum',
          },
          {
            icon: 'pi pi-language',
            title: translations['nav.language'],
            items: [
              {
                label: 'Deutsch',
                icon: this.currentLang === 'de' ? 'pi pi-check' : '',
                command: () => this.switchLanguage('de'),
              },
              {
                label: 'English',
                icon: this.currentLang === 'en' ? 'pi pi-check' : '',
                command: () => this.switchLanguage('en'),
              },
            ],
          },
        ];
      });
  }

  switchLanguage(lang: string) {
    this.translate.use(lang);
    localStorage.setItem('language', lang);
    this.currentLang = lang;
  }
}
