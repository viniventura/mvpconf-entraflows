# MVP Conf 2025 - Microsoft Entra ID Authentication Flows

> **Hands-on tutorial** demonstrating authentication flows with Microsoft Entra ID (formerly Azure AD) using React SPA, ASP.NET Core Web API, and .NET Console daemon.

[![Architecture](https://img.shields.io/badge/Architecture-Mermaid_Diagram-blue)](./src/ToDoList/ARCHITECTURE.md)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)](./src/ToDoList/DOCKER.md)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)]()
[![React](https://img.shields.io/badge/React-18.3.1-61DAFB?logo=react)]()

## 📚 Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Quick Start](#quick-start)
- [Project Structure](#project-structure)
- [Authentication Flows](#authentication-flows)
- [Configuration](#configuration)
- [Documentation](#documentation)

## 🎯 Overview

This repository contains a complete hands-on tutorial for **MVP Conf 2025** demonstrating three Microsoft Entra ID authentication patterns:

| Pattern | Use Case | Technology | Flow Type |
|---------|----------|------------|-----------|
| 🌐 **SPA → API** | User-interactive web app | React + MSAL React | Authorization Code + PKCE |
| ⚙️ **Daemon → API** | Background worker/service | .NET Console + MSAL.NET | Client Credentials |
| 📊 **API → Graph** | API calling downstream service | ASP.NET Core + Microsoft.Identity.Web | On-Behalf-Of (OBO) |

### What You'll Learn

✅ Configure Microsoft Entra ID app registrations  
✅ Implement Authorization Code flow with PKCE (SPA)  
✅ Implement Client Credentials flow (Daemon)  
✅ Use On-Behalf-Of (OBO) flow for downstream APIs  
✅ Validate JWT tokens in ASP.NET Core  
✅ Deploy with Docker Compose  

## 🏗️ Architecture

See **[ARCHITECTURE.md](./src/ToDoList/ARCHITECTURE.md)** for the complete system diagram with authentication flows.

## 🚀 Quick Start

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop) (Windows/Mac/Linux)
- Microsoft Entra ID tenant (free tier works)
- [Optional] .NET 8 SDK for local development
- [Optional] Node.js 22.20+ for local SPA development

### 1. Clone the Repository

```bash
git clone https://github.com/viniventura/mvpconf-entraflows.git
cd mvpconf-entraflows/src/ToDoList
```

### 2. Configure Microsoft Entra ID

#### Create 3 App Registrations:

1. **SPA App** (Single-Page Application)
   - Platform: SPA
   - Redirect URIs: `http://localhost:3000`, `http://localhost:3000/redirect.html`
   - API Permissions: ToDoList.Read, ToDoList.ReadWrite (delegated)

2. **API App** (Web API)
   - Expose scopes: `api://{clientId}/ToDoList.Read`, `api://{clientId}/ToDoList.ReadWrite`
   - App roles: `ToDoList.Read.All`, `ToDoList.ReadWrite.All`
   - API Permissions: Microsoft Graph → User.Read (delegated)

3. **Console App** (Confidential Client)
   - Client secret: Yes
   - API Permissions: ToDoList.Read.All (application role, requires admin consent)

👉 **[Detailed Entra ID setup guide →](./src/ToDoList/DOCKER.md#entra-id-configuration)**

### 3. Configure Environment Variables

```bash
# Copy the example file
cp .env.example .env

# Edit .env with your credentials
```

Required variables:

```env
# API
AZURE_AD_TENANT_ID=your-tenant-id
AZURE_AD_CLIENT_ID=your-api-client-id
AZURE_AD_CLIENT_SECRET=your-api-secret

# Console
CONSOLE_AZURE_AD_CLIENT_ID=your-console-client-id
CONSOLE_AZURE_AD_CLIENT_SECRET=your-console-secret

# SPA
REACT_APP_CLIENT_ID=your-spa-client-id
REACT_APP_API_CLIENT_ID=your-api-client-id
```

### 4. Run with Docker Compose

```bash
docker-compose up -d
```

### 5. Access the Application

| Service | URL | Description |
|---------|-----|-------------|
| **React SPA** | http://localhost:3000 | User interface with login |
| **API** | http://localhost:5000 | REST API (protected) |
| **Swagger UI** | http://localhost:5000/swagger | Interactive API docs |
| **Console** | `docker logs todolist-console -f` | Background worker logs |

## 📁 Project Structure

```
mvpconf-entraflows/
├── src/
│   ├── ToDoList/              # Main tutorial project
│   │   ├── API/
│   │   │   └── ToDoListAPI/   # ASP.NET Core 8.0 Web API
│   │   ├── SPA/               # React 18.3.1 + MSAL React
│   │   ├── Console/           # .NET 8 daemon app
│   │   ├── docker-compose.yml # Orchestration
│   │   ├── .env              # Configuration (gitignored)
│   │   ├── ARCHITECTURE.md   # Detailed architecture
│   │   └── DOCKER.md         # Docker setup guide
│   │
│   └── GrafanaOidcSso/       # Bonus: Grafana OIDC PoC
│
├── .github/
│   └── copilot-instructions.md  # GitHub Copilot context
└── README.md                     # This file
```

## 🔐 Authentication Flows

### 1. Authorization Code + PKCE (SPA)

**Used by:** React SPA  
**User context:** Yes (interactive login)  
**Token type:** Delegated permissions (scopes)

```javascript
// MSAL React configuration
const msalConfig = {
  auth: {
    clientId: "spa-client-id",
    authority: "https://login.microsoftonline.com/{tenant}",
    redirectUri: "http://localhost:3000"
  }
};
```

### 2. Client Credentials (Daemon)

**Used by:** Console worker  
**User context:** No (service-to-service)  
**Token type:** Application permissions (roles)

```csharp
// MSAL.NET daemon authentication
var app = ConfidentialClientApplicationBuilder
    .Create(clientId)
    .WithClientSecret(clientSecret)
    .WithAuthority(authority)
    .Build();

var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
```

### 3. On-Behalf-Of (API → Graph)

**Used by:** ToDoList API  
**User context:** Yes (preserves original user)  
**Token type:** Exchange user token for Graph token

```csharp
// Microsoft.Identity.Web OBO flow
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options => { /* ... */ })
    .EnableTokenAcquisitionToCallDownstreamApi(options => { /* ... */ })
    .AddDownstreamApi("GraphApi", config.GetSection("DownstreamApis:GraphApi"));
```

## ⚙️ Configuration

### Local Development (without Docker)

Each component can run independently:

#### API
```bash
cd src/ToDoList/API/ToDoListAPI
dotnet user-secrets set "AzureAd:ClientId" "your-client-id"
dotnet user-secrets set "AzureAd:ClientSecret" "your-secret"
dotnet run
```

#### SPA
```bash
cd src/ToDoList/SPA
cp .env.example .env.local
# Edit .env.local
npm install
npm start
```

#### Console
```bash
cd src/ToDoList/Console
dotnet user-secrets set "AzureAd:ClientId" "your-client-id"
dotnet user-secrets set "AzureAd:ClientSecret" "your-secret"
dotnet run
```

### Docker Deployment

See **[DOCKER.md](./src/ToDoList/DOCKER.md)** for:
- Multi-stage Dockerfile explanations
- Alpine Linux optimizations
- Environment variable management
- Health checks and dependencies
- Troubleshooting

## 📖 Documentation

| Document | Description |
|----------|-------------|
| [ARCHITECTURE.md](./src/ToDoList/ARCHITECTURE.md) | System architecture with Mermaid diagrams |
| [DOCKER.md](./src/ToDoList/DOCKER.md) | Complete Docker setup and deployment guide |
| [.github/copilot-instructions.md](./.github/copilot-instructions.md) | Project context for GitHub Copilot |

## 🔧 Technologies Used

### Frontend
- **React** 18.3.1 - UI library
- **MSAL React** 2.x - Microsoft Authentication Library
- **Bootstrap** 5.x - CSS framework
- **Nginx** (Alpine) - Production web server

### Backend
- **ASP.NET Core** 8.0 - Web API framework
- **Microsoft.Identity.Web** - Token validation and OBO flow
- **Entity Framework Core** - In-memory database
- **Swashbuckle** - Swagger/OpenAPI documentation

### Infrastructure
- **Docker** & **Docker Compose** - Containerization
- **Alpine Linux** - Lightweight base images
- **Wget** - Health check utility

## 🎓 Learning Resources

- [Microsoft Entra External ID Documentation](https://learn.microsoft.com/entra/external-id/)
- [MSAL React Documentation](https://github.com/AzureAD/microsoft-authentication-library-for-js/tree/dev/lib/msal-react)
- [Microsoft.Identity.Web](https://learn.microsoft.com/entra/msal/dotnet/microsoft-identity-web/)
- [OAuth 2.0 and OpenID Connect Protocols](https://learn.microsoft.com/entra/identity-platform/v2-protocols)

## 🤝 Contributing

This is a tutorial repository for MVP Conf 2025. If you find issues or have suggestions:

1. Open an issue describing the problem
2. Fork and create a pull request with fixes
3. Follow the existing code style and structure

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👤 Author

**Vinícius Ventura**  
MVP Conf 2025 Speaker

---

**Happy learning!** 🚀 If you found this tutorial helpful, please ⭐ star the repository!
