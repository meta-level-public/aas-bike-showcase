import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DataViewModule } from 'primeng/dataview';
import { ConfiguredProduct } from '../model/configured-product';
import { ConfigurationItemComponent } from './configuration-item/configuration-item.component';
import { ConfigurationListService } from './configuration-list.service';

@Component({
  selector: 'app-configuration-list',
  templateUrl: './configuration-list.component.html',
  styleUrl: './configuration-list.component.css',
  imports: [
    CommonModule,
    FormsModule,
    DataViewModule,
    ConfigurationItemComponent,
  ],
})
export class ConfigurationListComponent implements OnInit {
  items: ConfiguredProduct[] = [];

  constructor(private service: ConfigurationListService) {}

  ngOnInit(): void {
    this.loadData();
  }

  async loadData() {
    this.items = await this.service.getAllFertigteil();
  }

  itemDeleted(id: number) {
    this.items = this.items.filter((item) => item.id !== id);
  }
}
