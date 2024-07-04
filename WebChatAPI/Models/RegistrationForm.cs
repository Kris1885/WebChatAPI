namespace WebChatAPI.Models;

public class RegistrationForm
{
    public required string Login { get; set; }
    public required string Password { get; set; }
    public required string Name { get; set; }
}