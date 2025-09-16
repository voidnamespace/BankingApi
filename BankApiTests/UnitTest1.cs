namespace BankApi.Tests;
using BankApi.Data;
using BankApi.Entities;
using BankApi.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Xunit;

public class TransactionTests
{
    private readonly AppDbContext _context;
    private readonly TransactionService _service;

    public TransactionTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;

        _context = new AppDbContext(options);

        var logger = new Logger<TransactionService>(new LoggerFactory());

        _service = new TransactionService(_context, logger);
    }

    [Fact]
    public async Task Deposit_ShouldIncreaseBalance()
    {
        var card = new BankCard { Id = Guid.NewGuid(), Balance = 100 };
        _context.BankCards.Add(card);
        await _context.SaveChangesAsync();

        await _service.DepositAsync(card.Id, 50);

        var updatedCard = await _context.BankCards.FindAsync(card.Id);
        Assert.Equal(150, updatedCard.Balance);
    }

    [Fact]
    public async Task Withdrawal_ShouldDecreaseBalance()
    {
        var card = new BankCard { Id = Guid.NewGuid(), Balance = 100 };
        _context.BankCards.Add(card);
        await _context.SaveChangesAsync();

        await _service.WithdrawalAsync(card.Id, 50);

        var updatedCard = await _context.BankCards.FindAsync(card.Id);
        Assert.Equal(50, updatedCard.Balance);
    }

    [Fact]
    public async Task TransferAsync_ShouldMoveMoneyBetweenCards()
    {
        var fromCard = new BankCard { Id = Guid.NewGuid(), Balance = 100 };
        var toCard = new BankCard { Id = Guid.NewGuid(), Balance = 0 };

        _context.BankCards.Add(fromCard);
        _context.BankCards.Add(toCard);
        await _context.SaveChangesAsync();

        await _service.TransferAsync(fromCard.Id, toCard.Id, 50);

        var updatedFrom = await _context.BankCards.FindAsync(fromCard.Id);
        var updatedTo = await _context.BankCards.FindAsync(toCard.Id);

        Assert.Equal(50, updatedFrom.Balance);
        Assert.Equal(50, updatedTo.Balance);
    }

    [Fact]
    public async Task Deposit_CantAddNegativeAmount()
    {
        var card = new BankCard { Id = Guid.NewGuid(), Balance = 100 };
        _context.BankCards.Add(card);
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<ArgumentException>(() => _service.DepositAsync(card.Id, -10));
    }

    [Fact]
    public async Task Withdrawal_CantExceedBalance()
    {
        var card = new BankCard { Id = Guid.NewGuid(), Balance = 100 };
        _context.BankCards.Add(card);
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.WithdrawalAsync(card.Id, 200));
    }

    [Fact]
    public async Task Transfer_CantExceedBalance()
    {
        var fromCard = new BankCard { Id = Guid.NewGuid(), Balance = 100 };
        var toCard = new BankCard { Id = Guid.NewGuid(), Balance = 0 };

        _context.BankCards.Add(fromCard);
        _context.BankCards.Add(toCard);
        await _context.SaveChangesAsync();

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.TransferAsync(fromCard.Id, toCard.Id, 200));
    }
}
