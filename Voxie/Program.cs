// File: ServerApp/Program.cs

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

// Create a WebApplication builder to configure the application
var builder = WebApplication.CreateBuilder(args);

// Build the application
var app = builder.Build();

// Configure WebSocket options
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120), // Set the interval for WebSocket keep-alive messages
};
app.UseWebSockets(webSocketOptions); // Enable WebSocket support in the application

// Initialize the WebSocketHandler (assumed to handle WebSocket connections)
var webSocketHandler = new WebSocketHandler();

/// <summary>
/// Validates a JWT token and extracts the claims principal if valid.
/// <param name="token">The JWT token to validate.</param>
/// <param name="claimsPrincipal">The extracted claims principal if the token is valid.</param>
/// <returns>True if the token is valid, otherwise false.</returns>
bool ValidateToken(string token, out ClaimsPrincipal? claimsPrincipal)
{
    // Retrieve the secret key from environment variables
    var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
    if (string.IsNullOrEmpty(secretKey))
    {
        throw new InvalidOperationException("JWT secret key is not configured.");
    }

    // Convert the secret key to a byte array
    var key = Encoding.UTF8.GetBytes(secretKey);

    // Create a token handler to validate the token
    var tokenHandler = new JwtSecurityTokenHandler();
    try
    {
        // Validate the token using the provided parameters
        claimsPrincipal = tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true, // Ensure the signing key is valid
            IssuerSigningKey = new SymmetricSecurityKey(key), // Use the secret key for validation
            ValidateIssuer = false, // Skip issuer validation
            ValidateAudience = false, // Skip audience validation
            ClockSkew = TimeSpan.Zero // Disable clock skew for token expiration
        }, out SecurityToken validatedToken);

        return true; // Token is valid
    }
    catch
    {
        claimsPrincipal = null; // Token is invalid
        return false;
    }
}

// Map the WebSocket endpoint at "/ws"
app.Map("/ws", async context =>
{
    // Check if the request is a WebSocket request
    if (context.WebSockets.IsWebSocketRequest)
    {
        // Retrieve the token from the query string
        var token = context.Request.Query["token"];
        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 400; // Bad Request if token is missing
            return;
        }

        // Validate the token
        if (string.IsNullOrEmpty(token) || !ValidateToken(token, out var claimsPrincipal))
        {
            context.Response.StatusCode = 401; // Unauthorized if token is invalid
            return;
        }

        // Extract the username from the claims
        var username = claimsPrincipal?.Identity?.Name;
        if (string.IsNullOrEmpty(username))
        {
            Console.WriteLine("Username could not be extracted from the token.");
            context.Response.StatusCode = 401; // Unauthorized if username is missing
            return;
        }
        Console.WriteLine($"Authenticated user: {username}");

        // Accept the WebSocket connection
        var socket = await context.WebSockets.AcceptWebSocketAsync();
        var connectionId = Guid.NewGuid().ToString(); // Generate a unique connection ID
        webSocketHandler.AddSocket(connectionId, socket); // Add the socket to the handler

        // Handle the WebSocket connection
        await webSocketHandler.HandleConnectionAsync(socket, connectionId);
    }
    else
    {
        context.Response.StatusCode = 400; // Bad Request if not a WebSocket request
    }
});

/// <summary>
/// Generates a JWT token for a given username.
/// </summary>
/// <param name="username">The username to include in the token.</param>
/// <returns>A signed JWT token.</returns>
string GenerateJwtToken(string username)
{
    // Retrieve the secret key from environment variables
    var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET_KEY");
    if (string.IsNullOrEmpty(secretKey))
    {
        throw new InvalidOperationException("JWT secret key is not configured.");
    }

    // Convert the secret key to a byte array
    var key = Encoding.UTF8.GetBytes(secretKey);

    // Create a token handler to generate the token
    var tokenHandler = new JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, username) // Add the username as a claim
        }),
        Expires = DateTime.UtcNow.AddHours(1), // Set the token expiration time
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature) // Sign the token
    };

    // Create and return the token
    var token = tokenHandler.CreateToken(tokenDescriptor);
    return tokenHandler.WriteToken(token);
}

// Uncomment the following line for debugging purposes only
// Console.WriteLine("Example JWT Token: " + GenerateJwtToken("testuser"));

// Start the application
app.Run();
