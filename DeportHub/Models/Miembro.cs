namespace DeportHub.Models;

public class Miembro
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime BirthDate { get; set; } = DateTime.UtcNow;
    public string County {get; set;} = string.Empty;
    public string Bio {get; set;} = string.Empty;
    public string Role {get; set;} = string.Empty;
    public DateTime CreatedAt {get; set;} = DateTime.Now;
    public string UserId {get; set;} = string.Empty;
    public string PasswordHash {get; set;} = string.Empty;
    
}