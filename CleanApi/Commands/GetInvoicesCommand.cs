using CleanApi.Commands.Interfaces;
using CleanApi.Responses;
using CleanApi.Services.Interfaces;
using Serilog;

namespace CleanApi.Commands;

public sealed record GetInvoicesCommand(
    int? Count
) : ICommand<GetInvoicesResponse>;

public sealed class GetInvoicesCommandHandler : ICommandHandler<GetInvoicesCommand, GetInvoicesResponse>
{
    private readonly IInvoiceService _invoiceService;

    public GetInvoicesCommandHandler(IInvoiceService invoiceService)
    {
        _invoiceService = invoiceService;
    }

    public async Task<GetInvoicesResponse> Handle(GetInvoicesCommand command, CancellationToken cancellationToken)
    {
        try
        {
            var refCodeCount = command.Count ?? 10;

            if (!IsValidCount(refCodeCount))
                return HandleInvalidCountError(refCodeCount);

            var refCodes = GenerateRefCodes(refCodeCount);

            LogSuccessfulGeneration(refCodeCount);

            return CreateSuccessResponse(refCodes);
        }
        catch (Exception ex)
        {
            return HandleCaughtError(ex);
        }
    }

    private bool IsValidCount(int count)
    {
        return count >= 1 && count <= 100;
    }

    private List<string> GenerateRefCodes(int count)
    {
        return _invoiceService.GetAllInvoiceRefCodes().Take(count).ToList();
    }

    private void LogSuccessfulGeneration(int count)
    {
        Log.Information("GET /invoices returned {Count} RefCodes",count);
    }

    private GetInvoicesResponse HandleInvalidCountError(int count)
    {
        Log.Warning("Invalid count parameter: {Count}", count);
        return new GetInvoicesResponse
        {
            ErrorMessage = "Count must be between 1 and 100",
            ErrorCode = "BADREQUEST",
            InvoicesRefCode = new List<string>()
        };
    }

    private GetInvoicesResponse CreateSuccessResponse(List<string> refCodes)
    {
        return new GetInvoicesResponse
        {
            ErrorMessage = "OK",
            ErrorCode = "OK",
            InvoicesRefCode = refCodes
        };
    }

    private GetInvoicesResponse HandleCaughtError(Exception ex)
    {
        Log.Error(ex, "Error in GetInvoicesCommand");
        return new GetInvoicesResponse
        {
            ErrorMessage = "Failed to get Bills",
            ErrorCode = "BADREQUEST",
            InvoicesRefCode = new List<string>()
        };
    }
}
