import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { PopoverModule } from 'primeng/popover';
import { RatingModule } from 'primeng/rating';
import { TagModule } from 'primeng/tag';
import { TooltipModule } from 'primeng/tooltip';
import { ConfiguredProduct } from '../../model/configured-product';
import { InventoryStatus } from '../../model/inventory-status';
import { ConfigurationListService } from '../configuration-list.service';

@Component({
  selector: 'app-configuration-item',
  templateUrl: './configuration-item.component.html',
  styleUrl: './configuration-item.component.css',
  imports: [
    CommonModule,
    FormsModule,
    RatingModule,
    ButtonModule,
    PopoverModule,
    TagModule,
    TooltipModule,
  ],
})
export class ConfigurationItemComponent {
  @Input() item: ConfiguredProduct | undefined;
  @Input() first: boolean = false;
  InventoryStatus = InventoryStatus;
  loading: boolean = false;
  @Output() itemDeleted: EventEmitter<number> = new EventEmitter<number>();

  constructor(
    private service: ConfigurationListService,
    private router: Router,
  ) {}

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
    if (this.item?.id) {
      this.router.navigate(['/orders/create', this.item.id]);
    }
  }

  onOrderCreated() {
    // Diese Methode wird nicht mehr benötigt, da wir zur separaten Seite navigieren
    // Kann entfernt werden, falls sie nicht anderswo referenziert wird
    console.log('Navigating to order creation page');
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
    return (
      this.item?.bestandteile?.reduce(
        (sum, teil) => sum + teil.price * teil.amount,
        0,
      ) || 0
    );
  }
}
