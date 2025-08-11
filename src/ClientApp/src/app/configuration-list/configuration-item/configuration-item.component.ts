import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { PopoverModule } from 'primeng/popover';
import { RatingModule } from 'primeng/rating';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ConfiguredProduct } from '../../model/configured-product';
import { InventoryStatus } from '../../model/inventory-status';
import { OrderDialogComponent } from '../../order-dialog/order-dialog.component';
import { ConfigurationListService } from '../configuration-list.service';

@Component({
  selector: 'app-configuration-item',
  templateUrl: './configuration-item.component.html',
  styleUrl: './configuration-item.component.css',
  imports: [CommonModule, FormsModule, RatingModule, ButtonModule, PopoverModule, TagModule, TooltipModule, OrderDialogComponent]
})
export class ConfigurationItemComponent {
  @Input() item: ConfiguredProduct | undefined;
  @Input() first: boolean = false;
  InventoryStatus = InventoryStatus;
  loading: boolean = false;
  showOrderDialog: boolean = false;
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

  openOrderDialog() {
    this.showOrderDialog = true;
  }

  onOrderCreated() {
    // Hier können Sie zusätzliche Aktionen nach der Bestellerstellung hinzufügen
    // z.B. eine Benachrichtigung anzeigen oder die Liste aktualisieren
    console.log('Bestellung wurde erfolgreich erstellt');
  }

  getSeverity(eintrag: ConfiguredProduct) {
    // Für ConfiguredProduct müssen wir eine andere Logik verwenden
    // da es keine inventoryStatus Eigenschaft hat
    return 'success'; // Default
  }

  // Neue Methode für Bestandteile-Popover
  getTotalBestandteile(): number {
    return this.item?.bestandteile?.length || 0;
  }

  getTotalPrice(): number {
    return this.item?.bestandteile?.reduce((sum, teil) => sum + (teil.price * teil.amount), 0) || 0;
  }
}
