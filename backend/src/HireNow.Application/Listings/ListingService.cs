using HireNow.Domain;

namespace HireNow.Application.Listings;

public sealed record ListingInput(string Title, string Description, string Category, long MinimumPrice, long MaximumPrice, string? Status = null);

public enum ListingOutcomeKind { Success, NotFound, Forbidden, Validation, NotPublished, ActiveOrders }

public sealed record ListingOutcome(ListingOutcomeKind Kind, Listing? Listing = null, Dictionary<string, string[]>? Errors = null);

public sealed class ListingService(IListingData data, IListingOrders orders)
{
    public Task<IReadOnlyList<Listing>> GetMineAsync(string? userId, CancellationToken cancellationToken) =>
        data.GetBySellerAsync(userId, cancellationToken);

    public Task<ListingOutcome> GetOwnedAsync(int id, string userId, CancellationToken cancellationToken) =>
        FindOwnedAsync(id, userId, cancellationToken);

    public async Task<ListingOutcome> CreateAsync(ListingInput input, string userId, CancellationToken cancellationToken)
    {
        var (errors, category) = await ValidateAsync(input, cancellationToken);

        if (errors.Count > 0)
        {
            return new(ListingOutcomeKind.Validation, Errors: errors);
        }

        var listing = new Listing
        {
            Title = input.Title.Trim(),
            Description = input.Description.Trim(),
            Category = category!,
            MinimumPrice = input.MinimumPrice,
            MaximumPrice = input.MaximumPrice,
            Status = Enum.TryParse<ListingStatus>(input.Status, true, out var status) ? status : ListingStatus.Draft,
            UserId = userId,
        };

        data.Add(listing);
        await data.SaveChangesAsync(cancellationToken);

        return new(ListingOutcomeKind.Success, listing);
    }

    public async Task<ListingOutcome> UpdateAsync(int id, ListingInput input, string userId, CancellationToken cancellationToken)
    {
        var owned = await FindOwnedAsync(id, userId, cancellationToken);
        if (owned.Kind != ListingOutcomeKind.Success)
        {
            return owned;
        }

        var (errors, category) = await ValidateAsync(input, cancellationToken);
        if (errors.Count > 0)
        {
            return new(ListingOutcomeKind.Validation, Errors: errors);
        }

        var listing = owned.Listing!;

        // Orders store their own agreed price, so changing the range here leaves them alone.
        listing.Title = input.Title.Trim();
        listing.Description = input.Description.Trim();
        listing.Category = category!;
        listing.MinimumPrice = input.MinimumPrice;
        listing.MaximumPrice = input.MaximumPrice;

        await data.SaveChangesAsync(cancellationToken);
        return new(ListingOutcomeKind.Success, listing);
    }

    public async Task<ListingOutcome> PublishAsync(int id, string userId, CancellationToken cancellationToken)
    {
        var owned = await FindOwnedAsync(id, userId, cancellationToken);

        if (owned.Kind != ListingOutcomeKind.Success)
        {
            return owned;
        }

        owned.Listing!.Publish();
        await data.SaveChangesAsync(cancellationToken);
        return owned;
    }

    public async Task<ListingOutcome> UnpublishAsync(int id, string userId, CancellationToken cancellationToken)
    {
        var owned = await FindOwnedAsync(id, userId, cancellationToken);

        if (owned.Kind != ListingOutcomeKind.Success)
        {
            return owned;
        }

        if (!owned.Listing!.TryUnpublish())
        {
            return new(ListingOutcomeKind.NotPublished);
        }

        await data.SaveChangesAsync(cancellationToken);
        return owned;
    }

    public async Task<ListingOutcome> DeleteAsync(int id, string userId, CancellationToken cancellationToken)
    {
        var owned = await FindOwnedAsync(id, userId, cancellationToken);

        if (owned.Kind != ListingOutcomeKind.Success)
        {
            return owned;
        }

        if (!Listing.CanBeDeleted(await orders.HasActiveOrdersAsync(id, cancellationToken)))
        {
            return new(ListingOutcomeKind.ActiveOrders);
        }

        data.Remove(owned.Listing!);
        await data.SaveChangesAsync(cancellationToken);
        return new(ListingOutcomeKind.Success);
    }

    private async Task<ListingOutcome> FindOwnedAsync(int id, string userId, CancellationToken cancellationToken)
    {
        var listing = await data.FindAsync(id, cancellationToken);

        if (listing is null)
        {
            return new(ListingOutcomeKind.NotFound);
        }

        return listing.IsOwnedBy(userId) ? new(ListingOutcomeKind.Success, listing) : new(ListingOutcomeKind.Forbidden);
    }

    private async Task<(Dictionary<string, string[]> Errors, Category? Category)> ValidateAsync(ListingInput input, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();

        var title = input.Title?.Trim() ?? string.Empty;

        if (title.Length == 0)
        {
            errors["title"] = ["Title is required."];
        }

        else if (title.Length > 100)
        {
            errors["title"] = ["Title must be 100 characters or fewer."];
        }

        var description = input.Description?.Trim() ?? string.Empty;
        if (description.Length == 0)
        {
            errors["description"] = ["Description is required."];
        }
        else if (description.Length > 5000)
        {
            errors["description"] = ["Description must be 5000 characters or fewer."];
        }

        Category? category = null;
        var categoryName = input.Category?.Trim() ?? string.Empty;
        if (categoryName.Length == 0)
        {
            errors["category"] = ["Category is required."];
        }
        else
        {
            category = await data.FindCategoryAsync(categoryName, cancellationToken);
            if (category is null)
            {
                errors["category"] = ["Choose a valid category."];
            }
        }

        if (input.MinimumPrice <= 0)
        {
            errors["minimumPrice"] = ["Minimum price must be greater than 0."];
        }

        if (input.MaximumPrice <= 0)
        {
            errors["maximumPrice"] = ["Maximum price must be greater than 0."];
        }
        else if (input.MinimumPrice > 0 && !Listing.IsValidPriceRange(input.MinimumPrice, input.MaximumPrice))
        {
            errors["minimumPrice"] = ["Minimum price can't exceed maximum price."];
        }

        return (errors, category);
    }
}
