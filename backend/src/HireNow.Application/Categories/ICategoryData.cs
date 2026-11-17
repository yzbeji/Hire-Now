using HireNow.Domain;

namespace HireNow.Application.Categories;

public interface ICategoryData
{
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken cancellationToken);
}
