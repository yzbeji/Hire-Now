using System.ComponentModel.DataAnnotations;

namespace HireNow.Domain;

public enum ListingStatus
{
    Draft = 0,
    Published = 1,
    // Hidden from search after being live. Kept so existing orders still point at it.
    Unpublished = 2,
}

public class Listing
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public required string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(5000)]
    public string Description { get; set; } = string.Empty;

    public Category Category { get; set; } = Category.Other;

    [Required]
    public long MinimumPrice { get; set; } = 0;

    [Required]
    public long MaximumPrice { get; set; } = 0;

    [Required]
    public ListingStatus Status { get; set; } = ListingStatus.Draft;

    //to do images

    [Required]
    public required string UserId { get; set; }

    public bool IsOwnedBy(string userId) => UserId == userId;

    /// <summary>Makes a draft or unpublished listing visible in search again. Publishing a live listing changes nothing.</summary>
    public void Publish() => Status = ListingStatus.Published;

    /// <summary>Hides a live listing from search. Returns false if it isn't published.</summary>
    public bool TryUnpublish()
    {
        if (Status != ListingStatus.Published)
        {
            return false;
        }

        Status = ListingStatus.Unpublished;
        return true;
    }

    /// <summary>A listing with active orders can't be deleted. The seller has to unpublish it instead.</summary>
    public static bool CanBeDeleted(bool hasActiveOrders) => !hasActiveOrders;

    public static bool IsValidPriceRange(long minimumPrice, long maximumPrice)
    {
        return minimumPrice > 0 && maximumPrice > 0 && minimumPrice <= maximumPrice;
    }
}
