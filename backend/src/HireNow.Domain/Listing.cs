using System.ComponentModel.DataAnnotations;

namespace HireNow.Domain;

public enum ListingStatus
{
    Draft = 0,
    Published = 1,
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

    public static bool IsValidPriceRange(long minimumPrice, long maximumPrice)
    {
        return minimumPrice > 0 && maximumPrice > 0 && minimumPrice <= maximumPrice;
    }
}