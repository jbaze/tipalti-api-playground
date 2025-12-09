# Tipalti Invoice API Playground

Full-stack invoice API with .NET 8 Clean Architecture backend and Angular 18 frontend.

## Quick Start

### Docker (Recommended)
```bash
docker-compose up -d --build
```
- Frontend: http://localhost:4200
- Backend API: http://localhost:5001
- Swagger UI: http://localhost:5001

### Manual Setup

**Backend:**
```bash
cd CleanApi
dotnet restore && dotnet run
```

**Frontend:**
```bash
cd frontend
npm install && npm start
```

## Project Structure

```
Tipalti-test/
├── CleanApi/          # .NET 8 API (MediatR, FluentValidation)
└── frontend/          # Angular 18 (Standalone Components)
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Health check |
| GET | `/invoices` | Get invoice RefCodes |
| GET | `/invoices/{refCode}` | Get invoice by RefCode |
| POST | `/invoices` | Create invoice |
| PUT | `/invoices` | Update invoices |
| DELETE | `/invoices/{refCode}` | Delete invoice |

## Tech Stack

**Backend:** .NET 8, MediatR, FluentValidation, Serilog
**Frontend:** Angular 18, Standalone Components, SCSS
