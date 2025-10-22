# Setup Rápido - OIDC com Microsoft Entra ID (5 minutos)

## Opção Recomendada: OIDC ao invés de SAML

**Por quê?**
- Funciona no Grafana OSS (gratuito - sem necessidade de licença Enterprise)
- Mais simples que SAML
- Protocolo mais moderno (OpenID Connect)
- Setup em 5 minutos

---

## Passo 1: Criar App Registration no Azure

### 1.1. Acesse o Azure Portal

```
https://portal.azure.com
-> Microsoft Entra ID
-> App registrations
-> + New registration
```

### 1.2. Configurar a Aplicação

**Name**: `Grafana SSO Hands-on`

**Supported account types**: 
- [x] Accounts in this organizational directory only (Single tenant)

**Redirect URI**:
- Platform: **Web**
- URI: `http://localhost:3001/login/generic_oauth`

Click **Register**

---

## Passo 2: Criar Client Secret

### 2.1. Acessar Secrets

```
App registration -> Certificates & secrets -> Client secrets tab
-> + New client secret
```

### 2.2. Configurar Secret

**Description**: `grafana-local`
**Expires**: 3 months (ou conforme sua política de segurança)

Click **Add**

### 2.3. COPIAR O VALUE IMEDIATAMENTE

**IMPORTANTE**: O valor do secret só aparece **uma vez**!

Copie o campo **Value** (não o "Secret ID")

---

## Passo 3: Copiar IDs

Na página **Overview** do App Registration, copie:

1. **Application (client) ID** (formato GUID)
2. **Directory (tenant) ID** (formato GUID)

---

## Passo 4: Configurar Localmente

### 4.1. Criar arquivo .env

```bash
cd GrafanaOidcSso
cp .env.example .env
```

### 4.2. Editar .env

Abra o arquivo `.env` e preencha com os valores copiados do Azure Portal:

```env
# Application (client) ID from Azure Portal
OAUTH_CLIENT_ID=paste-your-client-id-here

# Client secret VALUE from Azure Portal
OAUTH_CLIENT_SECRET=paste-your-secret-value-here

# Directory (tenant) ID from Azure Portal
TENANT_ID=paste-your-tenant-id-here
```

**Salve o arquivo!**

**IMPORTANTE:** Não compartilhe estes valores! Eles são credenciais sensíveis.

---

## Passo 5: Iniciar o Grafana

### 5.1. Limpar ambiente anterior (se houver)

```bash
docker-compose down -v
```

### 5.2. Iniciar com OIDC

```bash
docker-compose up -d
```

### 5.3. Verificar logs

```bash
docker-compose logs -f grafana
```

**Procure por:**
```
[OK] msg="Generic OAuth authentication enabled"
[OK] msg="HTTP Server Listen"
```

---

## Passo 6: Testar SSO

### 6.1. Acessar Grafana

Abra o navegador em: **http://localhost:3001**

### 6.2. Login via SSO

Você verá **dois métodos de login**:

1. **"Sign in with Microsoft Entra"** <- Use este!
2. Username/Password (login local - fallback)

### 6.3. Clicar em "Sign in with Microsoft Entra"

- Você será redirecionado para `login.microsoftonline.com`
- Faça login com sua conta Microsoft do tenant
- Após autenticação, será redirecionado de volta ao Grafana
- **Login bem-sucedido!**

---

## Troubleshooting

### Problema 1: "invalid_client"

**Causa**: Client Secret incorreto ou expirado

**Solução**:
1. Volte ao Azure Portal
2. App registration -> Certificates & secrets
3. Crie um novo Client Secret
4. Atualize o `.env` com o novo valor
5. Reinicie: `docker-compose restart`

---

### Problema 2: "redirect_uri_mismatch"

**Causa**: Redirect URI não corresponde

**Solução**:
1. Azure Portal -> App registration -> Authentication
2. Verifique se tem **exatamente**: `http://localhost:3001/login/generic_oauth`
3. Não adicione `/` no final
4. Protocolo deve ser `http` (não `https`) para local

---

### Problema 3: Botão SSO não aparece

**Verificar configuração:**
```bash
# Ver variáveis OAuth
docker-compose exec grafana env | grep OAUTH

# Ver logs OAuth
docker-compose logs grafana | grep -i oauth
```

**Soluções**:
1. Verifique se o `.env` está no diretório correto
2. Verifique se as variáveis estão preenchidas (não vazias)
3. Reinicie com `--force-recreate`:
   ```bash
   docker-compose up -d --force-recreate
   ```

---

### Problema 4: "AADSTS50105: User is not assigned"

**Causa**: Usuário não está atribuído ao App Registration

**Solução**:
1. Azure Portal -> Enterprise applications
2. Procure por "Grafana SSO Hands-on"
3. Vá em **Users and groups**
4. Click **+ Add user/group**
5. Selecione seu usuário
6. Click **Assign**

**OU** desabilite a exigência de atribuição:
1. Enterprise applications -> "Grafana SSO Hands-on"
2. **Properties**
3. **Assignment required?** -> **No**
4. **Save**

---

## Comandos Úteis

```bash
# Parar
docker-compose down

# Parar e remover dados
docker-compose down -v

# Reiniciar
docker-compose restart grafana

# Ver logs
docker-compose logs -f grafana

# Ver apenas logs OAuth
docker-compose logs grafana | grep -i oauth

# Entrar no container
docker-compose exec grafana sh
```

---

## Checklist Final

Antes do hands-on, verifique:

- [ ] App Registration criado no Azure Portal
- [ ] Redirect URI configurado: `http://localhost:3001/login/generic_oauth`
- [ ] Client Secret criado e VALUE copiado
- [ ] Arquivo `.env` criado e preenchido com valores reais
- [ ] Container do Grafana rodando (`docker ps`)
- [ ] Botão "Sign in with Microsoft Entra" visível em http://localhost:3001
- [ ] Teste de login via SSO realizado com sucesso

---

## Pronto!

Se todos os itens do checklist estão OK, seu SSO via OIDC está funcionando!

**Vantagens do OIDC sobre SAML:**
- [x] Funciona no Grafana OSS (sem licença)
- [x] Setup mais rápido (5 min vs 15 min)
- [x] Debugging mais fácil (JSON vs XML)
- [x] Protocolo mais moderno e seguro
- [x] Melhor suporte a mobile/SPA

---

**Próximos passos:**
- Configurar role mapping (Admin, Editor, Viewer)
- Adicionar MFA (já suportado pelo Entra ID)
- Configurar Conditional Access policies
- Deploy em produção com HTTPS
