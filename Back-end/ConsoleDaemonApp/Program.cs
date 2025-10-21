using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;

namespace ConsoleApp;

class Program
{
    private static IConfiguration? _configuration;
    private static HttpClient? _httpClient;

    static async Task Main(string[] args)
    {
        // Load configuration
        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        _httpClient = new HttpClient();

        try
        {
            // Step 1: Acquire access token using Client Credentials
            string accessToken = await AcquireTokenAsync();

            // Step 2: Call TodoList API
            await CallTodoListApiAsync(accessToken);
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n? Error: {ex.Message}");
            Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");
            Console.ResetColor();
        }

        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    /// <summary>
    /// Acquires an access token using the Client Credentials flow (daemon/service authentication).
    /// This uses application permissions (roles) instead of delegated permissions (scopes).
    /// </summary>
    private static async Task<string> AcquireTokenAsync()
    {
        var instance = _configuration["AzureAd:Instance"];
        var tenantId = _configuration["AzureAd:TenantId"];
        var clientId = _configuration["AzureAd:ClientId"];
        var clientSecret = _configuration["AzureAd:ClientSecret"];
        var scopes = _configuration.GetSection("ToDoListApi:Scopes").Get<string[]>();

        if (string.IsNullOrEmpty(instance) || string.IsNullOrEmpty(tenantId) ||
            string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) ||
            scopes == null || scopes.Length == 0)
        {
            throw new InvalidOperationException(
                "Configuration is incomplete. Please check appsettings.json and ensure all values are set.");
        }

        var authority = $"{instance}{tenantId}";

        // Build the confidential client application
        var app = ConfidentialClientApplicationBuilder.Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority(new Uri(authority))
            .Build();

        // Acquire token for the configured scopes
        var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

        return result.AccessToken;
    }

    /// <summary>
    /// Calls the TodoList API GET endpoint to retrieve all todos.
    /// This demonstrates a daemon application calling a protected API using app permissions.
    /// </summary>
    private static async Task CallTodoListApiAsync(string accessToken)
    {
        Console.WriteLine("Calling TodoList API...");

        var baseUrl = _configuration["ToDoListApi:BaseUrl"];
        var endpoint = $"{baseUrl}/ToDoList";

        Console.WriteLine($"Endpoint: {endpoint}\n");

        // Set authorization header
        _httpClient!.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        // Call the API
        var response = await _httpClient.GetAsync(endpoint);

        Console.WriteLine($"Response Status: {(int)response.StatusCode} {response.StatusCode}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nSuccess! TodoList API Response:");
            Console.ResetColor();
            Console.WriteLine("==============================================");
            
            // Pretty print JSON
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(content);
                var formatted = System.Text.Json.JsonSerializer.Serialize(
                    json,
                    new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
                Console.WriteLine(formatted);
            }
            catch
            {
                Console.WriteLine(content);
            }
            
            Console.WriteLine("==============================================");
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nAPI call failed!");
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Response: {errorContent}");
            Console.ResetColor();
        }
    }
}
