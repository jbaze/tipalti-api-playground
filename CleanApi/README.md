# CleanApi - Clean Architecture Implementation

A clean architecture implementation of the Tipalti Invoice API with MediatR CQRS pattern following SOLID principles and DRY principle.

## Architecture Overview
CleanApi/
├── Controllers/           # HTTP layer - handles requests/responses
│   ├── ApiControllerBase.cs   # Base controller with Mediator access
│   ├── HealthController.cs
│   └── InvoicesController.cs
│
├── Commands/              # Business logic layer - MediatR command pattern
│   ├── Interfaces/
│   │   ├── ICommand.cs        # Base command interfaces (extends MediatR)
│   │   └── ICommandHandler.cs # Handler interfaces
│   ├── GetInvoicesCommand.cs
│   ├── GetInvoiceByRefCodeCommand.cs
│   ├── CreateInvoiceCommand.cs
│   ├── UpdateInvoiceCommand.cs
│   └── DeleteInvoiceCommand.cs
│
├── Services/              # Service layer
│   ├── Interfaces/
│   │   ├── IInvoiceService.cs        # Data access abstraction
│   │   └── IResponseHandlerService.cs # Response handling abstraction
│   ├── InvoiceService.cs            # Data storage implementation
│   └── ResponseHandlerService.cs    # DRY response handling implementation
│
├── Models/                # Domain entities
│   ├── Invoice.cs
│   ├── InvoiceRequest.cs
│   ├── CreateInvoiceRequest.cs
│   ├── InvoiceLine.cs
│   └── GLAccount.cs
│
├── Responses/             # API response DTOs
│   ├── HealthResponse.cs
│   ├── GetInvoicesResponse.cs
│   ├── GetInvoiceByRefCodeResponse.cs
│   └── InvoiceResponse.cs
│
├── Validators/            # FluentValidation validators
│   └── InvoiceRequestValidator.cs
│
└── Program.cs             # Application entry point & DI configuration
## MediatR CQRS Pattern

Commands are structured as **sealed records** with separate **handlers**:
// Command - immutable record with request data
public sealed record CreateInvoiceCommand(
    string PayerName,
    Invoice Invoice
) : ICommand<InvoiceResponse>;

// Handler - processes the command with injected services
public sealed class CreateInvoiceCommandHandler : ICommandHandler<CreateInvoiceCommand, InvoiceResponse>
{
    private readonly IInvoiceService _invoiceService;
    private readonly IValidator<CreateInvoiceRequest> _validator;
    private readonly IResponseHandlerService _responseHandler;

    public async Task<InvoiceResponse> Handle(CreateInvoiceCommand command, CancellationToken cancellationToken)
    {
        // Validation using shared service
        var validationResult = await _validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
            return _responseHandler.HandleValidationError(validationResult, "POST /invoices");

        // Business logic
        _invoiceService.StoreInvoice(command.Invoice);

        // Success response using shared service
        return _responseHandler.CreateInvoiceSuccessResponse();
    }
}
Controllers use `Mediator.Send()` to dispatch commands:
public class InvoicesController : ApiControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request)
    {
        return await Mediator.Send(new CreateInvoiceCommand(request.PayerName, request.Invoice));
    }
}
## SOLID Principles Applied

### Single Responsibility Principle (SRP)
- **Controllers**: Only handle HTTP concerns (routing, status codes)
- **Commands**: Records hold request data
- **Handlers**: Process business logic for one operation each, with dedicated private methods for specific concerns
- **Services**: 
  - `IInvoiceService` - Only handles data storage operations
  - `IResponseHandlerService` - Only handles response creation and error handling
- **Validators**: Only handle validation rules

### Open/Closed Principle (OCP)
- New commands/handlers can be added without modifying existing ones
- MediatR automatically discovers and registers handlers
- New response handling methods can be added to `IResponseHandlerService` without changing existing commands

### Liskov Substitution Principle (LSP)
- All commands implement `ICommand<TResponse>`
- All handlers implement `ICommandHandler<TCommand, TResponse>`
- Service implementations can be substituted without breaking functionality

### Interface Segregation Principle (ISP)
- `ICommand` and `ICommandHandler` interfaces are minimal
- `IInvoiceService` defines only the methods needed for data access
- `IResponseHandlerService` defines only shared response handling methods

### Dependency Inversion Principle (DIP)
- Controllers depend on `ISender` (MediatR abstraction)
- Handlers depend on service interfaces, not concrete implementations
- All dependencies are injected via constructor injection

## DRY Principle Implementation

### ResponseHandlerService
To eliminate code duplication across commands, we've implemented a `ResponseHandlerService`:
// Shared success response creation (was duplicated in Create, Update, Delete)
return _responseHandler.CreateInvoiceSuccessResponse();

// Shared validation error handling (was duplicated in Create, Update)
return _responseHandler.HandleValidationError(validationResult, "POST /invoices");

// Shared exception handling (was duplicated in Create, Update, Delete)
return _responseHandler.HandleExceptionError(ex, "CreateInvoiceCommand");
**What's Shared:**
- `CreateInvoiceSuccessResponse()` - Common success response for InvoiceResponse
- `HandleValidationError()` - FluentValidation error processing
- `HandleExceptionError()` - Consistent exception logging and error responses

**What Stays Command-Specific:**
- Business validation (e.g., `HandleInvoiceAlreadyExistsError`)
- Command-specific error scenarios
- Different response types (GetInvoicesResponse, GetInvoiceByRefCodeResponse)

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Health check |
| GET | `/invoices` | Get invoice reference codes |
| GET | `/invoices/{refCode}` | Get invoice by reference code |
| POST | `/invoices` | Create a new invoice |
| PUT | `/invoices` | Update invoices (upsert) |
| DELETE | `/invoices/{refCode}` | Delete an invoice |

## Running the Application

### Development Modecd CleanApi
dotnet restore
dotnet run
The API will be available at:
- HTTP: http://localhost:5197
- Swagger UI: http://localhost:5197

### Docker - 'docker-compose up -d --build'
The API will be available at http://localhost:5001

## Request/Response Examples

### POST /invoices (Create){
  "payerName": "TestCompany",
  "invoice": {
    "refCode": "INV-2024-001",
    "invoiceDate": "2024-01-01T00:00:00",
    "invoiceDueDate": "2024-02-01T00:00:00",
    "purchaseOrders": [],
    "invoiceLines": [{
      "currency": "USD",
      "amount": "100.00",
      "description": "Service",
      "glAccount": {
        "name": "General",
        "number": "1",
        "currency": "USD"
      },
      "lineType": "AccountBased",
      "quantity": 1
    }],
    "description": "Invoice Description",
    "currency": "USD",
    "invoiceNumber": "INV-2024-001",
    "payerEntityName": "TestEntity"
  }
}
### GET /invoices Response{
  "errorMessage": "OK",
  "errorCode": "OK",
  "invoicesRefCode": ["INV-2024-001", "INV-2024-002"]
}
### Error Response Example{
  "success": false,
  "errors": [
    {
      "field": "Invoice.RefCode",
      "message": "RefCode is required"
    }
  ],
  "reason": "Invoice.RefCode: RefCode is required"
}
### DELETE /invoices/{refCode}curl -X DELETE http://localhost:5197/invoices/INV-2024-001
## Key Features

| Feature | Implementation |
|---------|----------------|
| Command Pattern | MediatR with sealed record commands |
| Handler Pattern | Separate handler classes with `Handle()` method |
| DI Registration | Automatic via `AddMediatR()` assembly scanning |
| Validation | FluentValidation with comprehensive rules |
| Base Controller | `ApiControllerBase` with `Mediator` property |
| Response Handling | Shared `ResponseHandlerService` for DRY compliance |
| Error Handling | Consistent error responses across all endpoints |
| Logging | Structured logging with Serilog |

## Architecture Benefits

### Clean Architecture Layers
1. **Controllers** (HTTP/API Layer) - Handle HTTP concerns only
2. **Commands/Handlers** (Business Logic Layer) - Process business rules
3. **Services** (Data/Infrastructure Layer) - Handle data access and cross-cutting concerns

### SOLID + DRY Compliance
- **Single Responsibility**: Each class has one reason to change
- **DRY**: Common response handling logic is centralized
- **Dependency Injection**: All dependencies are injected, making testing easier
- **Separation of Concerns**: HTTP, business logic, and data access are clearly separated

## Testing
# Health check
curl http://localhost:5197/health

# Get invoices
curl http://localhost:5197/invoices?count=5

# Get specific invoice
curl http://localhost:5197/invoices/INV-2024-001

# Create invoice
curl -X POST http://localhost:5197/invoices \
  -H "Content-Type: application/json" \
  -d '{"payerName":"TestCompany","invoice":{"refCode":"INV-2024-001",...}}'

# Delete invoice
curl -X DELETE http://localhost:5197/invoices/INV-2024-001
## Technology Stack

- **.NET 8** - Target framework
- **MediatR** - CQRS pattern implementation
- **FluentValidation** - Input validation
- **Serilog** - Structured logging
- **Swagger/OpenAPI** - API documentation
- **ASP.NET Core** - Web API framework
