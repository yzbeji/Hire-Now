using HireNow.Application.Categories;

namespace HireNow.Api.Endpoints;

internal static class CategoryEndpoints
{
    public static void MapCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/categories", async (CategoryService categories, CancellationToken cancellationToken) =>
            TypedResults.Ok((await categories.GetCategoriesAsync(cancellationToken)).Select(c => new { c.Id, c.Name })));
    }
}
