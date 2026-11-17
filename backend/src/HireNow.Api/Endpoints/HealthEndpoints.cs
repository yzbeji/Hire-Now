namespace HireNow.Api.Endpoints;

internal static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder app) =>
        app.MapGet("/health", () => TypedResults.Ok(new { Status = "Healthy" }));
}
