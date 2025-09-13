using Shared.Core.Model;

namespace User.Core.Model;

public class User : BaseModel
{
    public string Name { get; set; }
    
    public string Email { get; set; }

    public string Password { get; set; }

    public bool EmailConfirmed { get; set; } = false;
}