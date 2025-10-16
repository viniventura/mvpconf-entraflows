# Instruções: React SPA com Microsoft Entra External ID e API ASP.NET Core

Este documento fornece instruções completas para configurar uma aplicação React (SPA) que autentica usuários com Microsoft Entra External ID e chama uma API ASP.NET Core protegida.

## Visão Geral

Este exemplo demonstra uma aplicação React single-page (SPA) que permite aos usuários fazer login com Microsoft Entra External ID usando a Microsoft Authentication Library for React (MSAL React).

### Cenário

1. O cliente React SPA usa MSAL para fazer login de um usuário e obter um JWT ID Token e Access Token do Microsoft Entra External ID
2. O access token é usado como bearer token para autorizar o usuário a chamar a API ASP.NET Core protegida pelo Microsoft Entra External ID
3. O serviço usa Microsoft.Identity.Web para proteger a Web API, verificar permissões e validar tokens

## Pré-requisitos

- Visual Studio ou Visual Studio Code e .NET Core SDK
- Um tenant externo do Microsoft Entra External ID
- Uma conta de usuário com permissões no tenant externo

## Configuração do Projeto

### Passo 1: Clone ou baixe o repositório

```bash
git clone https://github.com/Azure-Samples/ms-identity-ciam-javascript-tutorial.git
```

### Passo 2: Navegue para a pasta do projeto

```bash
cd 2-Authorization\1-call-api-react\SPA
npm install
```

### Passo 3: Registrar as aplicações no seu tenant

#### Escolher o tenant Microsoft Entra External ID

1. Entre no [Microsoft Entra admin center](https://entra.microsoft.com/)
2. Se sua conta estiver presente em mais de um tenant, selecione seu perfil no canto superior direito e mude para o tenant desejado

#### Criar User Flows

Consulte: [Tutorial: Create a sign-up and sign-in user flow](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-user-flow-sign-up-sign-in-customers)

#### Adicionar Provedores de Identidade Externa (Opcional)

- [Tutorial: Add Google as an identity provider](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-google-federation-customers)
- [Tutorial: Add Facebook as an identity provider](https://learn.microsoft.com/en-us/entra/external-id/customers/how-to-facebook-federation-customers)

#### Registrar a aplicação de serviço (ciam-msal-dotnet-api)

1. Navegue para o [Microsoft Entra admin center](https://entra.microsoft.com/) e selecione o serviço Microsoft Entra External ID
2. Selecione App Registrations no menu à esquerda, então selecione New registration
3. Na página Register an application que aparece, insira as informações de registro:
   - **Name**: `ciam-msal-dotnet-api`
   - **Supported account types**: Accounts in this organizational directory only
   - Selecione Register

4. Na página Overview, encontre e anote o Application (client) ID
5. Na tela de registro da app, selecione Expose an API à esquerda
6. Selecione Set próximo ao Application ID URI para gerar um URI único para esta app
7. Aceite o URI proposto (`api://{clientId}`) selecionando Save

##### Publicar Permissões Delegadas

1. Selecione Add a scope e insira os valores:
   - **Scope name**: `ToDoList.Read`
   - **Admin consent display name**: Read users ToDo list using the 'ciam-msal-dotnet-api'
   - **Admin consent description**: Allow the app to read the user's ToDo list using the 'ciam-msal-dotnet-api'
   - **State**: Enabled
   - Selecione Add scope

2. Repita para outro scope chamado `ToDoList.ReadWrite`

3. Selecione Manifest à esquerda
   - Defina `accessTokenAcceptedVersion` como 2
   - Selecione Save

##### Publicar Permissões de Aplicação

1. Selecione App roles à esquerda
2. Selecione Create app role:
   - **Display name**: `ToDoList.Read.All`
   - **Allowed member types**: Application
   - **Value**: `ToDoList.Read.All`
   - **Description**: Allow the app to read every user's ToDo list using the 'ciam-msal-dotnet-api'
   - Selecione Apply

3. Repita para outra permissão de app chamada `ToDoList.ReadWrite.All`

##### Configurar Claims Opcionais

1. Selecione Token configuration à esquerda
2. Selecione Add optional claim:
   - Selecione Access como tipo de claim
   - Selecione o claim opcional `idtyp`
   - Selecione Add

##### Configurar a aplicação de serviço

1. Abra o arquivo `API\ToDoListAPI\appsettings.json`
2. Encontre a chave `Enter_the_Application_Id_Here` e substitua pelo Application ID da app `ciam-msal-dotnet-api`
3. Encontre a chave `Enter_the_Tenant_Id_Here` e substitua pelo seu external tenant/directory ID
4. Encontre o placeholder `Enter_the_Tenant_Subdomain_Here` e substitua pelo subdomínio do diretório

#### Registrar a aplicação cliente (ciam-msal-react-spa)

1. No Microsoft Entra admin center, selecione App Registrations → New registration
2. Insira as informações de registro:
   - **Name**: `ciam-msal-react-spa`
   - **Supported account types**: Accounts in this organizational directory only
   - Selecione Register

3. Anote o Application (client) ID
4. Selecione Authentication à esquerda
5. Adicione uma plataforma Single-page application
6. Na seção Redirect URI insira:
   - `http://localhost:3000`
   - `http://localhost:3000/redirect`
7. Clique Save

8. Configure permissões API:
   - Selecione API permissions à esquerda
   - Add a permission → Microsoft APIs → Microsoft Graph
   - Em Delegated permissions, selecione `openid`, `offline_access`
   - Add permissions
   
   - Add a permission → My APIs → selecione `ciam-msal-dotnet-api`
   - Em Delegated permissions, selecione `ToDoList.Read`, `ToDoList.ReadWrite`
   - Add permissions

9. Selecione Grant admin consent para o tenant

##### Configurar a aplicação cliente

1. Abra o arquivo `SPA\src\authConfig.js`
2. Encontre a chave `Enter_the_Application_Id_Here` e substitua pelo Application ID da app `ciam-msal-react-spa`
3. Encontre o placeholder `Enter_the_Tenant_Subdomain_Here` e substitua pelo subdomínio do tenant
4. Encontre a chave `Enter_the_Web_Api_Application_Id_Here` e substitua pelo Application ID da app `ciam-msal-dotnet-api`

### Passo 4: Executar a aplicação

Execute os seguintes comandos em terminais separados:

**Para a API:**
```bash
cd 2-Authorization\1-call-api-react\API\ToDoListAPI
dotnet run
```

**Para o SPA:**
```bash
cd 2-Authorization\1-call-api-react\SPA
npm start
```

## Explorar a aplicação

1. Abra seu navegador e navegue para `http://localhost:3000`
2. Selecione o botão Sign In no canto superior direito
3. Selecione o botão ToDoList na barra de navegação para fazer uma chamada à ToDoListAPI

## Sobre o Código

### Configurações CORS

A política CORS deve ser configurada em `Program.cs` para permitir chamadas à ToDoListAPI:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddCors(o => o.AddPolicy("default", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    }));
}
```

### Validação do Access Token

O método `AddMicrosoftIdentityWebApiAuthentication` em `Program.cs` protege a Web API validando access tokens:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMicrosoftIdentityWebApiAuthentication(Configuration);
}
```

### Verificação de Permissões

Access tokens devem conter as claims `scp` (para permissões delegadas) ou `roles` (para permissões de aplicação). Isso é ilustrado no `ToDoListController.cs`:

```csharp
[HttpGet]
[RequiredScopeOrAppPermission(
    RequiredScopesConfigurationKey = "AzureAD:Scopes:Read",
    RequiredAppPermissionsConfigurationKey = "AzureAD:AppPermissions:Read"
)]
public async Task<IActionResult> GetAsync()
{
    // Lógica do controlador
}
```

### Acesso aos Dados

Os endpoints da Web API devem estar preparados para aceitar chamadas tanto de usuários quanto de aplicações:

```csharp
private bool IsAppMakingRequest()
{
    if (HttpContext.User.Claims.Any(c => c.Type == "idtyp"))
    {
        return HttpContext.User.Claims.Any(c => c.Type == "idtyp" && c.Value == "app");
    }
    else
    {
        return HttpContext.User.Claims.Any(c => c.Type == "roles") && 
               !HttpContext.User.Claims.Any(c => c.Type == "scp");
    }
}
```

## Solução de Problemas

- Use [Stack Overflow](http://stackoverflow.com/questions/tagged/msal) para obter suporte da comunidade
- Marque suas perguntas com [`azure-active-directory` `react` `ms-identity` `adal` `msal`]
- Para bugs, use [GitHub Issues](https://github.com/azure-samples/ms-identity-ciam-javascript-tutorial/issues)

## Recursos Adicionais

- [Configure your company branding](https://learn.microsoft.com/en-us/entra/fundamentals/how-to-customize-branding)
- [OAuth 2.0 authorization with Microsoft Entra ID](https://learn.microsoft.com/en-us/entra/architecture/auth-oauth2)
- [Microsoft.Identity.Web](https://aka.ms/microsoft-identity-web)
- [Validating Access Tokens](https://learn.microsoft.com/en-us/azure/active-directory/develop/access-tokens#validating-tokens)

## Debugging

Para debugar a API .NET Core, instale a [extensão C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp) para Visual Studio Code.

## Estrutura de Arquivos Importante

- `SPA/App.jsx` - Lógica principal da aplicação
- `SPA/hooks/useFetchWithMsal.jsx` - Hook customizado para fazer chamadas com bearer tokens
- `SPA/authConfig.js` - Parâmetros de autenticação para o projeto SPA
- `API/ToDoListAPI/appsettings.json` - Parâmetros de autenticação para a API
- `API/ToDoListAPI/Startup.cs` - Onde Microsoft.Identity.Web é inicializado

---

*Instruções baseadas no tutorial oficial da Microsoft para React SPA com Microsoft Entra External ID*