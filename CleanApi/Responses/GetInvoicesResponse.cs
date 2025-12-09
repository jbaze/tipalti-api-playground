namespace CleanApi.Responses;

public class GetInvoicesResponse
{
    public string ErrorMessage { get; set; } = string.Empty;
    public string ErrorCode { get; set; } = string.Empty;
    public List<string> InvoicesRefCode { get; set; } = new();
}
