namespace BankApi.Entities;
using BankApi.Enums;

public class Client
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public Role Role { get; set; } = Role.Client;
    public ICollection<BankCard> BankCards { get; set; } = new List<BankCard>();
}
