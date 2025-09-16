namespace BankApi.DTOs;
using BankApi.Enums;

public class ReadClientDTO
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; } = Role.Client;

    public List<Guid> BankCardIds { get; set; } = new List<Guid>();
}
