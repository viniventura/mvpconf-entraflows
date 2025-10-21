using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

using ToDoListAPI.Context;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

// Configure Authentication with OBO support for calling downstream APIs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(options =>
            {
                builder.Configuration.Bind("AzureAd", options);
                options.Events = new JwtBearerEvents();

                /// <summary>
                /// Below you can do extended token validation and check for additional claims, such as:
                ///
                /// - check if the caller's tenant is in the allowed tenants list via the 'tid' claim (for multi-tenant applications)
                /// - check if the caller's account is homed or guest via the 'acct' optional claim
                /// - check if the caller belongs to right roles or groups via the 'roles' or 'groups' claim, respectively
                ///
                /// Bear in mind that you can do any of the above checks within the individual routes and/or controllers as well.
                /// For more information, visit: https://docs.microsoft.com/azure/active-directory/develop/access-tokens#validate-the-user-has-permission-to-access-this-data
                /// </summary>

                //options.Events.OnTokenValidated = async context =>
                //{
                //    string[] allowedClientApps = { /* list of client ids to allow */ };

                //    string clientappId = context?.Principal?.Claims
                //        .FirstOrDefault(x => x.Type == "azp" || x.Type == "appid")?.Value;

                //    if (!allowedClientApps.Contains(clientappId))
                //    {
                //        throw new System.Exception("This client is not authorized");
                //    }
                //};
            }, options => { builder.Configuration.Bind("AzureAd", options); })
            .EnableTokenAcquisitionToCallDownstreamApi(options =>
            {
                builder.Configuration.Bind("AzureAd", options);
            })
            .AddInMemoryTokenCaches();

// Register Downstream APIs (example: Graph API or your own protected API)
// The downstream API can be Microsoft Graph or any other API protected by Azure AD
if (builder.Configuration.GetSection("DownstreamApis:GraphApi").Exists())
{
    builder.Services.AddDownstreamApi("GraphApi", 
        builder.Configuration.GetSection("DownstreamApis:GraphApi"));
}


builder.Services.AddDbContext<ToDoContext>(options =>
{
    options.UseInMemoryDatabase("ToDos");
});

builder.Services.AddControllers();

// Allowing CORS for all domains and HTTP methods for the purpose of the sample
// In production, modify this with the actual domains and HTTP methods you want to allow
builder.Services.AddCors(o => o.AddPolicy("default", builder =>
{
    builder.AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader();
}));

// Configure Swagger with OAuth2 Authorization Code Flow
builder.Services.AddEndpointsApiExplorer();

// Get scopes from configuration (declare before AddSwaggerGen to use in SwaggerUI later)
var todoListReadScope = builder.Configuration["Swagger:Scopes:ToDoList:Read"];
var todoListReadWriteScope = builder.Configuration["Swagger:Scopes:ToDoList:ReadWrite"];
var userReadScope = builder.Configuration["Swagger:Scopes:User:Read"];

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ToDoList API",
        Version = "v1",
        Description = "An API to manage ToDo items with Azure AD authentication and OBO support for downstream APIs"
    });

    var tenantId = builder.Configuration["AzureAd:TenantId"];
    var instance = builder.Configuration["AzureAd:Instance"];
    var authorizationUrl = $"{instance}{tenantId}/oauth2/v2.0/authorize";
    var tokenUrl = $"{instance}{tenantId}/oauth2/v2.0/token";

    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri(authorizationUrl),
                TokenUrl = new Uri(tokenUrl),
                Scopes = new Dictionary<string, string>
                {
                    { todoListReadScope ?? $"api://{builder.Configuration["AzureAd:ClientId"]}/ToDoList.Read", "Read access to ToDo items" },
                    { todoListReadWriteScope ?? $"api://{builder.Configuration["AzureAd:ClientId"]}/ToDoList.ReadWrite", "Read and write access to ToDo items" },
                    { userReadScope ?? $"api://{builder.Configuration["AzureAd:ClientId"]}/User.Read", "Read user profile from Microsoft Graph" }
                }
            }
        }
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { 
                todoListReadScope ?? $"api://{builder.Configuration["AzureAd:ClientId"]}/ToDoList.Read", 
                todoListReadWriteScope ?? $"api://{builder.Configuration["AzureAd:ClientId"]}/ToDoList.ReadWrite",
                userReadScope ?? $"api://{builder.Configuration["AzureAd:ClientId"]}/User.Read"
            }
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Since IdentityModel version 5.2.1 (or since Microsoft.AspNetCore.Authentication.JwtBearer version 2.2.0),
    // Personal Identifiable Information is not written to the logs by default, to be compliant with GDPR.
    // For debugging/development purposes, one can enable additional detail in exceptions by setting IdentityModelEventSource.ShowPII to true.
    // Microsoft.IdentityModel.Logging.IdentityModelEventSource.ShowPII = true;
    app.UseDeveloperExceptionPage();
    
    // Enable Swagger in development
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ToDoList API V1");
        options.OAuthClientId(builder.Configuration["Swagger:ClientId"]);
        options.OAuthUsePkce();
        
        // Configure default scopes for Swagger UI
        var defaultScopes = new List<string>();
        if (!string.IsNullOrEmpty(todoListReadScope)) defaultScopes.Add(todoListReadScope);
        if (!string.IsNullOrEmpty(todoListReadWriteScope)) defaultScopes.Add(todoListReadWriteScope);
        if (!string.IsNullOrEmpty(userReadScope)) defaultScopes.Add(userReadScope);
        
        options.OAuthScopes(defaultScopes.ToArray());
    });
}
else
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors("default");
app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
