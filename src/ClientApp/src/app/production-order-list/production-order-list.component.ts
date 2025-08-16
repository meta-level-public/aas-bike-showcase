import { CommonModule } from '@angular/common';
import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { DataViewModule } from 'primeng/dataview';
import { SelectModule } from 'primeng/select';
import { TagModule } from 'primeng/tag';
import { ToastModule } from 'primeng/toast';
import {
  ProductionOrder,
  ProductionOrderStatus,
} from '../model/production-order';
import { ProductionOrderListService } from './production-order-list.service';

interface FilterOption {
  label: string;
  value: ProductionOrderStatus | 'ALL';
}

@Component({
  selector: 'app-production-order-list',
  templateUrl: './production-order-list.component.html',
  styleUrl: './production-order-list.component.css',
  imports: [
    CommonModule,
    FormsModule,
    DataViewModule,
    CardModule,
    ButtonModule,
    SelectModule,
    TagModule,
    ToastModule,
  ],
  providers: [MessageService],
})
export class ProductionOrderListComponent implements OnInit {
  private router = inject(Router);

  filterOptions: FilterOption[] = [
    { label: 'Alle Aufträge', value: 'ALL' },
    {
      label: 'Zur Produktion anstehend',
      value: ProductionOrderStatus.PENDING_PRODUCTION,
    },
    {
      label: 'Zum Versand anstehend',
      value: ProductionOrderStatus.READY_FOR_SHIPPING,
    },
    { label: 'Abgeschlossen', value: ProductionOrderStatus.COMPLETED },
  ];

  // Signals für reaktive State-Verwaltung
  allOrders = signal<ProductionOrder[]>([]);
  selectedFilter = signal<FilterOption>(this.filterOptions[0]);
  loading = signal<boolean>(false);

  // Property für ngModel (da Signals nicht direkt mit ngModel funktionieren)
  selectedFilterModel: FilterOption = this.filterOptions[0];

  // Computed Signals für abgeleitete Werte
  filteredOrders = computed(() => {
    const orders = this.allOrders();
    const filter = this.selectedFilter();

    if (filter.value === 'ALL') {
      return [...orders];
    } else {
      return orders.filter(
        (order) => this.getOrderStatus(order) === filter.value
      );
    }
  });

  pendingProductionCount = computed(
    () => this.allOrders().filter((o) => !o.produktionAbgeschlossen).length
  );

  readyForShippingCount = computed(
    () =>
      this.allOrders().filter((o) => o.produktionAbgeschlossen && !o.versandt)
        .length
  );

  completedCount = computed(
    () => this.allOrders().filter((o) => o.versandt).length
  );

  constructor(
    private service: ProductionOrderListService,
    private messageService: MessageService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  async loadData() {
    this.loading.set(true);
    try {
      const orders = await this.service.getAllProductionOrders();
      this.allOrders.set(orders);
    } catch (error) {
      this.showError('Fehler beim Laden der Produktionsaufträge');
    } finally {
      this.loading.set(false);
    }
  }

  onFilterChange() {
    // Aktualisiere das Signal mit dem neuen Wert
    this.selectedFilter.set(this.selectedFilterModel);
  }

  getOrderStatus(order: ProductionOrder): ProductionOrderStatus {
    if (order.versandt) {
      return ProductionOrderStatus.COMPLETED;
    } else if (order.produktionAbgeschlossen) {
      return ProductionOrderStatus.READY_FOR_SHIPPING;
    } else {
      return ProductionOrderStatus.PENDING_PRODUCTION;
    }
  }

  getStatusLabel(order: ProductionOrder): string {
    const status = this.getOrderStatus(order);
    switch (status) {
      case ProductionOrderStatus.PENDING_PRODUCTION:
        return 'Zur Produktion anstehend';
      case ProductionOrderStatus.READY_FOR_SHIPPING:
        return 'Zum Versand anstehend';
      case ProductionOrderStatus.COMPLETED:
        return 'Abgeschlossen';
      default:
        return 'Unbekannt';
    }
  }

  getStatusSeverity(
    order: ProductionOrder
  ): 'success' | 'info' | 'warn' | 'danger' {
    const status = this.getOrderStatus(order);
    switch (status) {
      case ProductionOrderStatus.PENDING_PRODUCTION:
        return 'warn';
      case ProductionOrderStatus.READY_FOR_SHIPPING:
        return 'info';
      case ProductionOrderStatus.COMPLETED:
        return 'success';
      default:
        return 'danger';
    }
  }

  async startProduction(order: ProductionOrder) {
    if (!order.id) return;

    // navigiere zur route: /production/assembly/{productionOrderId}
    this.router.navigate(['/production/assembly', order.id]);
  }

  async markAsShipped(order: ProductionOrder) {
    if (!order.id) return;

    this.loading.set(true);
    try {
      const response = await this.service.markAsShipped(order.id);
      if (response.success) {
        this.showSuccess('Auftrag als versandt markiert');
        await this.loadData(); // Reload data
      } else {
        this.showError(
          response.message || 'Fehler beim Markieren des Versands'
        );
      }
    } catch (error) {
      this.showError('Fehler beim Markieren des Versands');
    } finally {
      this.loading.set(false);
    }
  }

  formatDate(date: Date | string | undefined): string {
    if (!date) return '-';
    const d = new Date(date);
    return (
      d.toLocaleDateString('de-DE') +
      ' ' +
      d.toLocaleTimeString('de-DE', { hour: '2-digit', minute: '2-digit' })
    );
  }

  getFullAddress(order: ProductionOrder): string {
    if (!order.address) return 'Keine Adresse angegeben';

    const parts = [];
    if (order.address.name || order.address.vorname) {
      parts.push(
        `${order.address.vorname || ''} ${order.address.name || ''}`.trim()
      );
    }
    if (order.address.strasse) parts.push(order.address.strasse);
    if (order.address.plz || order.address.ort) {
      parts.push(
        `${order.address.plz || ''} ${order.address.ort || ''}`.trim()
      );
    }
    if (order.address.land) parts.push(order.address.land);

    return parts.length > 0 ? parts.join(', ') : 'Keine Adresse angegeben';
  }

  private showSuccess(message: string) {
    this.messageService.add({
      severity: 'success',
      summary: 'Erfolg',
      detail: message,
    });
  }

  private showError(message: string) {
    this.messageService.add({
      severity: 'error',
      summary: 'Fehler',
      detail: message,
    });
  }
}
