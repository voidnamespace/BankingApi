namespace BankApi.DTOs;
using BankApi.Enums;

public class UpdateClientDTO
{
    public string? Name { get; set; }
    public string? Email { get; set; }

    public Role? Role { get; set; }
}

