import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { RatingModule } from 'primeng/rating';
import { InventoryStatus } from '../../model/inventory-status';
import { KatalogEintrag } from '../../model/katalog-eintrag';
import { ConfigurationListService } from '../configuration-list.service';

@Component({
  selector: 'app-configuration-item',
  templateUrl: './configuration-item.component.html',
  styleUrl: './configuration-item.component.css',
  imports: [CommonModule, FormsModule, RatingModule, ButtonModule]
})
export class ConfigurationItemComponent {
  @Input() item: KatalogEintrag | undefined;
  @Input() first: boolean = false;
  InventoryStatus = InventoryStatus;
  loading: boolean = false;
  @Output() itemDeleted: EventEmitter<number> = new EventEmitter<number>();

  constructor(private service: ConfigurationListService) {}

  deleteItem() {
    if (this.item?.id != null) {
      try {
        this.loading = true;
        this.service.deleteItem(this.item.id);
        this.itemDeleted.emit(this.item.id);
      } finally {
        this.loading = false;
      }
    }
  }

  getSeverity(eintrag: KatalogEintrag) {
    switch (eintrag.inventoryStatus) {
      case InventoryStatus.INSTOCK:
        return 'success';

      case InventoryStatus.LOWSTOCK:
        return 'warning';

      case InventoryStatus.OUTOFSTOCK:
        return 'danger';

      default:
        return undefined;
    }
  }
}
