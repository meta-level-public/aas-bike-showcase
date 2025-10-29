import { ISubmodelElement, Property, Range } from '@aas-core-works/aas-core3.0-typescript/types';
import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
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
import {
  AssemblyStepsPanelComponent,
  PartScanEvent,
} from './assembly-steps-panel/assembly-steps-panel.component';
import { AssemblyService } from './assembly.service';
import { OrderSelectionPanelComponent } from './order-selection-panel/order-selection-panel.component';
import { ToolDialogComponent } from './tool-dialog/tool-dialog.component';

@Component({
  selector: 'app-assembly',
  templateUrl: './assembly.component.html',
  styleUrl: './assembly.component.css',
  imports: [
    CommonModule,
    FormsModule,
    InputTextModule,
    ButtonModule,
    SelectModule,
    DialogModule,
    OrderSelectionPanelComponent,
    AssemblyStepsPanelComponent,
    ToolDialogComponent,
  ],
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
  showPdfDialog = signal<boolean>(false);
  pdfBlobUrl = signal<SafeResourceUrl | undefined>(undefined);
  pdfBlob = signal<Blob | undefined>(undefined);
  pdfFileName = signal<string>('handover_documentation.pdf');
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
  toolCheckLoading = signal(false); // Loading state für Tool-Überprüfung

  // Computed signals für Assembly Steps Panel
  currentParts = computed(() => {
    const currentBestandteil = this.selectedItem()?.bestandteile[this.currentAssemblyStep()];
    if (!currentBestandteil) return [];
    return this.getParts(currentBestandteil);
  });

  teilStatusMap = computed(() => {
    const map = new Map<number, boolean>();
    const currentBestandteil = this.selectedItem()?.bestandteile[this.currentAssemblyStep()];
    if (currentBestandteil && currentBestandteil.id !== undefined) {
      map.set(currentBestandteil.id, this.isTeilOk(currentBestandteil));
    }
    return map;
  });

  isToolRequiredForCurrentStep = computed(() => {
    // Für Computed Signals können wir nicht async verwenden
    // Wir verwenden ein separates Signal für den async Zustand
    return false; // Placeholder
  });

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

  isToolRequired = computed(() => {
    // Computed signals können nicht async sein - wir verwenden die originale Implementierung
    const id = this.selectedItem()?.bestandteile[this.currentAssemblyStep()]?.id;
    if (id == null) {
      return Promise.resolve(false);
    }

    return this.service.getAssemblySubmodel(id).then((submodel: any) => {
      if (!submodel) {
        return false;
      }
      const elements = submodel.submodelElements ?? [];
      return elements.some(
        (sme: any) =>
          typeof sme?.idShort === 'string' && sme.idShort.toLowerCase() === 'required_tool'
      );
    });
  });

  requiredToolAasId = computed(() => {
    const id = this.selectedItem()?.bestandteile[this.currentAssemblyStep()]?.id;
    if (id == null) {
      return Promise.resolve(null);
    }

    return this.service.getAssemblySubmodel(id).then((submodel: any) => {
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
      this.currentAssemblyStep() >= this.bestandteileLength() - 1 &&
      this.allPartsValid() &&
      (!this.isToolRequiredValue() || this.toolResultOk())
    );
  });

  constructor(
    private service: AssemblyService,
    private productionOrderService: ProductionOrderListService,
    private notificationService: NotificationService,
    private route: ActivatedRoute,
    private sanitizer: DomSanitizer
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
      // Initialize async values for the first step
      this.updateAsyncValues();

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
    // Setze Zustände für neuen Schritt zurück
    this.toolResultOk.set(false);
    this.toolInitialized.set(false);
    this.toolCheckLoading.set(false);

    if (currentStep < this.bestandteileLength() - 1) {
      this.currentAssemblyStep.set(currentStep + 1);
      // Update async values for the new step
      this.updateAsyncValues();
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
      const response = await this.service.createProduct(productToSave);
      console.log('Produced Product created:', response);
      console.log('PDF URL:', response.handoverDocumentationPdfUrl);

      // Markiere ProductionOrder als abgeschlossen, wenn eine ausgewählt ist
      if (selectedOrder?.id) {
        await this.productionOrderService.markProductionCompleted(selectedOrder.id);
        if (!response.handoverDocumentationPdfBase64) {
          this.notificationService.showMessageAlways(
            'Produkt erfolgreich erstellt und Produktionsauftrag als abgeschlossen markiert'
          );
        }
      }

      // Zeige PDF-Dialog an, wenn verfügbar
      if (response.handoverDocumentationPdfBase64) {
        try {
          // Konvertiere Base64 zu Blob
          const byteCharacters = atob(response.handoverDocumentationPdfBase64);
          const byteNumbers = new Array(byteCharacters.length);
          for (let i = 0; i < byteCharacters.length; i++) {
            byteNumbers[i] = byteCharacters.charCodeAt(i);
          }
          const byteArray = new Uint8Array(byteNumbers);
          const pdfBlob = new Blob([byteArray], { type: 'application/pdf' });

          this.pdfBlob.set(pdfBlob);

          // Erstelle eine Blob-URL für die Anzeige und sanitize sie
          const blobUrl = URL.createObjectURL(pdfBlob);
          const safeBlobUrl = this.sanitizer.bypassSecurityTrustResourceUrl(blobUrl);
          this.pdfBlobUrl.set(safeBlobUrl);

          // Setze Dateiname
          const fileName =
            response.handoverDocumentationPdfFileName || 'handover_documentation.pdf';
          this.pdfFileName.set(fileName);

          this.showPdfDialog.set(true);
        } catch (error) {
          console.error('Error converting Base64 to PDF:', error);
          this.notificationService.showMessageAlways('Fehler beim Laden des PDFs');
          this.router.navigate(['/production/assembly']);
        }
      } else {
        // Nur navigieren, wenn kein PDF vorhanden
        this.router.navigate(['/production/assembly']);
      }

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

  closePdfDialog() {
    // Räume die Blob-URL auf, um Speicherlecks zu vermeiden
    const safeBlobUrl = this.pdfBlobUrl();
    if (safeBlobUrl) {
      // Extract the actual URL from the SafeResourceUrl
      const urlString = (safeBlobUrl as any).changingThisBreaksApplicationSecurity;
      if (urlString) {
        URL.revokeObjectURL(urlString);
      }
    }

    this.showPdfDialog.set(false);
    this.pdfBlobUrl.set(undefined);
    this.pdfBlob.set(undefined);
    this.router.navigate(['/production/assembly']);
  }

  printPdf() {
    const safeBlobUrl = this.pdfBlobUrl();
    if (safeBlobUrl) {
      // Extract the actual URL from the SafeResourceUrl
      const urlString = (safeBlobUrl as any).changingThisBreaksApplicationSecurity;
      if (urlString) {
        // Öffne das PDF in einem neuen Fenster und löse den Druck aus
        const printWindow = window.open(urlString, '_blank');
        if (printWindow) {
          printWindow.onload = () => {
            printWindow.print();
          };
        }
      }
    }
  }

  downloadPdf() {
    const blob = this.pdfBlob();
    const fileName = this.pdfFileName();
    if (blob) {
      const link = document.createElement('a');
      link.href = URL.createObjectURL(blob);
      link.download = fileName;
      link.click();
      URL.revokeObjectURL(link.href);
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

  requiredTightenForce = computed(() => {
    const id = this.selectedItem()?.bestandteile[this.currentAssemblyStep()]?.id;
    if (id == null) {
      return Promise.resolve(null);
    }

    return this.service.getAssemblySubmodel(id).then((sm: any) => {
      var requiredTightenForceEl = sm?.submodelElements?.find(
        (el: any) => el.idShort === 'required_tighten_force'
      );
      if (requiredTightenForceEl instanceof Property) {
        return requiredTightenForceEl.value;
      } else {
        return null;
      }
    });
  });

  allowedTightenForceElement = computed(() => {
    const id = this.selectedItem()?.bestandteile[this.currentAssemblyStep()]?.id;
    if (id == null) {
      return Promise.resolve(null);
    }

    return this.service.getAssemblySubmodel(id).then((sm: any) => {
      var allowedTightenForceEl = sm?.submodelElements?.find(
        (el: any) => el.idShort === 'allowed_tighten_force'
      );
      return allowedTightenForceEl;
    });
  });

  currentTightenForce = signal(0);
  configuredRequiredTightenForce = signal(0);
  requiredTightenForceValue = signal<number | null>(null);
  isToolRequiredValue = signal<boolean>(false);
  private torqueTimeoutId: any | null = null;
  private isPollingActive = false;

  // Startet ein sequenzielles Polling zur Aktualisierung von currentTightenForce
  private startTorquePolling(): void {
    this.stopTorquePolling();
    this.isPollingActive = true;
    // Einmal sofort aktualisieren und dann sequenziell weitermachen
    void this.updateToolDataOnce();
  }

  // Stoppt das Polling
  private stopTorquePolling(): void {
    this.isPollingActive = false;
    if (this.torqueTimeoutId != null) {
      clearTimeout(this.torqueTimeoutId);
      this.torqueTimeoutId = null;
    }
  }

  // Liest einmalig das aktuelle Drehmoment und setzt das Signal
  private async updateToolDataOnce(): Promise<void> {
    if (!this.isPollingActive) return;

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
    } finally {
      // Nächsten Aufruf erst 1 Sekunde nach Abschluss des aktuellen Aufrufs planen
      if (this.isPollingActive) {
        this.torqueTimeoutId = setTimeout(() => {
          void this.updateToolDataOnce();
        }, 1000);
      }
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
    this.toolCheckLoading.set(true);

    try {
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

      const requiredTightenForce = +((await this.requiredTightenForce()) ?? 0);

      const allowedTightenForceElement = await this.allowedTightenForceElement();
      if (allowedTightenForceElement instanceof Range) {
        const allowedTightenForceMin =
          +(allowedTightenForceElement.min ?? '') + requiredTightenForce;
        const allowedTightenForceMax =
          +(allowedTightenForceElement.max ?? '') + requiredTightenForce;

        console.log(allowedTightenForceMin);
        console.log(allowedTightenForceMax);
        console.log(currentTightenForce);

        if (
          currentTightenForce < allowedTightenForceMin ||
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
    } catch (error) {
      console.error('Error checking tool data:', error);
      this.notificationService.showMessageAlways('Fehler beim Prüfen der Werkzeugdaten');
    } finally {
      this.toolCheckLoading.set(false);
    }
  }

  // Event Handler Methods for Child Components

  onOrderSelected(order: ProductionOrder) {
    this.selectedProductionOrder.set(order);
    this.productionOrderSelected();
  }

  onNextStep() {
    this.nextAssemblyStep();
  }

  onOpenTool() {
    this.openToolDialog();
  }

  onCompleteAssembly() {
    this.saveProducedProduct();
  }

  onPartScanned(event: PartScanEvent) {
    this.findRohteilInstanz(event.event, event.bestandteil, event.teilGuid);
  }

  // Update async values when step changes
  private async updateAsyncValues() {
    this.toolCheckLoading.set(true);
    try {
      const requiredForce = await this.requiredTightenForce();
      this.requiredTightenForceValue.set(requiredForce ? +requiredForce : null);

      const toolRequired = await this.checkIsToolRequired();
      this.isToolRequiredValue.set(toolRequired);

      // Wenn kein Tool erforderlich ist, setze toolResultOk auf true
      if (!toolRequired) {
        this.toolResultOk.set(true);
      }
    } catch (error) {
      console.error('Error updating async values:', error);
      this.requiredTightenForceValue.set(null);
      this.isToolRequiredValue.set(false);
      // Bei Fehlern auch toolResultOk auf true setzen, da kein Tool erforderlich
      this.toolResultOk.set(true);
    } finally {
      this.toolCheckLoading.set(false);
    }
  } // Separate async Methode für Tool-Überprüfung
  private async checkIsToolRequired(): Promise<boolean> {
    const id = this.selectedItem()?.bestandteile[this.currentAssemblyStep()]?.id;
    if (id == null) {
      return false;
    }

    try {
      const submodel = await this.service.getAssemblySubmodel(id);
      if (!submodel) {
        return false;
      }
      const elements = submodel.submodelElements ?? [];
      return elements.some(
        (sme: any) =>
          typeof sme?.idShort === 'string' && sme.idShort.toLowerCase() === 'required_tool'
      );
    } catch (error) {
      console.error('Error checking tool requirement:', error);
      return false;
    }
  }

  // Helper methods for template
  async getRequiredTightenForceAsNumber(): Promise<number | null> {
    const value = await this.requiredTightenForce();
    return value ? +value : null;
  }
}
