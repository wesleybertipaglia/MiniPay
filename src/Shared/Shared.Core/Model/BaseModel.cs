namespace Shared.Core.Model;

public abstract class BaseModel
{
    public Guid Id { get; set; }
    
    public string Code { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    protected BaseModel()
    {
        var id = Guid.NewGuid();
        Id = id;
        Code = GenerateCodeFromGuid(id);
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Touch()
    {
        UpdatedAt = DateTime.UtcNow;
    }
    
    private static string GenerateCodeFromGuid(Guid id)
    {
        return id.ToString("N")[..8].ToLower();
    }
}