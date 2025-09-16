namespace BankApi.DTOs;

public class ReadBankCardDTO
{
    public Guid Id { get; set; }
    public string Number { get; set; } = null!;
    public DateTime Expiry { get; set; }
    public ClientDTOForBankCard Owner { get; set; } = null!;
}
