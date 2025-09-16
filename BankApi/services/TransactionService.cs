namespace BankApi.Services;
using BankApi.Data;
using BankApi.DTOs;
using BankApi.Entities;
using BankApi.Enums;
using BankApi.Extensions;
using Microsoft.EntityFrameworkCore;

public class TransactionService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TransactionService> _logger;


    public TransactionService(AppDbContext context, ILogger<TransactionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ReadBankCardDTO> DepositAsync(Guid cardId, decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        var card = await _context.BankCards
            .Include(c => c.Client) // нужно для DTO
            .FirstOrDefaultAsync(c => c.Id == cardId)
            ?? throw new KeyNotFoundException($"Bank card with Id {cardId} not found");

        _logger.LogInformation("Deposit request: CardId {CardId}, Amount {Amount}", cardId, amount);

        card.Balance += amount;

        var trx = new Transaction
        {
            ToCardId = card.Id,
            Amount = amount,
            Type = TransactionType.Deposit
        };
        _context.Transactions.Add(trx);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Deposit success: CardId {CardId}, NewBalance {Balance}", cardId, card.Balance);

        return card.ToReadBankCardDTO();
    }

    public async Task<ReadBankCardDTO> WithdrawalAsync(Guid cardId, decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        var card = await _context.BankCards
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == cardId)
            ?? throw new KeyNotFoundException($"Bank card with Id {cardId} not found");

        if (card.Balance < amount)
            throw new InvalidOperationException("Insufficient funds");

        _logger.LogInformation("Withdraw request: CardId {CardId}, Amount {Amount}", cardId, amount);

        card.Balance -= amount;

        var trx = new Transaction
        {
            FromCardId = card.Id,
            Amount = amount,
            Type = TransactionType.Withdrawal
        };
        _context.Transactions.Add(trx);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Withdrawal success: CardId {CardId}, NewBalance {Balance}", cardId, card.Balance);

        return card.ToReadBankCardDTO();
    }


    public async Task<TransferResultDTO> TransferAsync(Guid fromCardId, Guid toCardId, decimal amount)
    {
        if (amount <= 0) 
            throw new ArgumentException("Amount must be greater than zero", nameof(amount));

        if (fromCardId == toCardId)
            throw new InvalidOperationException("Cannot transfer to the same card");

        var fromCard = await _context.BankCards
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == fromCardId)
            ?? throw new KeyNotFoundException($"Bank card with Id {fromCardId} not found");

        var toCard = await _context.BankCards
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == toCardId)
            ?? throw new KeyNotFoundException($"Bank card with Id {toCardId} not found");

        if (fromCard.Balance <= amount)
            throw new InvalidOperationException("Insufficient funds");

        _logger.LogInformation("Transfer request: from card {FromCardId} to {ToCardId}", fromCardId, toCardId);

        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            fromCard.Balance -= amount;
            toCard.Balance += amount;

            var trx = new Transaction
            {
                FromCardId = fromCardId,
                ToCardId = toCardId,
                Amount = amount,
                Type = TransactionType.Transfer,
            };
            _context.Transactions.Add(trx);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
            "Transfer success: From {FromCardId} NewBalance {FromBalance}, To {ToCardId} NewBalance {ToBalance}",
            fromCardId, fromCard.Balance, toCardId, toCard.Balance
);

            var result = new TransferResultDTO
            {
                FromCard = fromCard.ToReadBankCardDTO(),
                ToCard = toCard.ToReadBankCardDTO()
            };
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            _logger.LogError("Transfer failed: From card {fromCardId} to card {toCardId}", fromCardId, toCardId);
            throw;
        }
    }
}