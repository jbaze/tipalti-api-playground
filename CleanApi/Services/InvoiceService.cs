using CleanApi.Models;
using CleanApi.Services.Interfaces;

namespace CleanApi.Services;

public class InvoiceService : IInvoiceService
{
    private readonly Dictionary<string, Invoice> _invoices = new();

    public void StoreInvoice(Invoice invoice)
    {
        _invoices[invoice.RefCode] = invoice;
    }
    public Invoice? GetInvoiceByRefCode(string refCode)
    {
        return _invoices.TryGetValue(refCode, out var invoice) ? invoice : null;
    }
    public List<string> GetAllInvoiceRefCodes()
    {
        /*
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri("https://api.example.com");

            try
            {
                // GET request to external API endpoint
                var response = await httpClient.GetAsync("/api/invoices");
                response.EnsureSuccessStatusCode();

                // Deserialize the response to list of invoices
                var invoices = await response.Content.ReadFromJsonAsync<List<Invoice>>();

                // Extract and return reference codes
                return invoices?.Select(i => i.RefCode).ToList() ?? new List<string>();
            }
            catch (HttpRequestException ex)
            {
                // Handle API errors (log, throw, or return empty list)
                Console.WriteLine($"Error calling external API: {ex.Message}");
                return new List<string>();
            }
         */
        return _invoices.Keys.ToList();
    }
    public int GetInvoiceCount()
    {
        return _invoices.Count;
    }
    public bool DeleteInvoice(string refCode)
    {
        return _invoices.Remove(refCode);
    }
    public void ClearAllInvoices()
    {
        _invoices.Clear();
    }
}
