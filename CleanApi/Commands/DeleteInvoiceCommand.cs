using CleanApi.Commands.Interfaces;
using CleanApi.Responses;
using CleanApi.Services.Interfaces;
using Serilog;

namespace CleanApi.Commands;

/// <summary>
/// Command for deleting an invoice by reference code
/// </summary>
public sealed record DeleteInvoiceCommand(
    string RefCode
) : ICommand<InvoiceResponse>;

/// <summary>
/// Handler for DeleteInvoiceCommand
/// </summary>
public sealed class DeleteInvoiceCommandHandler : ICommandHandler<DeleteInvoiceCommand, InvoiceResponse>
{
    private readonly IInvoiceService _invoiceService;
    private readonly IResponseHandlerService _responseHandler;

    public DeleteInvoiceCommandHandler(IInvoiceService invoiceService, IResponseHandlerService responseHandler)
    {
        _invoiceService = invoiceService;
        _responseHandler = responseHandler;
    }

    public async Task<InvoiceResponse> Handle(DeleteInvoiceCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.RefCode))
                return HandleEmptyRefCodeError();

            var existingInvoice = _invoiceService.GetInvoiceByRefCode(command.RefCode);

            if (existingInvoice == null)
                return HandleInvoiceNotFoundError(command.RefCode);

            var deleted = DeleteInvoice(command.RefCode);

            if (deleted)
            {
                Log.Information("Deleted invoice with RefCode: {RefCode}", command.RefCode);
                return _responseHandler.CreateInvoiceSuccessResponse();
            }

            return HandleDeletionError(command.RefCode);
        }
        catch (Exception ex)
        {
            return _responseHandler.HandleExceptionError(ex, "DeleteInvoiceCommand", $"RefCode: {command.RefCode}");
        }
    }

    private bool DeleteInvoice(string refCode)
    {
        return _invoiceService.DeleteInvoice(refCode);
    }
    private InvoiceResponse HandleEmptyRefCodeError()
    {
        Log.Warning("Empty RefCode provided for delete");
        return new InvoiceResponse
        {
            Success = false,
            Errors = new List<ValidationError>
            {
                new() { Field = "RefCode", Message = "RefCode is required" }
            },
            Reason = "RefCode is required"
        };
    }

    private InvoiceResponse HandleInvoiceNotFoundError(string refCode)
    {
        Log.Warning("Invoice not found for deletion: {RefCode}", refCode);
        return new InvoiceResponse
        {
            Success = false,
            Errors = new List<ValidationError>
            {
                new() { Field = "RefCode", Message = $"Invoice with RefCode '{refCode}' not found" }
            },
            Reason = $"Invoice with RefCode '{refCode}' not found"
        };
    }

    private InvoiceResponse HandleDeletionError(string refCode)
    {
        Log.Error("Failed to delete invoice with RefCode: {RefCode}", refCode);
        return new InvoiceResponse
        {
            Success = false,
            Errors = new List<ValidationError>
            {
                new() { Field = "General", Message = "Failed to delete the invoice" }
            },
            Reason = "Failed to delete the invoice"
        };
    }
}
