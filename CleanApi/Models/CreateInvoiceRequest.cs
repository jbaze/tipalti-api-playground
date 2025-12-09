namespace CleanApi.Models;

public class CreateInvoiceRequest
{
    public string PayerName { get; set; } = string.Empty;
    public Invoice? Invoice { get; set; } = new();
}
