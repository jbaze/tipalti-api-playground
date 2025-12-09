using CleanApi.Models;

namespace CleanApi.Services.Interfaces;

public interface IInvoiceService
{
    void StoreInvoice(Invoice invoice);
    Invoice? GetInvoiceByRefCode(string refCode);
    List<string> GetAllInvoiceRefCodes();
    int GetInvoiceCount();
    bool DeleteInvoice(string refCode);
    void ClearAllInvoices();
}
