using HireNow.Application.Categories;
using HireNow.Domain;
using Microsoft.EntityFrameworkCore;

namespace HireNow.Infrastructure.Persistence;

internal sealed class CategoryData(HireNowDbContext db) : ICategoryData
{
    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        return await db.Categories
            .Where(c => c.Name != Category.Other.Name)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}
