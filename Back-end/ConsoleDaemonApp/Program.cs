using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
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
        // Load configuration from appsettings.json and User Secrets
        _configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddUserSecrets(Assembly.GetExecutingAssembly(), optional: true)
            .Build();

        _httpClient = new HttpClient();

        Console.WriteLine("==============================================");
        Console.WriteLine("Console Daemon - TodoList API Client");
        Console.WriteLine("Auto-refresh every 10 seconds");
        Console.WriteLine("Press 'Q' to quit");
        Console.WriteLine("==============================================\n");

        // Start the loop
        await RunPollingLoopAsync();
    }

    /// <summary>
    /// Runs a polling loop that calls the API every 10 seconds.
    /// </summary>
    private static async Task RunPollingLoopAsync()
    {
        var keepRunning = true;
        var pollingInterval = TimeSpan.FromSeconds(10);

        // Start a background task to listen for 'Q' key press
        var cancellationTask = Task.Run(() =>
        {
            while (keepRunning)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Q)
                    {
                        keepRunning = false;
                        Console.WriteLine("\n\n[Exiting application...]");
                    }
                }
                Task.Delay(100).Wait();
            }
        });

        while (keepRunning)
        {
            try
            {
                // Clear console and show header
                Console.Clear();
                Console.WriteLine("==============================================");
                Console.WriteLine("Console Daemon - TodoList API Client");
                Console.WriteLine($"Last refresh: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                Console.WriteLine("Press 'Q' to quit");
                Console.WriteLine("==============================================\n");

                // Step 1: Acquire access token using Client Credentials
                string accessToken = await AcquireTokenAsync();

                // Step 2: Call TodoList API
                await CallTodoListApiAsync(accessToken);

                // Show next refresh time
                Console.WriteLine($"\n[Next refresh in 10 seconds at {DateTime.Now.AddSeconds(10):HH:mm:ss}]");
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n[ERROR] {ex.Message}");
                Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");
                Console.ResetColor();
                Console.WriteLine($"\n[Retrying in 10 seconds...]");
            }

            // Wait for 10 seconds or until 'Q' is pressed
            await Task.Delay(pollingInterval);
        }

        await cancellationTask;
    }

    /// <summary>
    /// Acquires an access token using the Client Credentials flow (daemon/service authentication).
    /// This uses application permissions (roles) instead of delegated permissions (scopes).
    /// </summary>
    private static async Task<string> AcquireTokenAsync()
    {
        Console.WriteLine("[AUTH] Acquiring access token...");

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

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("[SUCCESS] Access token acquired successfully!");
        Console.ResetColor();
        Console.WriteLine($"   Token expires: {result.ExpiresOn:yyyy-MM-dd HH:mm:ss}\n");

        return result.AccessToken;
    }

    /// <summary>
    /// Calls the TodoList API GET endpoint to retrieve all todos.
    /// This demonstrates a daemon application calling a protected API using app permissions.
    /// </summary>
    private static async Task CallTodoListApiAsync(string accessToken)
    {
        Console.WriteLine("[API] Calling TodoList API...");

        var baseUrl = _configuration["ToDoListApi:BaseUrl"];
        var endpoint = $"{baseUrl}/ToDoList";

        Console.WriteLine($"   Endpoint: {endpoint}\n");

        // Set authorization header
        _httpClient!.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        // Call the API
        var response = await _httpClient.GetAsync(endpoint);

        Console.WriteLine($"[RESPONSE] Status: {(int)response.StatusCode} {response.StatusCode}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n[SUCCESS] TodoList API Response:");
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
            Console.WriteLine($"\n[FAILED] API call failed!");
            Console.WriteLine($"Status: {response.StatusCode}");
            Console.WriteLine($"Response: {errorContent}");
            Console.ResetColor();
        }
    }
}
