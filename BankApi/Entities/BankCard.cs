namespace BankApi.Entities;

public class BankCard
{
    public Guid Id { get; set; }
    public string CardNumber { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public DateTime Expiry { get; set; }
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;

    public ICollection<Transaction> TransactionsFrom { get; set; } = new List<Transaction>();
    public ICollection<Transaction> TransactionsTo { get; set; } = new List<Transaction>();

}
