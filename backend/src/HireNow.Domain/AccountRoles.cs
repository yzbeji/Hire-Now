namespace HireNow.Domain;

public static class AccountRoles
{
    public const string Seller = "Seller";
    public const string Buyer = "Buyer";
    public const string Admin = "Admin";

    public static readonly string[] All = [Seller, Buyer, Admin];

    /// <summary>The roles users may choose for themselves. Admin can only be assigned in Keycloak.</summary>
    public static readonly string[] SelfService = [Seller, Buyer];
}
