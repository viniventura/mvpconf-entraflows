# Grafana SSO with Microsoft Entra ID (OIDC)

Este projeto demonstra como configurar **Single Sign-On (SSO) via OIDC** entre Grafana e Microsoft Entra External ID.

## Solução: OIDC (OpenID Connect)

**Por que OIDC?**
- [x] Funciona no Grafana OSS (gratuito)
- [x] Mais simples que SAML
- [x] Protocolo moderno (JSON vs XML)
- [x] Setup em 5 minutos
- [x] Suporta autorização via App Roles

---

## Pré-requisitos

1. **Docker & Docker Compose** instalados
2. **Tenant do Microsoft Entra ID** (External ID ou regular)
3. **App Registration** configurado no Azure Portal

---

## Setup Rápido (5 minutos)

### 1. Criar App Registration no Azure

```
Azure Portal -> Microsoft Entra ID -> App registrations -> + New registration
```

**Configuração:**
- **Name**: `Grafana SSO`
- **Redirect URI**: 
  - Platform: **Web**
  - URI: `http://localhost:3001/login/generic_oauth`

### 2. Criar Client Secret

```
App registration -> Certificates & secrets -> + New client secret
```

**IMPORTANTE: Copie o VALUE imediatamente** (só aparece uma vez!)

### 3. Copiar IDs necessários

Na página **Overview** do App Registration, copie:

1. **Application (client) ID**
2. **Directory (tenant) ID**

### 4. Configurar App Roles (Autorização)

```
App registration -> App roles -> + Create app role
```

**Crie 3 roles:**

**Grafana Administrator:**
```
Display name: Grafana Administrator
Value: Admin
Description: Full administrative access to Grafana. Can manage users, data sources, dashboards, plugins, and all system settings.
Allowed member types: Users/Groups
```

**Grafana Editor:**
```
Display name: Grafana Editor
Value: Editor
Description: Can create and edit dashboards, queries, and alerts. Cannot manage users or system-wide settings.
Allowed member types: Users/Groups
```

**Grafana Viewer:**
```
Display name: Grafana Viewer
Value: Viewer
Description: Read-only access to dashboards and data. Can view all dashboards but cannot make any changes.
Allowed member types: Users/Groups
```

### 5. Atribuir Usuários

```
Azure Portal -> Enterprise applications -> [Seu App] -> Users and groups -> + Add user/group
```

Para cada usuário, selecione uma role: Admin, Editor ou Viewer.

### 6. Configurar Token (incluir roles)

```
App registration -> Token configuration -> + Add optional claim
```

- Token type: **ID**
- Claim: **roles**

### 7. Configurar Localmente

Copie o arquivo de exemplo:

```bash
cd GrafanaOidcSso
cp .env.example .env
```

Edite o `.env` com suas credenciais:

```env
# Application (client) ID from Azure Portal
OAUTH_CLIENT_ID=your-client-id-here

# Client secret VALUE (copy the VALUE, not the Secret ID!)
OAUTH_CLIENT_SECRET=your-client-secret-here

# Directory (tenant) ID from Azure Portal
TENANT_ID=your-tenant-id-here
```

**IMPORTANTE: Substitua pelos seus valores reais obtidos no Azure Portal!**

### 8. Iniciar o Grafana

```bash
docker-compose up -d
```

### 9. Acessar e Testar

1. Acesse: **http://localhost:3001**
2. Clique em **"Sign in with Microsoft Entra"**
3. Faça login com sua conta Microsoft
4. **SSO funcionando!**

---

## Autorização (Roles)

O Grafana tem 3 roles principais:

| Role | Permissões |
|------|-----------|
| **Admin** | Acesso total: gerenciar usuários, data sources, dashboards, configurações |
| **Editor** | Criar/editar dashboards, queries e alertas |
| **Viewer** | Visualização apenas (read-only) |

As roles são mapeadas automaticamente via App Roles do Azure AD.

---

## Segurança

### Arquivo .env

- O arquivo `.env` contém **informações sensíveis**
- **NUNCA** commite o `.env` para o Git
- O `.gitignore` já protege este arquivo
- Para produção, use **Azure Key Vault** ou similar

### Rotação de Secrets

Client secrets expiram! Verifique a data de expiração no Azure Portal:

```
App registration -> Certificates & secrets -> Client secrets
```

Configure alertas de expiração e planeje a rotação.

---

## Comandos Úteis

```bash
# Ver logs
docker-compose logs -f grafana

# Reiniciar
docker-compose restart grafana

# Parar
docker-compose down

# Parar e remover dados
docker-compose down -v

# Verificar configuração (mostra valores do .env)
docker-compose config

# Ver variáveis carregadas
docker-compose config | grep OAUTH
```

---

## Troubleshooting

### Botão SSO não aparece

```bash
# Verificar logs
docker-compose logs grafana | grep -i oauth

# Verificar se variáveis do .env foram carregadas
docker-compose exec grafana env | grep OAUTH
```

**Solução:**
- Certifique-se de que o arquivo `.env` está no mesmo diretório do `docker-compose.yml`
- Verifique se não há espaços extras nas variáveis do `.env`
- Reinicie com `docker-compose down` e `docker-compose up -d`

### Erro "invalid_client"

- Verifique se o Client Secret está correto
- Certifique-se de copiar o **Value**, não o Secret ID
- Verifique se o secret não expirou

### Erro "redirect_uri_mismatch"

- Redirect URI no Azure deve ser **exatamente**: `http://localhost:3001/login/generic_oauth`
- Não adicione `/` no final
- Protocolo deve ser `http` para local (ou `https` para produção)

### Erro "invalid_request: AADSTS900144"

- Tenant ID incorreto
- Verifique o valor de `TENANT_ID` no `.env`
- Copie novamente da página Overview do App Registration

### Role sempre Viewer

- Verifique se App Roles estão configuradas
- Verifique se usuário está atribuído a uma role
- Verifique se claim `roles` está no token (use jwt.io para decodificar)

---

## Estrutura de Arquivos

```
GrafanaOidcSso/
├── docker-compose.yml          # Configuração principal
├── .env                        # Credenciais (NÃO versionado)
├── .env.example                # Template de configuração
├── .gitignore                  # Protege .env e dados sensíveis
├── README.md                   # Esta documentação
└── OIDC_QUICKSTART.md         # Guia detalhado passo a passo
```

---

## Referências

- [Grafana Generic OAuth](https://grafana.com/docs/grafana/latest/setup-grafana/configure-security/configure-authentication/generic-oauth/)
- [Microsoft Entra ID - OAuth 2.0](https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-auth-code-flow)
- [Grafana Roles & Permissions](https://grafana.com/docs/grafana/latest/administration/roles-and-permissions/)

---

## Próximos Passos

Após configurar o SSO básico:

1. Configurar HTTPS para produção
2. Adicionar MFA (via Azure AD Conditional Access)
3. Configurar backup de dashboards
4. Integrar com data sources (Prometheus, Loki, etc.)
5. Implementar rotação automática de secrets

---

## Checklist de Segurança

Antes de ir para produção:

- [ ] Usar HTTPS com certificado válido
- [ ] Secrets gerenciados via Azure Key Vault
- [ ] MFA habilitado no Azure AD
- [ ] Conditional Access policies configuradas
- [ ] Auditoria de acesso habilitada
- [ ] Alertas de expiração de secrets
- [ ] Backup regular de dashboards
- [ ] Logs centralizados configurados

---

**Dica**: Para produção, sempre use **HTTPS** e **Azure Key Vault** para gerenciar secrets!
