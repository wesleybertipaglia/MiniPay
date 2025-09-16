using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using Shared.Core.Model;

namespace Authentication.Core.Model;

[Index(nameof(Email), IsUnique = true)]
[Index(nameof(Code), IsUnique = true)]
public class User : BaseModel
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    [MaxLength(255)]
    [EmailAddress]
    public string Email { get; private set; }

    [Required]
    [MaxLength(255)]
    public string Password { get; private set; }
    
    public User() {}

    public User(string name, string email, string password)
    {
        Name = name;
        Email = email;
        Password = HashPassword(password);
    }
    
    public void SetEmail(string email)
    {
        Email = email;
    }

    public void SetPassword(string password)
    {
        Password = HashPassword(password);
    }

    public bool ValidatePassword(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, Password);
    }

    private static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
}