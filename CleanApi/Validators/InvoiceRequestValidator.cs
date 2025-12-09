using FluentValidation;
using CleanApi.Models;

namespace CleanApi.Validators;

/// <summary>
/// FluentValidation validator for InvoiceRequest
/// </summary>
public class InvoiceRequestValidator : AbstractValidator<InvoiceRequest>
{
    public InvoiceRequestValidator()
    {
        RuleFor(x => x.PayerName)
            .NotEmpty()
            .WithMessage("PayerName is required");

        RuleFor(x => x.Invoices)
            .NotEmpty()
            .WithMessage("At least one invoice is required");

        RuleForEach(x => x.Invoices)
            .SetValidator(new InvoiceValidator());
    }
}

/// <summary>
/// FluentValidation validator for CreateInvoiceRequest
/// </summary>
public class CreateInvoiceRequestValidator : AbstractValidator<CreateInvoiceRequest>
{
    public CreateInvoiceRequestValidator()
    {
        RuleFor(x => x.PayerName)
            .NotEmpty()
            .WithMessage("PayerName is required");

        RuleFor(x => x.Invoice)
            .NotNull()
            .WithMessage("Invoice is required")
            .SetValidator(new InvoiceValidator());
    }
}

/// <summary>
/// FluentValidation validator for individual Invoice objects
/// </summary>
public class InvoiceValidator : AbstractValidator<Invoice>
{
    private const string RefCodePattern = @"^[a-zA-Z0-9-]+$";

    private static readonly HashSet<string> ValidCurrencies = new()
    {
        "USD", "EUR", "GBP", "CAD", "AUD", "JPY", "CHF", "CNY", "INR", "NZD",
        "SEK", "NOK", "DKK", "SGD", "HKD", "KRW", "MXN", "BRL", "ZAR", "ILS"
    };

    public InvoiceValidator()
    {
        RuleFor(x => x.RefCode)
            .NotEmpty()
            .WithMessage("RefCode is required")
            .Matches(RefCodePattern)
            .WithMessage("RefCode must contain only alphanumeric characters and hyphens");

        RuleFor(x => x.InvoiceNumber)
            .NotEmpty()
            .WithMessage("InvoiceNumber is required");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Currency is required")
            .Length(3)
            .WithMessage("Currency must be a 3-letter ISO code")
            .Must(c => ValidCurrencies.Contains(c.ToUpper()))
            .WithMessage("Currency must be a valid ISO 4217 code");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required");

        RuleFor(x => x.PayerEntityName)
            .NotEmpty()
            .WithMessage("PayerEntityName is required");

        RuleFor(x => x.InvoiceDueDate)
            .GreaterThan(x => x.InvoiceDate)
            .WithMessage("InvoiceDueDate must be after InvoiceDate");

        RuleFor(x => x.InvoiceLines)
            .NotEmpty()
            .WithMessage("At least one invoice line is required");

        RuleForEach(x => x.InvoiceLines)
            .SetValidator(new InvoiceLineValidator());
    }
}

/// <summary>
/// FluentValidation validator for InvoiceLine objects
/// </summary>
public class InvoiceLineValidator : AbstractValidator<InvoiceLine>
{
    private static readonly HashSet<string> ValidCurrencies = new()
    {
        "USD", "EUR", "GBP", "CAD", "AUD", "JPY", "CHF", "CNY", "INR", "NZD",
        "SEK", "NOK", "DKK", "SGD", "HKD", "KRW", "MXN", "BRL", "ZAR", "ILS"
    };

    public InvoiceLineValidator()
    {
        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("Line Currency is required")
            .Length(3)
            .WithMessage("Line Currency must be a 3-letter ISO code")
            .Must(c => ValidCurrencies.Contains(c.ToUpper()))
            .WithMessage("Line Currency must be a valid ISO 4217 code");

        RuleFor(x => x.Amount)
            .NotEmpty()
            .WithMessage("Line Amount is required")
            .Must(BeValidPositiveAmount)
            .WithMessage("Line Amount must be a positive number");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Line Description is required");

        RuleFor(x => x.LineType)
            .NotEmpty()
            .WithMessage("LineType is required");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0");

        RuleFor(x => x.GLAccount)
            .NotNull()
            .WithMessage("GLAccount is required")
            .SetValidator(new GLAccountValidator());
    }

    private bool BeValidPositiveAmount(string amount)
    {
        if (decimal.TryParse(amount, out decimal value))
        {
            return value > 0;
        }
        return false;
    }
}

/// <summary>
/// FluentValidation validator for GLAccount objects
/// </summary>
public class GLAccountValidator : AbstractValidator<GLAccount>
{
    private static readonly HashSet<string> ValidCurrencies = new()
    {
        "USD", "EUR", "GBP", "CAD", "AUD", "JPY", "CHF", "CNY", "INR", "NZD",
        "SEK", "NOK", "DKK", "SGD", "HKD", "KRW", "MXN", "BRL", "ZAR", "ILS"
    };

    public GLAccountValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("GL Account Name is required");

        RuleFor(x => x.Number)
            .NotEmpty()
            .WithMessage("GL Account Number is required");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .WithMessage("GL Account Currency is required")
            .Length(3)
            .WithMessage("GL Account Currency must be a 3-letter ISO code")
            .Must(c => ValidCurrencies.Contains(c.ToUpper()))
            .WithMessage("GL Account Currency must be a valid ISO 4217 code");
    }
}
