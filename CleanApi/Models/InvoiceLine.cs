namespace CleanApi.Models;

public class InvoiceLine
{
    public string Currency { get; set; } = string.Empty;
    public string Amount { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public GLAccount GLAccount { get; set; } = new();
    public string LineType { get; set; } = string.Empty;
    public int Quantity { get; set; }
}
