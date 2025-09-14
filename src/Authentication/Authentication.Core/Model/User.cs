using Shared.Core.Model;

namespace Authentication.Core.Model;

public class User : BaseModel
{
    public string Name { get; init; }
    
    public string Email { get; init; }
    
    public string Password { get; set; }

    public User() { }

    public User(string name, string email, string password)
    {
        Name = name;
        Email = email;
        Password = BCrypt.Net.BCrypt.HashPassword(password);
    }
    
    public void SetPassword(string newPassword)
    {
        Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
    }

    public bool ValidatePassword(string password)
    {
        return BCrypt.Net.BCrypt.Verify(password, Password);
    }
}