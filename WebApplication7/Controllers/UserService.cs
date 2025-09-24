using System.Runtime.CompilerServices;

namespace WebApplication7.Controllers;

public class UserService : IUserService
{
   

    public void Register ( string username)
    {
        Console.WriteLine($"Registering user: {username}");
    }
}