namespace BankApi.Entities;
using BankApi.Enums;

public class Transaction
{
    public Guid Id { get; set; }
    public Guid? FromCardId { get; set; }
    public BankCard? FromCard { get; set; }

    public Guid? ToCardId { get; set; }
    public BankCard? ToCard { get; set; }

    public decimal Amount { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public TransactionType Type { get; set; }

    public ICollection<Transaction> TransactionsFrom { get; set; } = new List<Transaction>();
    public ICollection<Transaction> TransactionsTo { get; set; } = new List<Transaction>();

}
