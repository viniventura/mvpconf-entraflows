# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["ConsoleApp.csproj", "./"]
RUN dotnet restore "ConsoleApp.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "ConsoleApp.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ConsoleApp.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage - Alpine Linux (lightweight and secure)
FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine AS final
WORKDIR /app

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConsoleApp.dll"]
