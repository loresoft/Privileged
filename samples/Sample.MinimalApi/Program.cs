using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using Privileged.Authorization;
using Privileged.Endpoint;

namespace Sample.MinimalApi;

public class Program
{
    private const string JwtKey = "YourSuperSecretKey1234567890123456"; // 256-bit HmacSha256 compatible key

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add authentication services with default scheme set to JwtBearer
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = "Sample.MinimalApi",
                ValidAudience = "Sample.MinimalApi",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey))
            };
        });
        builder.Services.AddAuthorization();

        // Add Privileged authorization and register a sample context provider
        builder.Services.AddPrivilegeAuthorization<SamplePrivilegeContextProvider>();

        var app = builder.Build();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        // Public endpoint (no privileges required)
        app.MapGet(
            pattern: "/",
            handler: () => Results.Ok(new { message = "Welcome to the Privileged Minimal API sample" })
        );

        // Login endpoint to generate JWT
        app.MapPost("/login", (UserLoginRequest request) =>
        {
            if (request.Username == "testuser" && request.Password == "password")
            {
                var claims = new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, request.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                };

                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var token = new JwtSecurityToken(
                    issuer: "Sample.MinimalApi",
                    audience: "Sample.MinimalApi",
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(30),
                    signingCredentials: creds
                );

                return Results.Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
            }

            return Results.Unauthorized();
        });

        // Endpoint requiring the 'read' privilege on 'Post'
        app.MapGet(
            pattern: "/posts",
            handler: [Privilege("read", "Post")] () => Results.Ok(new[]
            {
                new PostDto(1, "Hello World", "Sample content"),
                new PostDto(2, "Second Post", "Another example")
            })
        );

        // Endpoint requiring the 'update' privilege on 'Post' with qualifier 'title'
        app.MapPut(
            pattern: "/posts/{id:int}/title",
            handler: ([FromRoute] int id, [FromBody] UpdateTitleRequest request)
                => Results.Ok(new { id, request.Title, updated = true })
        ).RequirePrivilege("update", "Post", "title");

        // Endpoint requiring an ungranted privilege to show forbidden behavior
        app.MapDelete(
            pattern: "/posts/{id:int}",
            handler: [Privilege("delete", "Post")] ([FromRoute] int id)
                => Results.NoContent()
        );

        app.Run();
    }

    public record PostDto(int Id, string Title, string Content);
    public record UpdateTitleRequest(string Title);
    public record UserLoginRequest(string Username, string Password);
}
