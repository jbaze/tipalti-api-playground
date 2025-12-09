using CleanApi.Commands.Interfaces;
using CleanApi.Models;
using CleanApi.Responses;
using CleanApi.Services.Interfaces;
using FluentValidation;
using Serilog;

namespace CleanApi.Commands;

public sealed record UpdateInvoiceCommand(
    string PayerName,
    List<Invoice> Invoices
) : ICommand<InvoiceResponse>;

public sealed class UpdateInvoiceCommandHandler : ICommandHandler<UpdateInvoiceCommand, InvoiceResponse>
{
    private readonly IInvoiceService _invoiceService;
    private readonly IValidator<InvoiceRequest> _validator;
    private readonly IResponseHandlerService _responseHandler;

    public UpdateInvoiceCommandHandler(
        IInvoiceService invoiceService,
        IValidator<InvoiceRequest> validator,
        IResponseHandlerService responseHandler)
    {
        _invoiceService = invoiceService;
        _validator = validator;
        _responseHandler = responseHandler;
    }

    public async Task<InvoiceResponse> Handle(UpdateInvoiceCommand command, CancellationToken cancellationToken)
    {
        try
        {
            Log.Information("PUT /invoices received for PayerName: {PayerName} with {Count} invoice(s)",
                command.PayerName, command.Invoices?.Count ?? 0);

            var request = new InvoiceRequest
            {
                PayerName = command.PayerName,
                Invoices = command.Invoices
            };

            var validationResult = await _validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                return _responseHandler.HandleValidationError(validationResult, "PUT /invoices");

            ProcessInvoiceUpdates(command.Invoices);

            Log.Information("PUT /invoices succeeded for {Count} invoice(s)", command.Invoices != null ? command.Invoices.Count : 0);

            return _responseHandler.CreateInvoiceSuccessResponse();
        }
        catch (Exception ex)
        {
            return _responseHandler.HandleExceptionError(ex, "UpdateInvoiceCommand");
        }
    }

    private void ProcessInvoiceUpdates(List<Invoice>? invoices)
    {
        if (invoices != null && invoices.Any())
        {
            foreach (var invoice in invoices)
            {
                _invoiceService.StoreInvoice(invoice);
                Log.Information("Stored invoice with RefCode: {RefCode}", invoice.RefCode);
            }
        }
    }
}
