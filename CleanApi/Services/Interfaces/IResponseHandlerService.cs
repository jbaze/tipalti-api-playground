using CleanApi.Responses;
using FluentValidation.Results;

namespace CleanApi.Services.Interfaces;

public interface IResponseHandlerService
{
    InvoiceResponse CreateInvoiceSuccessResponse();
    InvoiceResponse HandleValidationError(ValidationResult validationResult, string operationContext);
    InvoiceResponse HandleExceptionError(Exception ex, string commandName, string? contextInfo = null);
}