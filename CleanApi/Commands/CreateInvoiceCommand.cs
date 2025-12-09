using CleanApi.Commands.Interfaces;
using CleanApi.Models;
using CleanApi.Responses;
using CleanApi.Services.Interfaces;
using FluentValidation;
using Serilog;

namespace CleanApi.Commands;

public sealed record CreateInvoiceCommand(
    string PayerName,
    Invoice Invoice
) : ICommand<InvoiceResponse>;

public sealed class CreateInvoiceCommandHandler : ICommandHandler<CreateInvoiceCommand, InvoiceResponse>
{
    private readonly IInvoiceService _invoiceService;
    private readonly IValidator<CreateInvoiceRequest> _validator;
    private readonly IResponseHandlerService _responseHandler;

    public CreateInvoiceCommandHandler(
        IInvoiceService invoiceService,
        IValidator<CreateInvoiceRequest> validator,
        IResponseHandlerService responseHandler)
    {
        _invoiceService = invoiceService;
        _validator = validator;
        _responseHandler = responseHandler;
    }

    public async Task<InvoiceResponse> Handle(CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        try
        {
            Log.Information("POST /invoices received for PayerName: {PayerName}, RefCode: {RefCode}", command.PayerName, command.Invoice?.RefCode);

            var request = new CreateInvoiceRequest
            {
                PayerName = command.PayerName,
                Invoice = command.Invoice
            };

            var validationResult = await _validator.ValidateAsync(request, cancellationToken);

            if (!validationResult.IsValid)
                return _responseHandler.HandleValidationError(validationResult, "POST /invoices");

            var existingInvoice = _invoiceService.GetInvoiceByRefCode(command.Invoice!.RefCode);

            if (existingInvoice != null)
                return HandleInvoiceAlreadyExistsError(command.Invoice.RefCode);

            StoreInvoice(command.Invoice);

            Log.Information("Created invoice with RefCode: {RefCode}", command.Invoice.RefCode);

            return _responseHandler.CreateInvoiceSuccessResponse();
        }
        catch (Exception ex)
        {
            return _responseHandler.HandleExceptionError(ex, "CreateInvoiceCommand");
        }
    }

    private void StoreInvoice(Invoice invoice)
    {
        _invoiceService.StoreInvoice(invoice);
    }

    private InvoiceResponse HandleInvoiceAlreadyExistsError(string refCode)
    {
        Log.Warning("Invoice with RefCode {RefCode} already exists", refCode);
        return new InvoiceResponse
        {
            Success = false,
            Errors = new List<ValidationError>
            {
                new() { Field = "Invoice.RefCode", Message = "An invoice with this RefCode already exists" }
            },
            Reason = "An invoice with this RefCode already exists"
        };
    }
}
