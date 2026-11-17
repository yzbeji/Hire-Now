using HireNow.Api.Endpoints;
using HireNow.Api.Hubs;
using HireNow.Application.Categories;
using HireNow.Application.Chat;
using HireNow.Application.Listings;
using HireNow.Application.Profiles;
using HireNow.Application.PublicServices;
using HireNow.Domain;
using HireNow.Infrastructure;
using HireNow.Infrastructure.Development;
using HireNow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ListingService>();
builder.Services.AddScoped<ProfileService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<PublicServiceService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, KeycloakUserIdProvider>();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHostedService<DevelopmentDataSeeder>();
}
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));

var keycloakAuthority = builder.Configuration["Keycloak:Authority"]
    ?? throw new InvalidOperationException("Keycloak:Authority is not configured.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.Authority = keycloakAuthority;
    options.Audience = "hirenow-api";
    options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    options.MapInboundClaims = false;
    options.TokenValidationParameters.NameClaimType = "preferred_username";
    options.TokenValidationParameters.RoleClaimType = "roles";
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            // Browser WebSocket and SSE clients send JWTs in the query string.
            if (context.HttpContext.Request.Path.StartsWithSegments(ChatHub.Path)
                && context.Request.Query.TryGetValue("access_token", out var token))
            {
                context.Token = token;
            }

            return Task.CompletedTask;
        },
    };
});

var authorization = builder.Services.AddAuthorizationBuilder();
foreach (var role in AccountRoles.All)
{
    authorization.AddPolicy(role, policy => policy.RequireRole(role));
}

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "HireNow API"));

    app.Services.MigrateDatabase();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapChatEndpoints();
app.MapHub<ChatHub>(ChatHub.Path, options => options.CloseOnAuthenticationExpiration = true)
    .RequireAuthorization(policy => policy.RequireClaim("sub"));

app.MapHealthEndpoints();
app.MapAccountEndpoints();
app.MapProfileEndpoints();
app.MapCategoryEndpoints();
app.MapListingEndpoints();
app.MapPublicServiceEndpoints();

app.Run();
