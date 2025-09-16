namespace BankApi.DTOs;

public class PostBankCardDTO
{
    public Guid ClientId { get; set; }
    public DateTime Expiry { get; set; }
}
