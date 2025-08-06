import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { RatingModule } from 'primeng/rating';
import { InventoryStatus } from 'src/app/model/inventory-status';
import { KatalogEintrag } from '../../model/katalog-eintrag';
import { CatalogListService } from '../catalog-list.service';

@Component({
  selector: 'app-catalog-item',
  templateUrl: './catalog-item.component.html',
  styleUrl: './catalog-item.component.css',
  imports: [CommonModule, FormsModule, RatingModule, ButtonModule]
})
export class CatalogItemComponent {
  @Input() item: KatalogEintrag | undefined;
  @Input() first: boolean = false;
  @Input() showDelete: boolean = false;
  @Input() imgUrl: string = '';
  InventoryStatus = InventoryStatus;
  loading: boolean = false;
  @Output() itemDeleted: EventEmitter<number> = new EventEmitter<number>();

  constructor(private service: CatalogListService) {}

  async deleteItem() {
    if (this.item?.id != null) {
      try {
        this.loading = true;
        await this.service.deleteItem(this.item.id);
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
