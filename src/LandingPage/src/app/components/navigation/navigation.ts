import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MenuItem } from 'primeng/api';
import { MenubarModule } from 'primeng/menubar';

@Component({
  selector: 'app-navigation',
  standalone: true,
  templateUrl: './navigation.html',
  styleUrl: './navigation.scss',
  imports: [CommonModule, MenubarModule],
})
export class Navigation implements OnInit {
  menuItems: MenuItem[] = [];

  ngOnInit() {
    this.menuItems = [
      {
        label: 'Home',
        icon: 'pi pi-home',
        routerLink: '/',
      },
      {
        label: 'DPP',
        icon: 'pi pi-book',
        routerLink: '/dpp',
      },
      {
        label: 'Projekt',
        icon: 'pi pi-briefcase',
        items: [
          {
            label: 'Übersicht',
            icon: 'pi pi-info-circle',
            routerLink: '/projekt',
          },
          {
            label: 'Architektur',
            icon: 'pi pi-sitemap',
            routerLink: '/projekt/architektur',
          },
        ],
      },
      {
        label: 'Members',
        icon: 'pi pi-users',
        routerLink: '/members',
      },
    ];
  }
}
