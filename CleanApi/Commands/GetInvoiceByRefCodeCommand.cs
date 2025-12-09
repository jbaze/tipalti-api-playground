using CleanApi.Commands.Interfaces;
using CleanApi.Models;
using CleanApi.Responses;
using CleanApi.Services.Interfaces;
using Serilog;

namespace CleanApi.Commands;

public sealed record GetInvoiceByRefCodeCommand(
    string RefCode
) : ICommand<GetInvoiceByRefCodeResponse>;

public sealed class GetInvoiceByRefCodeCommandHandler : ICommandHandler<GetInvoiceByRefCodeCommand, GetInvoiceByRefCodeResponse>
{
    private readonly IInvoiceService _invoiceService;

    public GetInvoiceByRefCodeCommandHandler(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    public async Task<GetInvoiceByRefCodeResponse> Handle(GetInvoiceByRefCodeCommand command, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.RefCode))
                return HandleEmptyRefCodeError();

            var invoice = RetrieveInvoice(command.RefCode);

            if (invoice == null)
                return HandleInvoiceNotFoundError(command.RefCode);

            LogSuccessfulRetrieval(command.RefCode);

            return CreateSuccessResponse(invoice);
        }
        catch (Exception ex)
        {
            return HandleCaughtError(ex, command.RefCode);
        }
    }

    private Invoice? RetrieveInvoice(string refCode)
    {
        return _invoiceService.GetInvoiceByRefCode(refCode);
    }

    private void LogSuccessfulRetrieval(string refCode)
    {
        Log.Information("Retrieved invoice with RefCode: {RefCode}", refCode);
    }

    private GetInvoiceByRefCodeResponse HandleEmptyRefCodeError()
    {
        Log.Warning("Empty RefCode provided");
        return new GetInvoiceByRefCodeResponse
        {
            Success = false,
            Invoice = null,
            ErrorMessage = "RefCode is required"
        };
    }

    private GetInvoiceByRefCodeResponse HandleInvoiceNotFoundError(string refCode)
    {
        Log.Information("Invoice not found for RefCode: {RefCode}", refCode);
        return new GetInvoiceByRefCodeResponse
        {
            Success = false,
            Invoice = null,
            ErrorMessage = $"Invoice with RefCode '{refCode}' not found"
        };
    }

    private GetInvoiceByRefCodeResponse CreateSuccessResponse(Invoice invoice)
    {
        return new GetInvoiceByRefCodeResponse
        {
            Success = true,
            Invoice = invoice,
            ErrorMessage = null
        };
    }

    private GetInvoiceByRefCodeResponse HandleCaughtError(Exception ex, string refCode)
    {
        Log.Error(ex, "Error in GetInvoiceByRefCodeCommand for RefCode: {RefCode}", refCode);
        return new GetInvoiceByRefCodeResponse
        {
            Success = false,
            Invoice = null,
            ErrorMessage = "An unexpected error occurred"
        };
    }
}
