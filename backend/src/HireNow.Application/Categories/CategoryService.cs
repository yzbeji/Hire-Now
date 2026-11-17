using HireNow.Domain;

namespace HireNow.Application.Categories;

public sealed class CategoryService(ICategoryData data)
{
    public Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        return data.GetCategoriesAsync(cancellationToken);
    }
}
