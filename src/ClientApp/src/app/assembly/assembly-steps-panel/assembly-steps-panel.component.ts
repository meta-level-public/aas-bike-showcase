import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ButtonModule } from 'primeng/button';
import { InputTextModule } from 'primeng/inputtext';
import { ConfiguredProduct } from '../../model/configured-product';
import { ProductionOrder } from '../../model/production-order';

export interface PartScanEvent {
  event: Event;
  bestandteil: any;
  teilGuid: string;
}

@Component({
  selector: 'app-assembly-steps-panel',
  templateUrl: './assembly-steps-panel.component.html',
  styleUrl: './assembly-steps-panel.component.css',
  imports: [CommonModule, FormsModule, ButtonModule, InputTextModule],
})
export class AssemblyStepsPanelComponent {
  @Input() selectedOrder: ProductionOrder | undefined;
  @Input() selectedItem: ConfiguredProduct | undefined;
  @Input() currentStep: number = 0;
  @Input() totalSteps: number = 0;
  @Input() hasBestandteile: boolean = false;
  @Input() assemblyComplete: boolean = false;
  @Input() toolResultOk: boolean = false;
  @Input() parts: any[] = [];
  @Input() teilStatusMap: Map<number, boolean> = new Map();
  @Input() isToolRequiredForCurrentStep: boolean = false;
  @Input() toolCheckLoading: boolean = false;
  @Input() saveLoading: boolean = false;

  @Output() nextStep = new EventEmitter<void>();
  @Output() openTool = new EventEmitter<void>();
  @Output() completeAssembly = new EventEmitter<void>();
  @Output() partScanned = new EventEmitter<PartScanEvent>();

  onNextStep() {
    this.nextStep.emit();
  }

  onOpenTool() {
    this.openTool.emit();
  }

  onCompleteAssembly() {
    this.completeAssembly.emit();
  }

  onPartScan(event: Event, bestandteil: any, teilGuid: string) {
    this.partScanned.emit({ event, bestandteil, teilGuid });
  }

  getCurrentBestandteil() {
    if (
      !this.selectedItem ||
      !this.selectedItem.bestandteile ||
      this.currentStep >= this.selectedItem.bestandteile.length
    ) {
      return null;
    }
    return this.selectedItem.bestandteile[this.currentStep];
  }

  getParts(bestandteil: any): any[] {
    return this.parts;
  }

  isTeilOk(bestandteil: any): boolean {
    if (!bestandteil || !bestandteil.id) return false;
    return this.teilStatusMap.get(bestandteil.id) || false;
  }

  isToolRequired(): boolean {
    return this.isToolRequiredForCurrentStep;
  }

  hasNextStep(): boolean {
    return this.currentStep < this.totalSteps - 1;
  }
}
