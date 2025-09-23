import { ISubmodelElement, Property, Range } from '@aas-core-works/aas-core3.0-typescript/types';
import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { SelectModule } from 'primeng/select';
import { v4 as guid } from 'uuid';
import { ConfiguredProduct } from '../model/configured-product';
import { ProducedProductRequest } from '../model/produced-product-request';
import { ProductPart } from '../model/product-part';
import { ProductionOrder } from '../model/production-order';
import { NotificationService } from '../notification.service';
import { ProductionOrderListService } from '../production-order-list/production-order-list.service';
import { AssemblyService } from './assembly.service';
@Component({
  selector: 'app-assembly',
  templateUrl: './assembly.component.html',
  styleUrl: './assembly.component.css',
  imports: [CommonModule, FormsModule, InputTextModule, ButtonModule, SelectModule, DialogModule],
})
export class AssemblyComponent implements OnInit, OnDestroy {
  private router = inject(Router);
  // Signals für den State
  productionOrders = signal<ProductionOrder[]>([]);
  selectedProductionOrder = signal<ProductionOrder | undefined>(undefined);
  items = signal<ConfiguredProduct[]>([]);
  selectedItem = signal<ConfiguredProduct | undefined>(undefined);
  newProduct = signal<ProducedProductRequest>({} as ProducedProductRequest);
  loading = signal<boolean>(false);
  currentAssemblyStep = signal<number>(0);
  showToolDialog = signal<boolean>(false);
  teileStatus = signal<
    {
      guid: string;
      typeGlobalAssetId: string;
      instanceGlobalAssetId: string;
      instanceAasId: string;
      statusOk: boolean;
      bestandteilId: number;
    }[]
  >([]);
  isRouteBasedSelection = signal<boolean>(false); // Zeigt an, ob ProductionOrder über Route ausgewählt wurde

  toolResultOk = signal(false);

  // Computed signals
  bestandteileLength = computed(() => {
    return this.selectedItem()?.bestandteile?.length || 0;
  });

  hasBestandteile = computed(() => {
    return this.bestandteileLength() > 0;
  });

  assemblySubmodel = computed(() => {
    const id = this.selectedItem()?.bestandteile[this.currentAssemblyStep()]?.id;
    if (id == null) {
      return null;
    } else {
      return this.service.getAssemblySubmodel(id);
    }
  });

  isToolRequired = computed(async () => {
    const submodel = await this.assemblySubmodel();
    if (!submodel) {
      return false;
    }

    const elements = submodel.submodelElements ?? [];
    return elements.some(
      (sme: any) =>
        typeof sme?.idShort === 'string' && sme.idShort.toLowerCase() === 'required_tool'
    );
  });

  requiredToolAasId = computed(async () => {
    const submodel = await this.assemblySubmodel();
    if (!submodel) {
      return null;
    }
    const elements = submodel.submodelElements ?? [];
    const requiredTool = elements.find(
      (sme: any) =>
        typeof sme?.idShort === 'string' && sme.idShort.toLowerCase() === 'required_tool'
    );
    if (requiredTool instanceof Property) {
      return requiredTool.value;
    }
    return null;
  });

  allPartsValid = computed(() => {
    const product = this.newProduct();
    if (product.bestandteilRequests != null) {
      return this.teileStatus().every((status) => status.statusOk === true);
    }
    return false;
  });

  assemblyComplete = computed(() => {
    return (
      this.currentAssemblyStep() >= this.bestandteileLength() - 1 && this.allPartsValid()
      // &&
      // (!this.isToolRequired() || this.toolResultOk())
    );
  });

  constructor(
    private service: AssemblyService,
    private productionOrderService: ProductionOrderListService,
    private notificationService: NotificationService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  ngOnDestroy(): void {
    // Clean up any running intervals
    this.stopTorquePolling();
  }

  async loadData() {
    // Lade verfügbare ProductionOrders
    const productionOrders = await this.productionOrderService.getAllProductionOrders();
    // Filtere nur Orders, die noch nicht abgeschlossen sind
    const pendingOrders = productionOrders.filter((order) => !order.produktionAbgeschlossen);
    this.productionOrders.set(pendingOrders);

    // Lade alle verfügbaren Produkte (für Fallback)
    const data = await this.service.getAllFertigteil();
    this.items.set(data);

    // Erst nach dem Laden der Daten Route-Parameter behandeln
    this.handleRouteParameter();
  }

  handleRouteParameter(): void {
    const productionOrderId = this.route.snapshot.paramMap.get('id');
    console.log('Production Order ID from route:', productionOrderId);
    if (productionOrderId) {
      // Wenn eine ProductionOrder-ID übergeben wurde, wähle sie aus
      const orderId = parseInt(productionOrderId, 10);
      if (!isNaN(orderId)) {
        this.isRouteBasedSelection.set(true);
        this.selectProductionOrderById(orderId);
      }
    } else {
      // Keine ID über Route - Dropdown soll angezeigt werden
      this.isRouteBasedSelection.set(false);
    }
  }

  async selectProductionOrderById(orderId: number) {
    const orders = this.productionOrders();
    const selectedOrder = orders.find((order) => order.id === orderId);
    if (selectedOrder) {
      this.selectedProductionOrder.set(selectedOrder);
      this.productionOrderSelected();
    } else {
      // ProductionOrder nicht gefunden - fallback auf normale Auswahl
      console.warn(`ProductionOrder with ID ${orderId} not found`);
      this.isRouteBasedSelection.set(false);
      this.notificationService.showMessageAlways(
        `Produktionsauftrag mit ID ${orderId} nicht gefunden.`
      );
    }
  }

  productionOrderSelected() {
    const selectedOrder = this.selectedProductionOrder();
    if (selectedOrder) {
      // Finde das entsprechende ConfiguredProduct basierend auf der ProductionOrder
      const items = this.items();
      const correspondingProduct = items.find(
        (item) => item.id === selectedOrder.configuredProductId
      );
      if (correspondingProduct) {
        this.selectedItem.set(correspondingProduct);
        this.initializeAssembly();
      } else {
        console.warn(`ConfiguredProduct with ID ${selectedOrder.configuredProductId} not found`);
        this.notificationService.showMessageAlways(
          `Produktkonfiguration für Auftrag nicht gefunden.`
        );
      }
    }
  }

  initializeAssembly() {
    const selectedItem = this.selectedItem();
    const selectedOrder = this.selectedProductionOrder();
    this.toolInitialized.set(false);
    if (selectedItem && selectedOrder) {
      console.log('Starting assembly for:', selectedItem);
      this.teileStatus.set([]);
      this.currentAssemblyStep.set(0);

      if (selectedItem?.bestandteile != null) {
        this.newProduct.update((product) => ({
          ...product,
          bestandteilRequests: [],
          configuredProductId: selectedItem.id ?? 0,
          productionOrderId: selectedOrder.id,
        }));

        const newTeileStatus: {
          guid: string;
          typeGlobalAssetId: string;
          instanceGlobalAssetId: string;
          instanceAasId: string;
          statusOk: boolean;
          bestandteilId: number;
        }[] = [];
        selectedItem.bestandteile.forEach((bestandteil) => {
          for (let i = 0; i < bestandteil.amount; i++) {
            newTeileStatus.push({
              guid: guid(),
              typeGlobalAssetId: bestandteil.katalogEintrag?.globalAssetId ?? '',
              instanceGlobalAssetId: '',
              instanceAasId: bestandteil.katalogEintrag?.aasId ?? '',
              statusOk: false,
              bestandteilId: bestandteil.id ?? 0,
            });
          }
        });
        this.teileStatus.set(newTeileStatus);
      }
    }
  }

  async findRohteilInstanz(event: any, item: ProductPart, guid: string) {
    if (event.target.value == '') return;
    try {
      this.loading.set(true);
      const katalogEintrag = await this.service.getRohteilInstanz(event.target.value);
      if (katalogEintrag == null) {
        this.notificationService.showMessageAlways('Rohteil Instanz nicht gefunden');
      } else {
        console.log(this.teileStatus());
        const currentTeileStatus = this.teileStatus();
        const teileStatusItem = currentTeileStatus.find(
          (teileStatus) =>
            teileStatus.typeGlobalAssetId == item.katalogEintrag?.globalAssetId &&
            teileStatus.guid == guid
        );
        console.log(teileStatusItem);
        if (teileStatusItem == null) return;
        if (katalogEintrag.referencedType?.globalAssetId != item.katalogEintrag?.globalAssetId) {
          this.notificationService.showMessageAlways('Rohteil hat den falschen Typ!');
        } else {
          // Update the specific item in the array
          this.teileStatus.update((statuses) =>
            statuses.map((status) =>
              status.guid === guid
                ? {
                    ...status,
                    instanceGlobalAssetId: katalogEintrag.globalAssetId,
                    statusOk: true,
                    instanceAasId: katalogEintrag.aasId,
                  }
                : status
            )
          );
        }
      }
    } finally {
      this.loading.set(false);
    }
  }

  getParts(item: ProductPart) {
    return this.teileStatus().filter(
      (teileStatus) => teileStatus.typeGlobalAssetId == item.katalogEintrag?.globalAssetId
    );
  }

  isTeilOk(item: ProductPart) {
    return this.teileStatus()
      .filter((teileStatus) => teileStatus.typeGlobalAssetId == item.katalogEintrag?.globalAssetId)
      .every((teileStatus) => teileStatus.statusOk);
  }

  nextAssemblyStep() {
    const currentStep = this.currentAssemblyStep();
    this.toolResultOk.set(false);
    this.toolInitialized.set(false);
    if (currentStep < this.bestandteileLength() - 1) {
      this.currentAssemblyStep.set(currentStep + 1);
    }
    // Stoppe ggf. laufendes Polling beim Schrittwechsel
    this.stopTorquePolling();
    // Falls der Dialog offen bleibt, Polling für den neuen Schritt neu starten
    if (this.showToolDialog()) {
      this.startTorquePolling();
    }
  }

  // Prüft, ob der aktuelle Montageschritt "Lenker" im Namen enthält - entfernt da bereits als computed signal definiert

  // Öffnet den Schrauben-Dialog
  openToolDialog(): void {
    // hier muss jetzt das benötigte drehmoment an den schrauber übergeben werden
    this.toolInitialized.set(false);
    this.showToolDialog.set(true);
    // Starte Live-Abfrage des aktuellen Drehmoments
    this.startTorquePolling();
  }

  // Schließt den Schrauben-Dialog
  closeToolDialog(nextStep: boolean) {
    // hier muss das genutzte drehmoment vom schrauber gelesen und überprüft werden
    this.showToolDialog.set(false);
    // Beende Live-Abfrage
    this.stopTorquePolling();
    const currentStep = this.currentAssemblyStep();
    let hasNextStep = false;
    if (currentStep < this.bestandteileLength() - 1) {
      hasNextStep = true;
    }

    if (nextStep && hasNextStep) {
      this.nextAssemblyStep();
    }
  }

  async saveProducedProduct() {
    const currentProduct = this.newProduct();
    const selectedOrder = this.selectedProductionOrder();
    if (currentProduct == null) return;

    const currentTeileStatus = this.teileStatus();
    const selectedItem = this.selectedItem();

    const bestandteilRequests = currentTeileStatus
      .map((teileStatus) => {
        const typePart = selectedItem?.bestandteile?.find(
          (bestandteil) =>
            bestandteil.katalogEintrag?.globalAssetId == teileStatus.typeGlobalAssetId
        );
        if (typePart == null) return null;
        return {
          amount: 1,
          usageDate: new Date(),
          globalAssetId: teileStatus.instanceGlobalAssetId,
        };
      })
      .filter((req) => req !== null);

    const productToSave = {
      ...currentProduct,
      bestandteilRequests,
    };

    try {
      this.loading.set(true);
      await this.service.createProduct(productToSave);

      // Markiere ProductionOrder als abgeschlossen, wenn eine ausgewählt ist
      if (selectedOrder?.id) {
        await this.productionOrderService.markProductionCompleted(selectedOrder.id);
        this.notificationService.showMessageAlways(
          'Produkt erfolgreich erstellt und Produktionsauftrag als abgeschlossen markiert'
        );
      } else {
        this.notificationService.showMessageAlways('Produkt erfolgreich erstellt');
      }

      this.router.navigate(['/production/assembly']);

      this.newProduct.set({} as ProducedProductRequest);
      this.selectedProductionOrder.set(undefined);
      this.selectedItem.set(undefined);
      this.teileStatus.set([]);
      this.currentAssemblyStep.set(0);

      // Lade die ProductionOrders neu, um aktualisierte Liste zu erhalten
      // await this.loadData();
    } finally {
      this.loading.set(false);
    }
  }

  hasNextStep() {
    const currentStep = this.currentAssemblyStep();
    return currentStep < this.bestandteileLength() - 1;
  }

  toolInitialized = signal(false);

  async initializeTool() {
    const requiredTightenForce = await this.requiredTightenForce();
    const requiredToolAasId = await this.requiredToolAasId();

    if (!requiredToolAasId) {
      this.notificationService.showMessageAlways('Fehler: RequiredTool AAS ID nicht gefunden');
      return;
    }
    if (requiredTightenForce == null || requiredTightenForce === '') {
      this.notificationService.showMessageAlways('Fehler: required_tighten_force nicht gefunden');
      return;
    }
    await this.service.initializeTool(requiredToolAasId, 'TargetTorque', requiredTightenForce);
    // await this.service.initializeTool(requiredToolAasId, 'requiredTorque', requiredTightenForce);

    this.toolInitialized.set(true);
  }

  requiredTightenForce = computed(async () => {
    const sm = await this.assemblySubmodel();
    var requiredTightenForceEl = sm?.submodelElements?.find(
      (el) => el.idShort === 'required_tighten_force'
    );
    if (requiredTightenForceEl instanceof Property) {
      return requiredTightenForceEl.value;
    } else {
      return null;
    }
  });

  allowedTightenForceElement = computed(async () => {
    const sm = await this.assemblySubmodel();
    var allowedTightenForceEl = sm?.submodelElements?.find(
      (el) => el.idShort === 'allowed_tighten_force'
    );
    return allowedTightenForceEl;
  });

  currentTightenForce = signal(0);
  configuredRequiredTightenForce = signal(0);
  private torqueIntervalId: any | null = null;

  // Startet ein 5s-Polling zur Aktualisierung von currentTightenForce
  private startTorquePolling(): void {
    this.stopTorquePolling();
    // Einmal sofort aktualisieren
    void this.updateToolDataOnce();
    this.torqueIntervalId = setInterval(() => {
      void this.updateToolDataOnce();
    }, 1000);
  }

  // Stoppt das Polling
  private stopTorquePolling(): void {
    if (this.torqueIntervalId != null) {
      clearInterval(this.torqueIntervalId);
      this.torqueIntervalId = null;
    }
  }

  // Liest einmalig das aktuelle Drehmoment und setzt das Signal
  private async updateToolDataOnce(): Promise<void> {
    try {
      const aasId = await this.requiredToolAasId();
      if (!aasId) return;
      const toolData = await this.service.getToolData(aasId);
      if (!toolData) return;
      let vCurr = 0;
      const elCurrentTorque = toolData.submodelElements?.find(
        (e) => e.idShort === 'LastMeasuredTorque'
      );
      if (elCurrentTorque instanceof Property) {
        vCurr = +(elCurrentTorque.value ?? '0');
      }
      this.currentTightenForce.set(vCurr);

      let vReq = 0;
      const elRequiredTorque = toolData.submodelElements?.find(
        (e) => e.idShort === 'requiredTorque'
      );
      if (elRequiredTorque instanceof Property) {
        vReq = +(elRequiredTorque.value ?? '0');
      }
      this.configuredRequiredTightenForce.set(vReq);
    } catch {
      // still, keep silent for polling
    }
  }

  showFormattedValue(sme: ISubmodelElement) {
    if (sme instanceof Property) {
      return sme.value;
    }
    if (sme instanceof Range) {
      return `${sme.min} - ${sme.max}`;
    }
    return '';
  }

  async checkToolData() {
    console.log('checking tool Data');

    const aasId = await this.requiredToolAasId();
    if (aasId == null || aasId == '') {
      this.notificationService.showMessageAlways('Fehler: AAS ID nicht gefunden');
      return;
    }

    const toolData = await this.service.getToolData(aasId);
    if (!toolData) {
      this.notificationService.showMessageAlways('Fehler: Werkzeugdaten nicht gefunden');
      return;
    }

    let currentTightenForce = 0;
    const currentTightenForceElement = toolData?.submodelElements?.find(
      (el) => el.idShort === 'LastMeasuredTorque'
    );
    if (currentTightenForceElement instanceof Property) {
      currentTightenForce = +(currentTightenForceElement.value ?? '');
    }
    this.currentTightenForce.set(currentTightenForce);

    const allowedTightenForceElement = await this.allowedTightenForceElement();
    if (allowedTightenForceElement instanceof Range) {
      const allowedTightenForceMin = +(allowedTightenForceElement.min ?? '') + currentTightenForce;
      const allowedTightenForceMax = +(allowedTightenForceElement.max ?? '') + currentTightenForce;

      console.log(allowedTightenForceMin);
      console.log(allowedTightenForceMax);
      console.log(currentTightenForce);

      if (
        currentTightenForce < allowedTightenForceMin &&
        currentTightenForce > allowedTightenForceMax
      ) {
        this.notificationService.showMessageAlways('Fehler: Ungültiges Drehmoment');
        this.toolResultOk.set(false);
      } else {
        // this.notificationService.showMessageAlways('Drehmoment ist gültig');
        this.toolResultOk.set(true);
        this.closeToolDialog(true);
      }
    }
  }
}
