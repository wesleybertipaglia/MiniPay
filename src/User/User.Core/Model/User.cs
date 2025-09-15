using Microsoft.EntityFrameworkCore;
using Shared.Core.Model;

namespace User.Core.Model;

[Index(nameof(Code), IsUnique = true)]
public class User : BaseModel
{
    public string Name { get; set; }
    
    public string Email { get; set; }

    public bool EmailConfirmed { get; set; }

    public User() { }

    public User(Guid id, string code, string name, string email)
    {
        Id = id;
        Code = code;
        Name = name;
        Email = email;
    }
}