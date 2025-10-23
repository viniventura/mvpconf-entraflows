# ToDoList Application - Docker Setup

## 🚀 Quick Start

### 1. Configure Environment Variables

```powershell
# Copy the example file
cp .env.example .env

# Edit .env with your Microsoft Entra credentials
# All configurations for API, Console, and SPA are in this single file
```

### 2. Run with Docker Compose

```bash
docker-compose up -d
```

### 3. Access the Application

- **SPA**: http://localhost:3000
- **API**: http://localhost:5000
- **API Swagger**: http://localhost:5000/swagger
- **Health Check**: http://localhost:5000/health

## 📝 Required Environment Variables

All environment variables are configured in a single `.env` file at the root of `src/ToDoList/`.

### API Configuration

```env
AZURE_AD_INSTANCE=https://your-tenant.ciamlogin.com/
AZURE_AD_TENANT_ID=your-tenant-id
AZURE_AD_CLIENT_ID=your-api-client-id
AZURE_AD_CLIENT_SECRET=your-api-client-secret
SWAGGER_CLIENT_ID=your-swagger-client-id
ASPNETCORE_ENVIRONMENT=Development
```

### Console Configuration

```env
CONSOLE_AZURE_AD_TENANT_ID=your-tenant-id
CONSOLE_AZURE_AD_CLIENT_ID=your-console-client-id
CONSOLE_AZURE_AD_CLIENT_SECRET=your-console-client-secret
TODOLIST_API_BASE_URL=http://api:8080/api
TODOLIST_API_SCOPE=api://your-api-client-id/.default
DOTNET_ENVIRONMENT=Development
```

### SPA Configuration

```env
REACT_APP_TENANT_ID=your-tenant-id
REACT_APP_TENANT_SUBDOMAIN=your-tenant-subdomain
REACT_APP_CLIENT_ID=your-spa-client-id
REACT_APP_API_CLIENT_ID=your-api-client-id
REACT_APP_API_ENDPOINT=http://localhost:3000/api/todolist
```

## �️ Local Development (Without Docker)

For local development outside Docker, each application has its own configuration:

- **API**: Uses User Secrets or `appsettings.local.json`
- **Console**: Uses User Secrets or `appsettings.local.json`  
- **SPA**: Create `SPA/.env.local` with React environment variables

The `.env` file in the root is specifically for Docker Compose.

## �📚 Full Documentation

See [DOCKER.md](./DOCKER.md) for complete documentation.

## 🛠️ Common Commands

```bash
# View logs
docker-compose logs -f

# Stop services
docker-compose down

# Rebuild and restart
docker-compose up -d --build

# View running containers
docker-compose ps
```
