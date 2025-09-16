namespace BankApi.DTOs;

public class TransferRequestDTO
{
    public Guid FromCardId { get; set; }
    public Guid ToCardId { get; set; }
    public decimal Amount { get; set; }
}