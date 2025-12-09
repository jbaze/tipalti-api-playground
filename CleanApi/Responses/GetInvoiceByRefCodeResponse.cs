using CleanApi.Models;

namespace CleanApi.Responses;

public class GetInvoiceByRefCodeResponse
{
    public bool Success { get; set; }
    public Invoice? Invoice { get; set; }
    public string? ErrorMessage { get; set; }
}
