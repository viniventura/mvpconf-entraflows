# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["ToDoListAPI.csproj", "./"]
RUN dotnet restore "ToDoListAPI.csproj"

# Copy everything else and build
COPY . .
RUN dotnet build "ToDoListAPI.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "ToDoListAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage - Alpine Linux (lightweight and secure)
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ToDoListAPI.dll"]
