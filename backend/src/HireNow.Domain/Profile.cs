using System.ComponentModel.DataAnnotations;

namespace HireNow.Domain;

public class Profile
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string DisplayName { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Bio { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Location { get; set; } = string.Empty;

    [MaxLength(2048)]
    public string ProfilePhoto { get; set; } = string.Empty;

    [Required]
    public required string UserId { get; set; }
}