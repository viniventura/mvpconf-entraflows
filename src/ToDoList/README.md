# ToDoList - Microsoft Entra ID Tutorial

👉 **See the [main README](../../README.md) for complete documentation.**

## Quick Start

### Run with Docker Compose

```bash
# 1. Configure environment
cp .env.example .env
# Edit .env with your Entra ID credentials

# 2. Start all services
docker-compose up -d

# 3. Access
# - SPA: http://localhost:3000
# - API: http://localhost:5000
# - Swagger: http://localhost:5000/swagger
```

### Check Service Status

```bash
# View all containers
docker-compose ps

# View logs
docker-compose logs -f api
docker-compose logs -f spa
docker-compose logs -f console
```

## Documentation

| Document | Description |
|----------|-------------|
| [ARCHITECTURE.md](./ARCHITECTURE.md) | System architecture with Mermaid diagrams |
| [DOCKER.md](./DOCKER.md) | Complete Docker setup guide |
| [Main README](../../README.md) | Full tutorial and configuration |

## Project Structure

```
ToDoList/
├── API/ToDoListAPI/   # ASP.NET Core 8.0 Web API
├── SPA/               # React 18.3.1 + MSAL React
├── Console/           # .NET 8 Console Daemon
├── docker-compose.yml # Container orchestration
├── .env              # Configuration (gitignored)
└── .env.example      # Configuration template
```

---

**MVP Conf 2025** | [GitHub Repository](https://github.com/viniventura/mvpconf-entraflows)
