import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NavigationEnd, Router } from '@angular/router';
import { MenuItem } from 'primeng/api';
import { MenubarModule } from 'primeng/menubar';
import { Subscription, filter } from 'rxjs';

@Component({
  selector: 'app-nav-menu',
  templateUrl: './nav-menu.component.html',
  styleUrls: ['./nav-menu.component.scss'],
  imports: [CommonModule, FormsModule, MenubarModule],
})
export class NavMenuComponent implements OnInit, OnDestroy {
  items: MenuItem[] = [];
  subscriptions: Subscription[] = [];

  constructor(private router: Router) {}

  ngOnDestroy(): void {
    this.subscriptions.forEach((subscription) => subscription.unsubscribe());
  }

  ngOnInit(): void {
    this.subscriptions.push(
      this.router.events.pipe(filter((event) => event instanceof NavigationEnd)).subscribe(() => {
        this.items.forEach((item) => {
          item.styleClass = this.router.url === item.routerLink ? 'activeItem' : '';
        });
        if (this.router.url === '/') {
          this.items[0].styleClass = 'activeItem';
        }
      })
    );

    this.items = [
      {
        label: 'Dashboard',
        icon: 'pi pi-fw pi-th-large',
        routerLink: '/',
      },
      // {
      //   label: 'Browse',
      //   icon: 'pi pi-fw pi-th-large',
      //   items: [
      //     {
      //       label: 'AAS Discovery',
      //       icon: 'pi pi-fw pi-file',
      //       routerLink: '/browse/discovery',
      //     },
      //     {
      //       label: 'AAS Registry',
      //       icon: 'pi pi-fw pi-file',
      //       routerLink: '/browse/registry',
      //     },
      //     {
      //       label: 'AAS Repository',
      //       icon: 'pi pi-fw pi-file',
      //       routerLink: '/browse/repository',
      //     },
      //   ],
      // },
      {
        label: 'Katalog',
        icon: 'pi pi-fw pi-book',
        items: [
          {
            label: 'Auflisten',
            icon: 'pi pi-fw pi-list',
            routerLink: '/catalog/list',
          },
          {
            label: 'Befüllen',
            icon: 'pi pi-fw pi-plus',
            routerLink: '/catalog/import',
          },
        ],
      },
      {
        label: 'Konfiguration',
        icon: 'pi pi-fw pi-ticket',
        items: [
          {
            label: 'Auflisten',
            icon: 'pi pi-fw pi-list',
            routerLink: '/config/list',
          },
          {
            label: 'Erstellen',
            icon: 'pi pi-fw pi-plus',
            routerLink: '/config/create',
          },
        ],
      },
      {
        label: 'Waren',
        icon: 'pi pi-fw pi-box',
        items: [
          {
            label: 'Bestand Auflisten',
            icon: 'pi pi-fw pi-list',
            routerLink: '/goods/list',
          },
          {
            label: 'Eingang Buchen',
            icon: 'pi pi-fw pi-check-square',
            routerLink: '/goods/incoming',
          },
        ],
      },
      {
        label: 'Produktionsaufträge',
        icon: 'pi pi-fw pi-box',
        items: [
          {
            label: 'Aufträge Auflisten',
            icon: 'pi pi-fw pi-receipt', // TODO: Auftragsicon finden
            routerLink: '/production-orders/list',
          },
        ],
      },
      {
        label: 'Produktion',
        icon: 'pi pi-fw pi-step-forward',
        items: [
          {
            label: 'Montage',
            icon: 'fas fa-solid fa-industry',
            routerLink: '/production/assembly',
          },
        ],
      },
      {
        label: 'Dokumentation',
        icon: 'pi pi-fw pi-book',
        items: [
          {
            label: 'DPP',
            icon: 'pi pi-fw pi-file-pdf',
            routerLink: '/documentation/dpp',
          },
        ],
      },
      {
        label: 'Setup',
        icon: 'pi pi-fw pi-cog',
        items: [
          {
            label: 'Darkmode umschalten',
            command: () => {
              const element = document.querySelector('html');
              console.log('Darkmode umschalten');
              console.log(element);
              element?.classList.toggle('my-app-dark');
              console.log('Darkmode umgeschaltet');
            },
          },
          {
            label: 'Grundsetup',
            routerLink: '/setup/setup',
          },
        ],
      },
      {
        label: 'Impressum',
        icon: 'pi pi-fw pi-file',
        routerLink: '/impressum',
      },
    ];
  }
}
