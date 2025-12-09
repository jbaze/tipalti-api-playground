import { Component, Input, output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-response-display',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './response-display.component.html',
  styleUrls: ['./response-display.component.scss']
})
export class ResponseDisplayComponent {
  @Input() currentResponse: any = null;
  
  // Using output() signal instead of EventEmitter
  copyResponseEvent = output<void>();

  responseCopied = signal(false);

  // Check if response is success
  isResponseSuccess(): boolean {
    if (!this.currentResponse) return false;
    return this.currentResponse.status >= 200 && this.currentResponse.status < 300;
  }

  // Format response JSON for display
  formatResponseJson(): string {
    if (!this.currentResponse) return '';
    return JSON.stringify(this.currentResponse.data, null, 2);
  }

  // Get response size in bytes
  getResponseSize(): number {
    if (!this.currentResponse) return 0;
    return JSON.stringify(this.currentResponse.data).length;
  }

  // Copy response to clipboard
  copyResponse(): void {
    this.copyResponseEvent.emit();
    this.responseCopied.set(true);
    setTimeout(() => this.responseCopied.set(false), 2000);
  }
}