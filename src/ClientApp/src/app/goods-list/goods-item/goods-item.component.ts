import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { RatingModule } from 'primeng/rating';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { InventoryStatus } from 'src/app/model/inventory-status';
import { KatalogEintrag } from '../../model/katalog-eintrag';
import { GoodsListService } from '../goods-list.service';

@Component({
  selector: 'app-goods-item',
  templateUrl: './goods-item.component.html',
  styleUrl: './goods-item.component.css',
  imports: [
    CommonModule,
    FormsModule,
    RatingModule,
    ButtonModule,
    TagModule,
    TooltipModule,
  ],
})
export class GoodsItemComponent {
  @Input() item: KatalogEintrag | undefined;
  @Input() first: boolean = false;
  InventoryStatus = InventoryStatus;
  @Output() itemDeleted: EventEmitter<number> = new EventEmitter<number>();
  loading: boolean = false;

  constructor(private service: GoodsListService) {}

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

  async copyGlobalAssetId() {
    if (this.item?.globalAssetId) {
      try {
        await navigator.clipboard.writeText(this.item.globalAssetId);
        console.log(
          'GlobalAssetId copied to clipboard:',
          this.item.globalAssetId,
        );
      } catch (err) {
        console.error('Failed to copy GlobalAssetId to clipboard:', err);
        // Fallback für ältere Browser
        this.fallbackCopyTextToClipboard(this.item.globalAssetId);
      }
    }
  }

  private fallbackCopyTextToClipboard(text: string) {
    const textArea = document.createElement('textarea');
    textArea.value = text;
    textArea.style.position = 'fixed';
    textArea.style.left = '-999999px';
    textArea.style.top = '-999999px';
    document.body.appendChild(textArea);
    textArea.focus();
    textArea.select();
    try {
      document.execCommand('copy');
      console.log('GlobalAssetId copied to clipboard (fallback):', text);
    } catch (err) {
      console.error(
        'Fallback: Failed to copy GlobalAssetId to clipboard:',
        err,
      );
    }
    document.body.removeChild(textArea);
  }
}
