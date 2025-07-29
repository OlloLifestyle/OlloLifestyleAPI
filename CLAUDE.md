## Project Memories

- Remember layer project: A project involving layer management or layer-based architecture

## Project Overview

This is a production-grade .NET 9.0 ASP.NET Core Web API using Clean Architecture with multi-tenancy support:

- **OlloLifestyleAPI.Core** - Domain entities, interfaces, and DTOs
- **OlloLifestyleAPI.Application** - Business logic, services, validation, and mapping
- **OlloLifestyleAPI.Infrastructure** - Data access, external services, and cross-cutting concerns
- **OlloLifestyleAPI** - Web API layer with controllers, middleware, and configuration

### Key Components
- **Repository Pattern**: Generic repository with Unit of Work
- **AutoMapper**: Entity to DTO mapping
- **FluentValidation**: Request validation
- **Serilog**: Structured logging with file and console output
- **Global Exception Handling**: Centralized error management
- **Swagger Documentation**: API documentation with JWT auth support