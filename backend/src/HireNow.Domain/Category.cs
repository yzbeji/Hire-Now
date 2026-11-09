namespace HireNow.Domain;

public class Category
{
    public int Id { get; set; }
    public required string Name { get; set; }
    
    static public Category Other { get; } = new Category { Id = -1, Name = "Other" };
}


