namespace BankApi.DTOs;
public class DepositRequestDTO
{
    public Guid CardId { get; set; }
    public decimal Amount { get; set; }
}
