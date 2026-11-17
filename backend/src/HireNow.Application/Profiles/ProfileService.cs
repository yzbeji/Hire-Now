using HireNow.Domain;

namespace HireNow.Application.Profiles;

public sealed class ProfileService(IProfileData data)
{
    public Task<Profile?> FindAsync(string userId, CancellationToken cancellationToken)
    {
        return data.FindAsync(userId, cancellationToken);
    }

    public async Task<Profile> UpdateCurrentAsync(string userId, string displayName, string? bio, string? location, 
                                                  string? profilePhoto, CancellationToken cancellationToken)
    {
        var profile = await data.FindAsync(userId, cancellationToken);
        if (profile is null)
        {
            profile = new Profile
            {
                UserId = userId,
                DisplayName = displayName.Trim(),
                Bio = bio ?? string.Empty,
                Location = location ?? string.Empty,
                ProfilePhoto = profilePhoto ?? string.Empty,
            };
            data.Add(profile);
        }
        else
        {
            profile.DisplayName = displayName.Trim();
            profile.Bio = bio ?? string.Empty;
            profile.Location = location ?? string.Empty;
            profile.ProfilePhoto = profilePhoto ?? string.Empty;
        }

        await data.SaveChangesAsync(cancellationToken);
        return profile;
    }
}
