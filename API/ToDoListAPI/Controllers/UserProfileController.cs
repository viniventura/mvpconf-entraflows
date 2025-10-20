using System;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Abstractions;
using Microsoft.Identity.Web.Resource;

namespace ToDoListAPI.Controllers;

/// <summary>
/// Controller that demonstrates calling downstream APIs using On-Behalf-Of (OBO) flow.
/// 
/// This controller shows how ToDoListAPI can act as a middle-tier API that:
/// 1. Receives a token from the client (SPA)
/// 2. Validates the token
/// 3. Uses OBO to get a new token for downstream APIs
/// 4. Calls the downstream API on behalf of the user
/// 5. Returns the response to the client
/// </summary>
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class UserProfileController : ControllerBase
{
    private readonly IDownstreamApi _downstreamApi;

    public UserProfileController(IDownstreamApi downstreamApi)
    {
        _downstreamApi = downstreamApi;
    }

    /// <summary>
    /// Gets the user profile from Microsoft Graph API using OBO flow.
    /// 
    /// Flow:
    /// 1. Client calls this endpoint with a token (scope: ToDoList.Read)
    /// 2. This API validates the token
    /// 3. This API uses OBO to exchange the token for a Graph token (scope: User.Read)
    /// 4. This API calls Graph API with the new token
    /// 5. Returns the user profile to the client
    /// </summary>
    /// <returns>User profile from Microsoft Graph</returns>
    [HttpGet("me")]
    [RequiredScopeOrAppPermission(
        RequiredScopesConfigurationKey = "AzureAd:Scopes:User:Read"
    )]
    public async Task<IActionResult> GetUserProfile()
    {
        try
        {
            // Call Microsoft Graph API on behalf of the user
            // BaseUrl: https://graph.microsoft.com/v1.0
            // RelativePath: me
            // Full URL: https://graph.microsoft.com/v1.0/me
            var response = await _downstreamApi.CallApiForUserAsync("GraphApi", 
                options =>
                {
                    options.RelativePath = "me";
                }).ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return Problem(
                    detail: $"Graph API call failed with status code: {response.StatusCode}",
                    statusCode: (int)response.StatusCode,
                    title: "Failed to retrieve user profile");
            }

            var userProfile = await response.Content.ReadFromJsonAsync<JsonDocument>().ConfigureAwait(false);

            return Ok(userProfile);
        }
        catch (Exception ex)
        {
            return Problem(
                detail: ex.Message,
                title: "Error calling Microsoft Graph API",
                statusCode: 500);
        }
    }
}

