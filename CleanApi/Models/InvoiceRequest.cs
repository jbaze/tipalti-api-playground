namespace CleanApi.Models;

public class InvoiceRequest
{
    public string PayerName { get; set; } = string.Empty;
    public List<Invoice>? Invoices { get; set; } = new();
}
