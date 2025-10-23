# Docker Compose - ToDoList Application

Este arquivo Docker Compose orquestra os três workloads da aplicação ToDoList:

## 🏗️ Arquitetura

```
┌─────────────┐      ┌─────────────┐      ┌─────────────┐
│     SPA     │─────▶│     API     │◀─────│   Console   │
│  (React)    │      │  (ASP.NET)  │      │  (Daemon)   │
│  Port 3000  │      │  Port 5000  │      │             │
└─────────────┘      └─────────────┘      └─────────────┘
```

## 📦 Serviços

### 1. **API** (ASP.NET Core Web API)
- **Porta**: `5000` (HTTP)
- **Imagem Base**: `mcr.microsoft.com/dotnet/aspnet:8.0`
- **Função**: API REST protegida com Microsoft Entra External ID
- **Health Check**: `GET /health`

### 2. **SPA** (React Single Page Application)
- **Porta**: `3000`
- **Imagem Base**: `node:22.20.0-alpine` (build) + `nginx:alpine` (runtime)
- **Função**: Interface web usando MSAL React
- **Dependência**: Aguarda API estar saudável antes de iniciar

### 3. **Console** (Console Daemon App)
- **Porta**: Nenhuma (aplicação console)
- **Imagem Base**: `mcr.microsoft.com/dotnet/runtime:8.0`
- **Função**: Aplicação daemon para tarefas em background
- **Dependência**: Aguarda API estar saudável antes de iniciar

## 🚀 Como Usar

### Pré-requisitos

- Docker Desktop instalado
- Docker Compose v3.8 ou superior
- User Secrets configurados na API (ou arquivo `.env`)

### Configurar User Secrets

#### Opção 1: Extrair User Secrets automaticamente (Recomendado)

Se você já tem User Secrets configurados:

```powershell
# Na pasta src/ToDoList/
.\extract-secrets.ps1          # Extrai secrets da API
.\extract-secrets-console.ps1  # Extrai secrets do Console
.\extract-secrets-spa.ps1      # Extrai secrets do SPA
```

Estes scripts criarão automaticamente os arquivos `.env` com suas credenciais.

#### Opção 2: Criar .env manualmente

```bash
# Copie os exemplos
cp API/ToDoListAPI/.env.example API/ToDoListAPI/.env
cp Console/.env.example Console/.env
cp SPA/.env.example SPA/.env.local

# Edite cada .env com suas credenciais
# Use seu editor favorito para preencher os valores
```

Preencha com os valores do Microsoft Entra admin center:
- `AZURE_AD_INSTANCE`: URL do tenant (ex: https://contoso.ciamlogin.com/)
- `AZURE_AD_TENANT_ID`: Directory (tenant) ID
- `AZURE_AD_CLIENT_ID`: Application (client) ID da API
- `AZURE_AD_CLIENT_SECRET`: Client Secret da API
- `SWAGGER_CLIENT_ID`: Application (client) ID para Swagger UI

#### Opção 3: User Secrets via dotnet CLI

```bash
cd API/ToDoListAPI
dotnet user-secrets set "AzureAd:Instance" "https://your-tenant.ciamlogin.com/"
dotnet user-secrets set "AzureAd:TenantId" "your-tenant-id"
dotnet user-secrets set "AzureAd:ClientId" "your-client-id"
dotnet user-secrets set "AzureAd:ClientSecret" "your-client-secret"
dotnet user-secrets set "Swagger:ClientId" "your-swagger-client-id"

cd ../Console
dotnet user-secrets set "AzureAd:TenantId" "your-tenant-id"
dotnet user-secrets set "AzureAd:ClientId" "your-console-client-id"
dotnet user-secrets set "AzureAd:ClientSecret" "your-console-client-secret"
dotnet user-secrets set "ToDoListApi:Scopes:0" "api://your-api-client-id/.default"

# Depois, extraia com os scripts PowerShell
cd ../..
.\extract-secrets.ps1
.\extract-secrets-console.ps1
```

### Iniciar todos os serviços

```bash
# Na pasta src/ToDoList/
docker-compose up -d
```

### Verificar status dos containers

```bash
docker-compose ps
```

### Ver logs

```bash
# Todos os serviços
docker-compose logs -f

# Apenas um serviço específico
docker-compose logs -f api
docker-compose logs -f spa
docker-compose logs -f console
```

### Parar os serviços

```bash
docker-compose down
```

### Rebuild e restart

```bash
# Rebuild as imagens
docker-compose build

# Rebuild e restart
docker-compose up -d --build
```

### Remover tudo (incluindo volumes)

```bash
docker-compose down -v
```

## 🌐 Acesso às Aplicações

Após o `docker-compose up`:

- **SPA**: http://localhost:3000
- **API**: http://localhost:5000
- **API Health Check**: http://localhost:5000/health
- **API Swagger** (Dev): http://localhost:5000/swagger

## ⚙️ Variáveis de Ambiente

### API

As seguintes variáveis são **obrigatórias** e devem estar no arquivo `API/ToDoListAPI/.env`:

#### Azure AD (Microsoft Entra External ID)
- `AZURE_AD_INSTANCE`: URL base do tenant (ex: `https://contoso.ciamlogin.com/`)
- `AZURE_AD_TENANT_ID`: ID do diretório/tenant (GUID)
- `AZURE_AD_CLIENT_ID`: ID da aplicação API (GUID)
- `AZURE_AD_CLIENT_SECRET`: Secret da aplicação API

#### Swagger
- `SWAGGER_CLIENT_ID`: ID da aplicação para Swagger UI (GUID)

#### Opcionais
- `TODOLIST_READ_SCOPE`: Scope customizado de leitura
- `TODOLIST_READWRITE_SCOPE`: Scope customizado de leitura/escrita
- `USER_READ_SCOPE`: Scope customizado de leitura de usuário
- `ASPNETCORE_ENVIRONMENT`: Ambiente ASP.NET (padrão: `Development`)

### Console

As seguintes variáveis são **obrigatórias** e devem estar no arquivo `Console/.env`:

#### Azure AD (Daemon App)
- `AZURE_AD_TENANT_ID`: ID do diretório/tenant (GUID)
- `AZURE_AD_CLIENT_ID`: ID da aplicação Console (GUID)
- `AZURE_AD_CLIENT_SECRET`: Secret da aplicação Console

#### ToDoList API
- `TODOLIST_API_BASE_URL`: URL da API
  - **Desenvolvimento local** (fora do Docker): `http://localhost:5000/api`
  - **Docker Compose**: `http://api:8080/api` (padrão se não definido)
- `TODOLIST_API_SCOPE`: Scope de acesso (ex: `api://api-client-id/.default`)

#### Opcionais
- `DOTNET_ENVIRONMENT`: Ambiente .NET (padrão: `Development`)

### SPA
- `NODE_ENV`: Define o ambiente do Node.js (definido automaticamente como `production`)

As seguintes variáveis são **opcionais** e devem estar no arquivo `SPA/.env.local` (para desenvolvimento local):

#### React App Configuration

**Para Docker**: As variáveis são passadas como **build arguments** no `docker-compose.yml` e compiladas na imagem durante o build:

- `REACT_APP_TENANT_ID`: ID do tenant (GUID)
- `REACT_APP_TENANT_SUBDOMAIN`: Subdomain do tenant (ex: `contoso`)
- `REACT_APP_CLIENT_ID`: ID da aplicação SPA (GUID)
- `REACT_APP_API_CLIENT_ID`: ID da aplicação API (GUID)
- `REACT_APP_API_ENDPOINT`: URL do endpoint da API
  - **Docker**: `http://localhost:5000/api/todolist`
  - **Produção**: Ajustar para o domínio real
- `REACT_APP_REDIRECT_URI`: URI de redirecionamento após login (padrão: `http://localhost:3000`)

**Para Desenvolvimento Local**: Crie o arquivo `SPA/.env.local` (não versionado) com as mesmas variáveis acima.

**⚠️ IMPORTANTE**: 
- No Docker, as variáveis React são **compiladas em tempo de build** e não podem ser alteradas após a imagem ser criada
- Credenciais do SPA (Client ID) não são secrets críticos, mas mantenha o `.env` fora do Git
- Para mudar configurações React, é necessário fazer **rebuild**: `docker-compose up -d --build spa`

**Localização dos arquivos**:
- API: `src/ToDoList/API/ToDoListAPI/.env`
- Console: `src/ToDoList/Console/.env`
- SPA: `src/ToDoList/SPA/.env.local`

## 🔧 Configurações Locais

**IMPORTANTE**: As credenciais do Microsoft Entra External ID podem ser fornecidas de duas formas:

### Para Docker (Recomendado)

Use os arquivos `.env` nas pastas de cada aplicação:
- API: `API/ToDoListAPI/.env`
- Console: `Console/.env`

```bash
# Gere automaticamente dos User Secrets
.\extract-secrets.ps1          # Para API
.\extract-secrets-console.ps1  # Para Console

# Ou crie manualmente a partir dos .env.example
cp API/ToDoListAPI/.env.example API/ToDoListAPI/.env
cp Console/.env.example Console/.env
# Depois edite cada .env com suas credenciais
```

### Para Desenvolvimento Local (sem Docker)

Use os arquivos de configuração local:

- `SPA/src/authConfig.local.js` - Configurações de autenticação do React
- `API/ToDoListAPI/appsettings.local.json` - Configurações da API

Ou use **User Secrets** do .NET (mais seguro):

```bash
cd API/ToDoListAPI
dotnet user-secrets list
```

**Hierarquia de Configuração da API**:
1. Variáveis de ambiente (`.env` no Docker)
2. User Secrets (desenvolvimento local)
3. `appsettings.local.json` (desenvolvimento local)
4. `appsettings.json` (base com placeholders)

Esses arquivos são ignorados pelo `.dockerignore` e `.gitignore` e **não devem** ser incluídos na imagem Docker em produção.

## 🐛 Troubleshooting

### API não inicia
```bash
docker-compose logs api
```

Verifique se as configurações do Microsoft Entra estão corretas.

### SPA não consegue conectar à API
- Verifique se a API está saudável: `curl http://localhost:5000/health`
- Verifique as configurações de CORS na API

### Console falha ao iniciar
- Verifique os logs: `docker-compose logs console`
- Certifique-se de que a API está respondendo

## 🔒 Segurança

⚠️ **ATENÇÃO**: Esta configuração é para desenvolvimento local.

Para produção, considere:
- Usar HTTPS/TLS
- Configurar secrets management (Azure Key Vault, Docker Secrets)
- Restringir CORS para domínios específicos
- Usar variáveis de ambiente seguras
- Implementar rate limiting

## 📝 Notas

- A rede `todolist-network` isola os containers
- O health check da API garante que dependentes só iniciem quando ela estiver pronta
- O Console usa `restart: unless-stopped` para recuperação automática
