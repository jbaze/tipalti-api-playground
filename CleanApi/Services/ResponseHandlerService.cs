using CleanApi.Responses;
using CleanApi.Services.Interfaces;
using FluentValidation.Results;
using Serilog;

namespace CleanApi.Services;

public sealed class ResponseHandlerService : IResponseHandlerService
{
    public InvoiceResponse CreateInvoiceSuccessResponse()
    {
        return new InvoiceResponse
        {
            Success = true,
            Errors = null,
            Reason = null
        };
    }
    public InvoiceResponse HandleValidationError(ValidationResult validationResult, string operationContext)
    {
        var errors = validationResult.Errors.Select(e => new ValidationError
        {
            Field = e.PropertyName,
            Message = e.ErrorMessage
        }).ToList();

        var firstError = validationResult.Errors.First();
        var reason = $"{firstError.PropertyName}: {firstError.ErrorMessage}";

        Log.Warning("Validation failed for {OperationContext}: {Reason}", operationContext, reason);

        return new InvoiceResponse
        {
            Success = false,
            Errors = errors,
            Reason = reason
        };
    }
    public InvoiceResponse HandleExceptionError(Exception ex, string commandName, string? contextInfo = null)
    {
        if (contextInfo != null)
            Log.Error(ex, "Error in {CommandName} for {ContextInfo}", commandName, contextInfo);
        else
            Log.Error(ex, "Error in {CommandName}", commandName);

        return new InvoiceResponse
        {
            Success = false,
            Errors = new List<ValidationError>
            {
                new() { Field = "General", Message = "An unexpected error occurred processing the request" }
            },
            Reason = "An unexpected error occurred"
        };
    }
}