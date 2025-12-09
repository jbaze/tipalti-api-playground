namespace CleanApi.Responses;

public class InvoiceResponse
{
    public bool Success { get; set; }
    public List<ValidationError>? Errors { get; set; }
    public string? Reason { get; set; }
}

public class ValidationError
{
    public string Field { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}
