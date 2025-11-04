namespace BankApi.Services;
using BankApi.DTOs;
using BankApi.Entities;


public class AuthService
{
    private readonly ClientService _clientService;
    private readonly PasswordService _passwordService;
    private readonly TokenService _tokenService;
    private readonly RedisService _redis;

    public AuthService(ClientService clientService, PasswordService passwordService, TokenService tokenService, RedisService redis)
    {
        _clientService = clientService;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _redis = redis;
    }
    public async Task<string> LoginAsync(string email, string password)
    {
        var client = await _clientService.GetByEmailAsync(email);
        if (client == null)
            throw new UnauthorizedAccessException("Client not found");

        if (!_passwordService.VerifyPassword(client.Password, password))
            throw new UnauthorizedAccessException("Invalid pass");

        var token = _tokenService.GenerateToken(client);

        await _redis.SetStringAsync($"session:{client.Id}", client.Id.ToString(), TimeSpan.FromMinutes(15));


        return token;
    }
}
