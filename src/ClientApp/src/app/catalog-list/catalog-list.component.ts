
import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { DataViewModule } from 'primeng/dataview';
import { DropdownModule } from 'primeng/dropdown';
import { KatalogEintrag } from '../model/katalog-eintrag';
import { CatalogItemComponent } from './catalog-item/catalog-item.component';
import { CatalogListService } from './catalog-list.service';

@Component({
  selector: 'app-catalog-list',
  templateUrl: './catalog-list.component.html',
  styleUrl: './catalog-list.component.css',
  imports: [CommonModule, FormsModule, CatalogItemComponent, DropdownModule, DataViewModule, ButtonModule]
})
export class CatalogListComponent implements OnInit {
  items: KatalogEintrag[] = [];
  groups: string[] = [];
  selectedCategory: string | undefined;
  currentGroupItems: KatalogEintrag[] = [];

  constructor(private catalogService: CatalogListService, private cdRef: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.loadData();
  }

  async loadData() {
    this.items = await this.catalogService.getAllRohteil();

    this.groups = [
      'Alle',
      ...new Set(this.items.map((item) => item.kategorie)),
    ];
    this.selectedCategory = 'Alle';
    this.setCurrentGroupItems();
  }

  setCurrentGroupItems() {
    if (this.selectedCategory === 'Alle') {
      this.currentGroupItems = this.items;
    } else {
      this.currentGroupItems = this.items.filter(
        (item) => item.kategorie === this.selectedCategory
      );
    }
  }

  itemDeleted(id: number) {
    this.items = this.items.filter((item) => item.id !== id);
    this.setCurrentGroupItems();
  }
}
