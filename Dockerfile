# Use the official .NET 9.0 runtime as base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Use the SDK image to build the application
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
COPY ["OlloLifestyleAPI/OlloLifestyleAPI.csproj", "OlloLifestyleAPI/"]
COPY ["OlloLifestyleAPI.Application/OlloLifestyleAPI.Application.csproj", "OlloLifestyleAPI.Application/"]
COPY ["OlloLifestyleAPI.Core/OlloLifestyleAPI.Core.csproj", "OlloLifestyleAPI.Core/"]
COPY ["OlloLifestyleAPI.Infrastructure/OlloLifestyleAPI.Infrastructure.csproj", "OlloLifestyleAPI.Infrastructure/"]

RUN dotnet restore "OlloLifestyleAPI/OlloLifestyleAPI.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/OlloLifestyleAPI"
RUN dotnet build "OlloLifestyleAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "OlloLifestyleAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage/image
FROM base AS final
WORKDIR /app

# Create logs directory
RUN mkdir -p /app/Logs

# Copy published application
COPY --from=publish /app/publish .

# Create non-root user for security
RUN adduser --disabled-password --gecos '' --uid 1000 apiuser && \
    chown -R apiuser:apiuser /app
USER apiuser

ENTRYPOINT ["dotnet", "OlloLifestyleAPI.dll"]