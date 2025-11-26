import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NavigationEnd, Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MenuItem } from 'primeng/api';
import { MenubarModule } from 'primeng/menubar';
import { Subscription, filter } from 'rxjs';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss'],
  imports: [CommonModule, FormsModule, MenubarModule, TranslateModule],
})
export class NavMenuComponent implements OnInit, OnDestroy {
  items: MenuItem[] = [];
  subscriptions: Subscription[] = [];
  currentLang: string = 'de';

  constructor(
    private router: Router,
    private translate: TranslateService
  ) {
    this.currentLang = this.translate.currentLang || 'de';

    // Subscribe to language changes to update menu
    this.translate.onLangChange.subscribe(() => {
      this.currentLang = this.translate.currentLang;
      this.updateMenuItems();
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach((subscription) => subscription.unsubscribe());
  }

  ngOnInit(): void {
    this.updateMenuItems();

    this.subscriptions.push(
      this.router.events.pipe(filter((event) => event instanceof NavigationEnd)).subscribe(() => {
        this.updateActiveMenuItems();
      })
    );
  }

  updateActiveMenuItems(): void {
    this.items.forEach((item) => {
      if (item.routerLink) {
        item.styleClass = this.router.url === item.routerLink ? 'activeItem' : '';
      }
    });
    if (this.router.url === '/' && this.items[0]) {
      this.items[0].styleClass = 'activeItem';
    }
  }

  updateMenuItems(): void {
    this.translate
      .get([
        'nav.dashboard',
        'nav.catalog',
        'nav.catalogList',
        'nav.catalogImport',
        'nav.configuration',
        'nav.configList',
        'nav.configCreate',
        'nav.goods',
        'nav.goodsList',
        'nav.goodsIncoming',
        'nav.productionOrders',
        'nav.ordersList',
        'nav.production',
        'nav.assembly',
        'nav.documentation',
        'nav.dpp',
        'nav.setup',
        'nav.darkmode',
        'nav.basicSetup',
        'nav.language',
      ])
      .subscribe((translations) => {
        this.items = [
          {
            icon: 'pi pi-fw pi-home',
            routerLink: '/',
            title: translations['nav.dashboard'],
          },
          {
            label: translations['nav.catalog'],
            icon: 'pi pi-fw pi-book',
            items: [
              {
                label: translations['nav.catalogList'],
                icon: 'pi pi-fw pi-list',
                routerLink: '/catalog/list',
              },
              {
                label: translations['nav.catalogImport'],
                icon: 'pi pi-fw pi-plus',
                routerLink: '/catalog/import',
              },
            ],
          },
          {
            label: translations['nav.configuration'],
            icon: 'pi pi-fw pi-ticket',
            items: [
              {
                label: translations['nav.configList'],
                icon: 'pi pi-fw pi-list',
                routerLink: '/config/list',
              },
              {
                label: translations['nav.configCreate'],
                icon: 'pi pi-fw pi-plus',
                routerLink: '/config/create',
              },
            ],
          },
          {
            label: translations['nav.goods'],
            icon: 'pi pi-fw pi-box',
            items: [
              {
                label: translations['nav.goodsList'],
                icon: 'pi pi-fw pi-list',
                routerLink: '/goods/list',
              },
              {
                label: translations['nav.goodsIncoming'],
                icon: 'pi pi-fw pi-check-square',
                routerLink: '/goods/incoming',
              },
            ],
          },
          {
            label: translations['nav.productionOrders'],
            icon: 'pi pi-fw pi-box',
            items: [
              {
                label: translations['nav.ordersList'],
                icon: 'pi pi-fw pi-receipt',
                routerLink: '/production-orders/list',
              },
            ],
          },
          {
            label: translations['nav.production'],
            icon: 'pi pi-fw pi-step-forward',
            items: [
              {
                label: translations['nav.assembly'],
                icon: 'fas fa-solid fa-industry',
                routerLink: '/production/assembly',
              },
            ],
          },
          {
            label: translations['nav.documentation'],
            icon: 'pi pi-fw pi-book',
            items: [
              {
                label: translations['nav.dpp'],
                icon: 'pi pi-fw pi-file-pdf',
                routerLink: '/documentation/dpp',
              },
            ],
          },
          {
            icon: 'pi pi-fw pi-cog',
            title: translations['nav.setup'],
            items: [
              {
                label: translations['nav.darkmode'],
                command: () => {
                  const element = document.querySelector('html');
                  element?.classList.toggle('my-app-dark');
                },
              },
              {
                label: translations['nav.basicSetup'],
                routerLink: '/setup/setup',
              },
              {
                separator: true,
              },
              {
                label: translations['nav.language'],
                icon: 'pi pi-fw pi-language',
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
            ],
          },
        ];

        // Update active menu items after items are set
        this.updateActiveMenuItems();
      });
  }

  switchLanguage(lang: string) {
    this.translate.use(lang);
    localStorage.setItem('language', lang);
    this.currentLang = lang;
  }
}
