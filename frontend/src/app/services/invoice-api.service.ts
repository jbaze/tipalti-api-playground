import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { InvoiceRequest, InvoiceResponse, GetInvoicesResponse } from '../models/invoice.model';
import { environment } from '../../environments/environment';

/**
 * Response model for POST /invoices endpoint (create)
 */
export interface CreateInvoiceResponse {
  success: boolean;
  invoiceId?: string;
  refCode?: string;
  message?: string;
  reason?: string | null;
}

/**
 * Response model for DELETE /invoices endpoint
 */
export interface DeleteInvoiceResponse {
  success: boolean;
  message?: string;
  deletedRefCode?: string;
  reason?: string | null;
}

/**
 * Service for communicating with the Invoice API backend
 *
 * Purpose:
 * - Provides typed HTTP methods for all invoice endpoints
 * - Centralizes API URL configuration
 * - Uses Angular HttpClient with RxJS Observables
 * - Works with HTTP interceptor for authentication headers
 *
 * Endpoints:
 * - GET /invoices - Retrieve invoice reference codes
 * - POST /invoices - Create a new invoice
 * - PUT /invoices - Submit invoices for validation
 * - DELETE /invoices/:refCode - Delete an invoice
 * - GET /health - Health check
 */
@Injectable({
  providedIn: 'root'
})
export class InvoiceApiService {
  /**
   * Base URL for the backend API
   * In production, this would come from environment configuration
   */
  private readonly API_URL = environment.API_URL;

  constructor(private http: HttpClient) { }

  /**
   * GET /invoices - Retrieve invoice reference codes
   *
   * @param count - Optional number of RefCodes to return (1-100, default: 3)
   * @returns Observable of GetInvoicesResponse
   */
  getInvoices(count?: number): Observable<GetInvoicesResponse> {
    let params = new HttpParams();

    if (count !== undefined) {
      params = params.set('count', count.toString());
    }
    return this.http.get<GetInvoicesResponse>(`${this.API_URL}/invoices`, { params });
  }

  /**
   * POST /invoices - Create a new invoice
   *
   * @param request - Invoice data in either Swagger format (with 'invoice' object) or old format (with 'invoices' array)
   * @returns Observable of CreateInvoiceResponse
   */
  createInvoice(request: any): Observable<CreateInvoiceResponse> {
    // Determine the request format and transform if needed
    let swaggerRequest: any;
    
    if (request.invoice) {
      // Already in Swagger format
      swaggerRequest = {
        payerName: request.payerName || request.PayerName,
        invoice: {
          refCode: request.invoice.refCode || request.invoice.RefCode,
          invoiceDate: request.invoice.invoiceDate || request.invoice.InvoiceDate || new Date().toISOString(),
          invoiceDueDate: request.invoice.invoiceDueDate || request.invoice.InvoiceDueDate || new Date(Date.now() + 30 * 24 * 60 * 60 * 1000).toISOString(),
          purchaseOrders: request.invoice.purchaseOrders || request.invoice.PurchaseOrders || [],
          invoiceLines: (request.invoice.invoiceLines || request.invoice.InvoiceLines || []).map((line: any) => ({
            currency: line.currency || line.Currency || 'USD',
            amount: line.amount || line.Amount || '0.00',
            description: line.description || line.Description || '',
            glAccount: line.glAccount || line.GlAccount || {
              name: 'General',
              number: '1',
              currency: 'USD'
            },
            lineType: line.lineType || line.LineType || 'AccountBased',
            quantity: line.quantity || line.Quantity || 1
          })),
          description: request.invoice.description || request.invoice.Description || '',
          currency: request.invoice.currency || request.invoice.Currency || 'USD',
          invoiceNumber: request.invoice.invoiceNumber || request.invoice.InvoiceNumber || request.invoice.refCode || request.invoice.RefCode,
          payerEntityName: request.invoice.payerEntityName || request.invoice.PayerEntityName || request.payerName || request.PayerName
        }
      };
    } else {
      // Invalid format
      return new Observable(observer => {
        observer.next({
          success: false,
          reason: 'Invalid request format. Must contain either "invoice" object or "invoices" array'
        });
        observer.complete();
      });
    }
    
    return this.http.post<CreateInvoiceResponse>(`${this.API_URL}/invoices`, swaggerRequest);
  }

  /**
   * PUT /invoices - Submit invoices for validation
   *
   * @param request - InvoiceRequest containing payer info and invoice array
   * @returns Observable of InvoiceResponse (success/failure with field errors)
   */
  putInvoices(request: InvoiceRequest): Observable<InvoiceResponse> {
    return this.http.put<InvoiceResponse>(`${this.API_URL}/Invoices`, request);
  }

  /**
   * DELETE /invoices/:refCode - Delete an invoice
   *
   * @param refCode - The reference code of the invoice to delete
   * @returns Observable of DeleteInvoiceResponse
   */
  deleteInvoice(refCode: string): Observable<DeleteInvoiceResponse> {
    return this.http.delete<DeleteInvoiceResponse>(`${this.API_URL}/invoices/${encodeURIComponent(refCode)}`);
  }

  /**
   * GET /health - Health check endpoint
   *
   * @returns Observable of health status
   */
  getHealth(): Observable<any> {
    return this.http.get(`${this.API_URL}/health`);
  }
}