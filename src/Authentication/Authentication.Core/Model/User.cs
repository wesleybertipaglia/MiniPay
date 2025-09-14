using Shared.Core.Model;

namespace Authentication.Core.Model;

public class User : BaseModel
{
    public string Name { get; set; }
    
    public string Email { get; set; }
    
    public string Password { get; set; }

    public User() { }

    public User(string name, string email)
    {
        Name = name;
        Email = email;
    }

    public void SetPassword(string hashedPassword)
    {
        Password = hashedPassword;
    }
}