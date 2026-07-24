namespace Clausio.Legal.Core.Dtos;

public class RegisterDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
    public string? Phone { get; set; }
}

public class LoginDto
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class AuthResponseDto
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
}
