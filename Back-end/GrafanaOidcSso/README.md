# Grafana SSO with Microsoft Entra ID (OIDC)

Este projeto demonstra como configurar **Single Sign-On (SSO) via OIDC** entre Grafana e Microsoft Entra External ID.

## Solu��o: OIDC (OpenID Connect)

**Por que OIDC?**
- [x] Funciona no Grafana OSS (gratuito)
- [x] Mais simples que SAML
- [x] Protocolo moderno (JSON vs XML)
- [x] Setup em 5 minutos
- [x] Suporta autoriza��o via App Roles

---

## Pr�-requisitos

1. **Docker & Docker Compose** instalados
2. **Tenant do Microsoft Entra ID** (External ID ou regular)
3. **App Registration** configurado no Azure Portal

---

## Setup R�pido (5 minutos)

### 1. Criar App Registration no Azure

```
Azure Portal -> Microsoft Entra ID -> App registrations -> + New registration
```

**Configura��o:**
- **Name**: `Grafana SSO`
- **Redirect URI**: 
  - Platform: **Web**
  - URI: `http://localhost:3001/login/generic_oauth`

### 2. Criar Client Secret

```
App registration -> Certificates & secrets -> + New client secret
```

**IMPORTANTE: Copie o VALUE imediatamente** (s� aparece uma vez!)

### 3. Copiar IDs necess�rios

Na p�gina **Overview** do App Registration, copie:

1. **Application (client) ID**
2. **Directory (tenant) ID**

### 4. Configurar App Roles (Autoriza��o)

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

### 5. Atribuir Usu�rios

```
Azure Portal -> Enterprise applications -> [Seu App] -> Users and groups -> + Add user/group
```

Para cada usu�rio, selecione uma role: Admin, Editor ou Viewer.

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
3. Fa�a login com sua conta Microsoft
4. **SSO funcionando!**

---

## Autoriza��o (Roles)

O Grafana tem 3 roles principais:

| Role | Permiss�es |
|------|-----------|
| **Admin** | Acesso total: gerenciar usu�rios, data sources, dashboards, configura��es |
| **Editor** | Criar/editar dashboards, queries e alertas |
| **Viewer** | Visualiza��o apenas (read-only) |

As roles s�o mapeadas automaticamente via App Roles do Azure AD.

---

## Seguran�a

### Arquivo .env

- O arquivo `.env` cont�m **informa��es sens�veis**
- **NUNCA** commite o `.env` para o Git
- O `.gitignore` j� protege este arquivo
- Para produ��o, use **Azure Key Vault** ou similar

### Rota��o de Secrets

Client secrets expiram! Verifique a data de expira��o no Azure Portal:

```
App registration -> Certificates & secrets -> Client secrets
```

Configure alertas de expira��o e planeje a rota��o.

---

## Comandos �teis

```bash
# Ver logs
docker-compose logs -f grafana

# Reiniciar
docker-compose restart grafana

# Parar
docker-compose down

# Parar e remover dados
docker-compose down -v

# Verificar configura��o (mostra valores do .env)
docker-compose config

# Ver vari�veis carregadas
docker-compose config | grep OAUTH
```

---

## Troubleshooting

### Bot�o SSO n�o aparece

```bash
# Verificar logs
docker-compose logs grafana | grep -i oauth

# Verificar se vari�veis do .env foram carregadas
docker-compose exec grafana env | grep OAUTH
```

**Solu��o:**
- Certifique-se de que o arquivo `.env` est� no mesmo diret�rio do `docker-compose.yml`
- Verifique se n�o h� espa�os extras nas vari�veis do `.env`
- Reinicie com `docker-compose down` e `docker-compose up -d`

### Erro "invalid_client"

- Verifique se o Client Secret est� correto
- Certifique-se de copiar o **Value**, n�o o Secret ID
- Verifique se o secret n�o expirou

### Erro "redirect_uri_mismatch"

- Redirect URI no Azure deve ser **exatamente**: `http://localhost:3001/login/generic_oauth`
- N�o adicione `/` no final
- Protocolo deve ser `http` para local (ou `https` para produ��o)

### Erro "invalid_request: AADSTS900144"

- Tenant ID incorreto
- Verifique o valor de `TENANT_ID` no `.env`
- Copie novamente da p�gina Overview do App Registration

### Role sempre Viewer

- Verifique se App Roles est�o configuradas
- Verifique se usu�rio est� atribu�do a uma role
- Verifique se claim `roles` est� no token (use jwt.io para decodificar)

---

## Estrutura de Arquivos

```
GrafanaOidcSso/
??? docker-compose.yml          # Configura��o principal
??? .env                        # Credenciais (N�O versionado)
??? .env.example                # Template de configura��o
??? .gitignore                  # Protege .env e dados sens�veis
??? README.md                   # Esta documenta��o
??? OIDC_QUICKSTART.md         # Guia detalhado passo a passo
```

---

## Refer�ncias

- [Grafana Generic OAuth](https://grafana.com/docs/grafana/latest/setup-grafana/configure-security/configure-authentication/generic-oauth/)
- [Microsoft Entra ID - OAuth 2.0](https://learn.microsoft.com/en-us/entra/identity-platform/v2-oauth2-auth-code-flow)
- [Grafana Roles & Permissions](https://grafana.com/docs/grafana/latest/administration/roles-and-permissions/)

---

## Pr�ximos Passos

Ap�s configurar o SSO b�sico:

1. Configurar HTTPS para produ��o
2. Adicionar MFA (via Azure AD Conditional Access)
3. Configurar backup de dashboards
4. Integrar com data sources (Prometheus, Loki, etc.)
5. Implementar rota��o autom�tica de secrets

---

## Checklist de Seguran�a

Antes de ir para produ��o:

- [ ] Usar HTTPS com certificado v�lido
- [ ] Secrets gerenciados via Azure Key Vault
- [ ] MFA habilitado no Azure AD
- [ ] Conditional Access policies configuradas
- [ ] Auditoria de acesso habilitada
- [ ] Alertas de expira��o de secrets
- [ ] Backup regular de dashboards
- [ ] Logs centralizados configurados

---

**Dica**: Para produ��o, sempre use **HTTPS** e **Azure Key Vault** para gerenciar secrets!
