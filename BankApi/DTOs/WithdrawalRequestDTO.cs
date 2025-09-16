namespace BankApi.DTOs;
public class WithdrawalRequestDTO
{
    public Guid CardId { get; set; }
    public decimal Amount { get; set; }
}
