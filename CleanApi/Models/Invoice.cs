namespace CleanApi.Models;

public class Invoice
{
    public string RefCode { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public DateTime InvoiceDueDate { get; set; }
    public List<string> PurchaseOrders { get; set; } = new();
    public List<InvoiceLine> InvoiceLines { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public string PayerEntityName { get; set; } = string.Empty;
}
