namespace BankApi.Services;
using Microsoft.AspNetCore.Identity;

public class PasswordService
{
    private readonly IPasswordHasher<string> _hasher;

    public PasswordService()
    {
        _hasher = new PasswordHasher<string>();
    }

    public string HashPassword(string password)
    {
        return _hasher.HashPassword(null!, password);
    }
    
    public bool VerifyPassword(string hashedPassword, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(null!, hashedPassword, providedPassword);
        return result == PasswordVerificationResult.Success;
    }
}
