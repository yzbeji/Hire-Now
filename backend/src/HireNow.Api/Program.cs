using HireNow.Infrastructure;
using HireNow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy
    .WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
    .AllowAnyHeader()
    .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "HireNow API"));

    using var scope = app.Services.CreateScope();
    scope.ServiceProvider.GetRequiredService<HireNowDbContext>().Database.Migrate();
}

app.UseCors();

app.MapGet("/health", () => TypedResults.Ok(new { Status = "Healthy" }));

app.Run();
