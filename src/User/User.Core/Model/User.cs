using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Model;

namespace User.Core.Model;

[Index(nameof(Code), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
public class User : BaseModel
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [MaxLength(255)]
    [EmailAddress]
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

    public void ConfirmEmail()
    {
        EmailConfirmed = true;
        Touch();
    }
}