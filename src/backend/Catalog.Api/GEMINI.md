# GEMINI.md

## Project Overview

This project is a .NET 8 Azure Functions application named `Catalog.Api`. It serves as the backend API for a catalog system. The project follows a layered architecture, with separate `Catalog.Core` and `Catalog.Infrastructure` projects. It utilizes Swagger for API documentation and includes a basic health check endpoint.

## Building and Running

To build and run this project, you will need the .NET 8 SDK and the Azure Functions Core Tools.

**Build:**

```bash
dotnet build
```

**Run:**

```bash
func start
```

The API will be available at `http://localhost:7071`. The health check endpoint is at `http://localhost:7071/health`. The Swagger UI is available at `http://localhost:7071/swagger`.

## Development Conventions

*   **Dependency Injection:** The project uses dependency injection, with services registered in `Program.cs`.
*   **Layered Architecture:** The code is organized into three layers: `Catalog.Api`, `Catalog.Core`, and `Catalog.Infrastructure`.
*   **Azure Functions:** The API endpoints are implemented as Azure Functions.
*   **Swagger:** The API is documented using Swagger. To enable Swagger during development, the `SWAGGER` compilation constant is defined in the `.csproj` file for the Debug configuration.
