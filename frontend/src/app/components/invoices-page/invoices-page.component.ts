import { Component, OnInit, signal, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../services/auth.service';
import { InvoiceApiService } from '../../services/invoice-api.service';
import { ResponseDisplayComponent } from '../response-display/response-display.component';
import { EXAMPLE_INVOICE_REQUEST } from '../../models/invoice.model';

// Monaco Editor types
declare const monaco: any;

/**
 * Tipalti API Playground Page
 *
 * Features:
 * - Two-column card-based layout matching React design
 * - Dark header with API key management
 * - GET and PUT invoice cards with documentation
 * - Sticky response panel with status indicators
 * - Monaco editor for JSON editing
 */
@Component({
  selector: 'app-invoices-page',
  standalone: true,
  imports: [CommonModule, FormsModule, ResponseDisplayComponent],
  templateUrl: './invoices-page.component.html',
  styleUrls: ['./invoices-page.component.scss'],
})
export class InvoicesPageComponent implements OnInit, AfterViewInit {
  @ViewChild('editorContainer', { static: false }) editorContainer!: ElementRef;
  @ViewChild('postEditorContainer', { static: false }) postEditorContainer!: ElementRef;

  private editor: any;
  private postEditor: any;
  private startTime: number = 0;

  // GET section
  getCount: number | null = null;
  getLoading = signal(false);
  getResponse = signal<any>(null);

  // PUT section
  putLoading = signal(false);
  putResponse = signal<any>(null);

  // POST section
  postLoading = signal(false);
  postResponse = signal<any>(null);
  postRequestBodyCopied = signal(false);

  // DELETE section
  deleteLoading = signal(false);
  deleteResponse = signal<any>(null);
  deleteRefCode: string = '';

  // Current response for display panel
  currentResponse = signal<any>(null);

  // API Key management
  showApiKey = signal(false);
  apiKeyCopied = signal(false);
  private apiKey = signal('ey****mock-api-key-token****');

  // Copy states
  requestBodyCopied = signal(false);
  responseCopied = signal(false);

  constructor(
    public authService: AuthService,
    private invoiceApi: InvoiceApiService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Initialize API key from auth service token
    const token = this.authService.token();
    if (token) {
      this.apiKey.set(token);
    }
  }

  ngAfterViewInit(): void {
    this.loadMonacoEditor();
  }

  private loadMonacoEditor(): void {
    if (typeof monaco !== 'undefined') {
      this.initializeEditor();
      return;
    }

    const script = document.createElement('script');
    script.src = 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.45.0/min/vs/loader.min.js';
    script.onload = () => {
      (window as any).require.config({
        paths: { vs: 'https://cdnjs.cloudflare.com/ajax/libs/monaco-editor/0.45.0/min/vs' }
      });
      (window as any).require(['vs/editor/editor.main'], () => {
        this.initializeEditor();
      });
    };
    document.body.appendChild(script);
  }

  private initializeEditor(): void {
    // Initialize PUT editor
    if (this.editorContainer) {
      this.editor = monaco.editor.create(this.editorContainer.nativeElement, {
        value: JSON.stringify(EXAMPLE_INVOICE_REQUEST, null, 2),
        language: 'json',
        theme: 'vs-dark',
        automaticLayout: true,
        minimap: { enabled: false },
        fontSize: 13,
        lineNumbers: 'on',
        scrollBeyondLastLine: false,
        tabSize: 2,
        formatOnPaste: true,
        formatOnType: true
      });
    }

    // Initialize POST editor
    if (this.postEditorContainer) {
      this.postEditor = monaco.editor.create(this.postEditorContainer.nativeElement, {
        value: JSON.stringify(EXAMPLE_INVOICE_REQUEST, null, 2),
        language: 'json',
        theme: 'vs-dark',
        automaticLayout: true,
        minimap: { enabled: false },
        fontSize: 13,
        lineNumbers: 'on',
        scrollBeyondLastLine: false,
        tabSize: 2,
        formatOnPaste: true,
        formatOnType: true
      });
    }
  }

  // API Key methods
  apiKeyDisplay(): string {
    return this.apiKey();
  }

  onApiKeyChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.apiKey.set(input.value);
  }

  toggleApiKeyVisibility(): void {
    this.showApiKey.set(!this.showApiKey());
  }

  copyApiKey(): void {
    navigator.clipboard.writeText(this.apiKey()).then(() => {
      this.apiKeyCopied.set(true);
      setTimeout(() => this.apiKeyCopied.set(false), 2000);
    });
  }

  // Execute GET request
  executeGet(): void {
    this.getLoading.set(true);
    this.startTime = Date.now();

    this.invoiceApi.getInvoices(this.getCount ?? undefined)
      .subscribe({
        next: (response) => {
          const responseTime = Date.now() - this.startTime;
          const formattedResponse = {
            status: 200,
            statusText: 'OK',
            data: response,
            time: responseTime
          };
          this.getResponse.set(formattedResponse);
          this.currentResponse.set(formattedResponse);
          this.getLoading.set(false);
        },
        error: (error) => {
          const responseTime = Date.now() - this.startTime;
          const errorResponse = {
            status: error.status,
            statusText: error.statusText,
            data: { error: error.message },
            time: responseTime
          };
          this.getResponse.set(errorResponse);
          this.currentResponse.set(errorResponse);
          this.getLoading.set(false);
        }
      });
  }

  // Execute PUT request
  executePut(): void {
    try {
      const request = JSON.parse(this.editor.getValue());
      
      // Validate that PUT request has at least payerName
      if (!request.payerName) {
        const errorResponse = {
          status: 400,
          statusText: 'Bad Request',
          data: { success: false, reason: 'PUT request must contain "payerName"' },
          time: 50
        };
        this.putResponse.set(errorResponse);
        this.currentResponse.set(errorResponse);
        return;
      }

      // Determine the correct format to send to the API
      let apiRequest: any;
      
      if (request.invoice) {
        // Single invoice format: convert to array
        apiRequest = {
          payerName: request.payerName,
          invoices: [request.invoice]
        };
      } else {
        // No invoices provided
        const errorResponse = {
          status: 400,
          statusText: 'Bad Request',
          data: { 
            success: false, 
            reason: 'PUT request must contain either "invoice" object or "invoices" array' 
          },
          time: 50
        };
        this.putResponse.set(errorResponse);
        this.currentResponse.set(errorResponse);
        return;
      }

      // Validate that we have at least one invoice
      if (!apiRequest.invoices || apiRequest.invoices.length === 0) {
        const errorResponse = {
          status: 400,
          statusText: 'Bad Request',
          data: { success: false, reason: 'PUT request must contain at least one invoice' },
          time: 50
        };
        this.putResponse.set(errorResponse);
        this.currentResponse.set(errorResponse);
        return;
      }

      this.putLoading.set(true);
      this.startTime = Date.now();
      
      // Send the properly formatted request
      this.invoiceApi.putInvoices(apiRequest).subscribe({
        next: (response) => {
          const responseTime = Date.now() - this.startTime;
          const isSuccess = response.success === true;
          const formattedResponse = {
            status: isSuccess ? 200 : 400,
            statusText: isSuccess ? 'OK' : 'Bad Request',
            data: response,
            time: responseTime
          };
          this.putResponse.set(formattedResponse);
          this.currentResponse.set(formattedResponse);
          this.putLoading.set(false);
        },
        error: (error) => {
          const responseTime = Date.now() - this.startTime;
          const errorResponse = {
            status: error.status || 500,
            statusText: error.statusText || 'Error',
            data: { success: false, reason: error.message },
            time: responseTime
          };
          this.putResponse.set(errorResponse);
          this.currentResponse.set(errorResponse);
          this.putLoading.set(false);
        }
      });
    } catch (error) {
      const errorResponse = {
        status: 400,
        statusText: 'Bad Request',
        data: { success: false, reason: 'Invalid JSON format in request body' },
        time: 50
      };
      this.putResponse.set(errorResponse);
      this.currentResponse.set(errorResponse);
    }
  }

  // Format JSON in editor
  formatJson(): void {
    try {
      const value = this.editor.getValue();
      const parsed = JSON.parse(value);
      this.editor.setValue(JSON.stringify(parsed, null, 2));
    } catch (error) {
      alert('Invalid JSON - cannot format');
    }
  }

  // Copy request body
  copyRequestBody(): void {
    if (this.editor) {
      navigator.clipboard.writeText(this.editor.getValue()).then(() => {
        this.requestBodyCopied.set(true);
        setTimeout(() => this.requestBodyCopied.set(false), 2000);
      });
    }
  }

  // Copy response
  copyResponse(): void {
    const response = this.currentResponse();
    if (response) {
      navigator.clipboard.writeText(JSON.stringify(response.data, null, 2)).then(() => {
        this.responseCopied.set(true);
        setTimeout(() => this.responseCopied.set(false), 2000);
      });
    }
  }

  // Execute POST request (Create Invoice)
  executePost(): void {
    try {
      const request = JSON.parse(this.postEditor.getValue());
      
      // Validate that POST request has single invoice (not array)
      if (request.invoices && Array.isArray(request.invoices)) {
        // If user accidentally used the array format, convert to single invoice format
        if (request.invoices.length > 0) {
          // Convert array format to single invoice format
          const convertedRequest = {
            payerName: request.payerName,
            invoice: request.invoices[0]
          };
          this.sendCreateInvoiceRequest(convertedRequest);
        } else {
          const errorResponse = {
            status: 400,
            statusText: 'Bad Request',
            data: { success: false, reason: 'POST request must contain at least one invoice' },
            time: 50
          };
          this.postResponse.set(errorResponse);
          this.currentResponse.set(errorResponse);
          this.postLoading.set(false);
        }
      } else if (request.invoice) {
        // Already in correct single invoice format
        this.sendCreateInvoiceRequest(request);
      } else {
        const errorResponse = {
          status: 400,
          statusText: 'Bad Request',
          data: { success: false, reason: 'POST request must contain "invoice" object' },
          time: 50
        };
        this.postResponse.set(errorResponse);
        this.currentResponse.set(errorResponse);
        this.postLoading.set(false);
      }
    } catch (error: any) {
      const errorResponse = {
        status: 400,
        statusText: 'Bad Request',
        data: { success: false, reason: error.message || 'Invalid JSON format in request body' },
        time: 50
      };
      this.postResponse.set(errorResponse);
      this.currentResponse.set(errorResponse);
      this.postLoading.set(false);
    }
  }

  // Helper method to send create invoice request
  private sendCreateInvoiceRequest(request: any): void {
    this.postLoading.set(true);
    this.startTime = Date.now();
      
    this.invoiceApi.createInvoice(request).subscribe({
      next: (response) => {
        const responseTime = Date.now() - this.startTime;
        const formattedResponse = {
          status: response.success ? 201 : 400,
          statusText: response.success ? 'Created' : 'Bad Request',
          data: response,
          time: responseTime
        };
        this.postResponse.set(formattedResponse);
        this.currentResponse.set(formattedResponse);
        this.postLoading.set(false);
      },
      error: (error) => {
        const responseTime = Date.now() - this.startTime;
        const errorResponse = {
          status: error.status || 500,
          statusText: error.statusText || 'Error',
          data: { success: false, reason: error.message },
          time: responseTime
        };
        this.postResponse.set(errorResponse);
        this.currentResponse.set(errorResponse);
        this.postLoading.set(false);
      }
    });
  }

  // Execute DELETE request
  executeDelete(): void {
    if (!this.deleteRefCode || this.deleteRefCode.trim() === '') {
      const errorResponse = {
        status: 400,
        statusText: 'Bad Request',
        data: { success: false, reason: 'RefCode is required' },
        time: 50
      };
      this.deleteResponse.set(errorResponse);
      this.currentResponse.set(errorResponse);
      return;
    }

    this.deleteLoading.set(true);
    this.startTime = Date.now();

    this.invoiceApi.deleteInvoice(this.deleteRefCode.trim()).subscribe({
      next: (response) => {
        const responseTime = Date.now() - this.startTime;
        const formattedResponse = {
          status: response.success ? 200 : 400,
          statusText: response.success ? 'OK' : 'Bad Request',
          data: response,
          time: responseTime
        };
        this.deleteResponse.set(formattedResponse);
        this.currentResponse.set(formattedResponse);
        this.deleteLoading.set(false);
      },
      error: (error) => {
        const responseTime = Date.now() - this.startTime;
        const errorResponse = {
          status: error.status || 500,
          statusText: error.statusText || 'Error',
          data: { success: false, reason: error.message },
          time: responseTime
        };
        this.deleteResponse.set(errorResponse);
        this.currentResponse.set(errorResponse);
        this.deleteLoading.set(false);
      }
    });
  }

  // Copy POST request body
  copyPostRequestBody(): void {
    if (this.postEditor) {
      navigator.clipboard.writeText(this.postEditor.getValue()).then(() => {
        this.postRequestBodyCopied.set(true);
        setTimeout(() => this.postRequestBodyCopied.set(false), 2000);
      });
    }
  }

  // Format POST JSON in editor
  formatPostJson(): void {
    try {
      const value = this.postEditor.getValue();
      const parsed = JSON.parse(value);
      this.postEditor.setValue(JSON.stringify(parsed, null, 2));
    } catch (error) {
      alert('Invalid JSON - cannot format');
    }
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}