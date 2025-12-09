/**
 * TypeScript models matching backend C# DTOs
 * These interfaces ensure type safety across the full stack
 *
 * Note: Manually maintained to match backend/Models/*.cs
 * In a production system, consider using OpenAPI code generation
 */

/**
 * Represents a General Ledger account for accounting classification
 */
export interface GLAccount {
  /** The descriptive name of the GL account */
  name: string;

  /** The unique identifier number for this GL account */
  number: string;

  /** The currency code for this GL account (ISO 4217 - 3 letters) */
  currency: string;
}

/**
 * Represents a single line item within an invoice
 */
export interface InvoiceLine {
  /** The currency code for this line item */
  currency: string;

  /** The monetary amount for this line item */
  amount: string;

  /** A detailed description of what this line item represents */
  description: string;

  /** The General Ledger account to which this line item should be posted */
  glAccount: GLAccount;

  /** The type of line item (e.g., "AccountBased", "ItemBased") */
  lineType: string;

  /** The quantity of items for this line */
  quantity: number;
}

/**
 * Represents a complete invoice document
 */
export interface Invoice {
  /** The unique reference code identifying this invoice */
  refCode: string;

  /** The date when the invoice was issued (ISO 8601) */
  invoiceDate: string;

  /** The date by which payment is due (ISO 8601) */
  invoiceDueDate: string;

  /** List of purchase order references */
  purchaseOrders: string[];

  /** The collection of line items that make up this invoice */
  invoiceLines: InvoiceLine[];

  /** A general description of the invoice */
  description: string;

  /** The currency code for the overall invoice */
  currency: string;

  /** The invoice number as assigned by the issuer */
  invoiceNumber: string;

  /** The name of the payer entity responsible for this invoice */
  payerEntityName: string;
}

/**
 * Request model for submitting invoices via PUT /invoices
 */
export interface InvoiceRequest {
  /** The name of the entity paying for these invoices */
  payerName: string;

  /** Collection of invoices to be submitted */
  invoices: Invoice[];
}

/**
 * Request model for creating a single invoice via POST /invoices
 * Follows the Swagger API specification
 */
export interface CreateInvoiceRequest {
  /** The name of the entity paying for this invoice */
  payerName: string;

  /** The invoice data to be created */
  invoice: Invoice;
}


/**
 * Represents a single field-level validation error
 */
export interface ValidationError {
  /** The field path that failed validation */
  field: string;

  /** Human-readable error message */
  message: string;
}

/**
 * Response model for PUT /invoices endpoint
 */
export interface InvoiceResponse {
  /** Indicates whether the submission was successful */
  success: boolean;

  /** Collection of validation errors (null if success) */
  errors?: ValidationError[] | null;

  /** General reason for failure (null if success) */
  reason?: string | null;
}

/**
 * Response model for GET /invoices endpoint
 */
export interface GetInvoicesResponse {
  /** Human-readable error message ("OK" for success) */
  errorMessage: string;

  /** Machine-readable error code ("OK" for success) */
  errorCode: string;

  /** Collection of invoice reference codes */
  invoicesRefCode: string[];
}

/**
 * Example invoice payload for testing
 * Used by the "Load Example" button in the UI
 */
export const EXAMPLE_INVOICE_REQUEST: CreateInvoiceRequest = {
  payerName: "TestName",
  invoice: {
    refCode: "02345-1",
    invoiceDate: "2024-09-25T00:00:00",
    invoiceDueDate: "2024-10-25T00:00:00",
    purchaseOrders: [
      "string"
    ],
    invoiceLines: [
      {
        currency: "USD",
        amount: "100.00",
        description: "Test Invoice Line Description",
        glAccount: {
          name: "General",
          number: "1",
          currency: "USD"
        },
        lineType: "AccountBased",
        quantity: 1
      }
    ],
    description: "Test Invoice Header Description",
    currency: "USD",
    invoiceNumber: "02345-1",
    payerEntityName: "TestEntity"
  }
};
