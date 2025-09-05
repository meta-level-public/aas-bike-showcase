import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DataViewModule } from 'primeng/dataview';
import { SelectModule } from 'primeng/select';
import { KatalogEintrag } from '../model/katalog-eintrag';
import { GoodsItemComponent } from './goods-item/goods-item.component';
import { GoodsListService } from './goods-list.service';

@Component({
  selector: 'app-goods-list',
  templateUrl: './goods-list.component.html',
  styleUrl: './goods-list.component.css',
  imports: [
    CommonModule,
    FormsModule,
    DataViewModule,
    SelectModule,
    GoodsItemComponent,
  ],
})
export class GoodsListComponent implements OnInit {
  items: KatalogEintrag[] = [];
  groups: string[] = [];
  selectedCategory: string | undefined;
  currentGroupItems: KatalogEintrag[] = [];

  constructor(private goodsService: GoodsListService) {}

  ngOnInit(): void {
    this.loadData();
  }

  async loadData() {
    this.items = await this.goodsService.getAllRohteilInstanz();

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
        (item) => item.kategorie === this.selectedCategory,
      );
    }
  }

  itemDeleted(id: number) {
    this.items = this.items.filter((item) => item.id !== id);
    this.setCurrentGroupItems();
  }
}
