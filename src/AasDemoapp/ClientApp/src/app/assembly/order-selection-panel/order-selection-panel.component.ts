import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SelectModule } from 'primeng/select';
import { ProductionOrder } from '../../model/production-order';

@Component({
  selector: 'app-order-selection-panel',
  templateUrl: './order-selection-panel.component.html',
  styleUrl: './order-selection-panel.component.css',
  imports: [CommonModule, FormsModule, SelectModule],
})
export class OrderSelectionPanelComponent {
  @Input() productionOrders: ProductionOrder[] = [];
  @Input() selectedOrder: ProductionOrder | undefined;
  @Input() isRouteBasedSelection: boolean = false;
  @Input() currentStep: number = 0;
  @Input() totalSteps: number = 0;
  @Input() hasBestandteile: boolean = false;

  @Output() orderSelected = new EventEmitter<ProductionOrder>();

  onOrderChange(order: ProductionOrder) {
    this.orderSelected.emit(order);
  }
}
