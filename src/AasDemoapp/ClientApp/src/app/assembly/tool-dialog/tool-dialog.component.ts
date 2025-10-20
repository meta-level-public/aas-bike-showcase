import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';

@Component({
  selector: 'app-tool-dialog',
  templateUrl: './tool-dialog.component.html',
  styleUrl: './tool-dialog.component.css',
  imports: [CommonModule, DialogModule, ButtonModule],
})
export class ToolDialogComponent {
  @Input() visible: boolean = false;
  @Input() toolResultOk: boolean = false;
  @Input() requiredToolAasId: string | null = null;
  @Input() requiredTightenForce: number | null = null;
  @Input() allowedTightenForceRange: any = null;
  @Input() configuredRequiredTightenForce: number = 0;
  @Input() currentTightenForce: number = 0;
  @Input() toolInitialized: boolean = false;
  @Input() checkingToolData: boolean = false;

  @Output() initializeTool = new EventEmitter<void>();
  @Output() checkToolData = new EventEmitter<void>();
  @Output() closeDialog = new EventEmitter<boolean>();

  onInitializeTool() {
    this.initializeTool.emit();
  }

  onCheckToolData() {
    this.checkToolData.emit();
  }

  onCloseDialog(success: boolean) {
    this.closeDialog.emit(success);
  }
}
