namespace BankApi.Services;
using BankApi.Data;
using BankApi.Entities;
using Microsoft.EntityFrameworkCore;
using BankApi.DTOs;
using BankApi.Extensions;


public class BankCardService
{
    private readonly AppDbContext _context;
    private readonly ILogger<BankCardService> _logger;

    public BankCardService(AppDbContext context, ILogger<BankCardService> logger)
    {
        _context = context;
        _logger = logger;
    }
    private string GenerateCardNumber()
    {
        var rnd = new Random();
        return string.Concat(Enumerable.Range(0, 16).Select(_ => rnd.Next(0, 10).ToString()));
    }
    public async Task<List<ReadBankCardDTO>> GetAllAsync(int page, int pageSize)
    {
        _logger.LogInformation("Request to recieve the list of all bank cards (page {page}, pageSize {pageSize})", page, pageSize);

        var cards = await _context.BankCards
            .Include(c => c.Client)
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (cards.Count == 0)
            _logger.LogWarning("Bank cards not found");

        return cards.Select(c => c.ToReadBankCardDTO()).ToList();
    }
    public async Task<ReadBankCardDTO> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Request to receive bank card by id {Id}", id);

        var card = await _context.BankCards
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (card == null)
        {
            _logger.LogWarning("Bank card with id {Id} not found", id);
            throw new KeyNotFoundException("Bank card not found");
        }

        return card.ToReadBankCardDTO();
    }
    public async Task<ReadBankCardDTO> PostAsync(PostBankCardDTO postBankCardDTO)
    {
        if (postBankCardDTO == null)
            throw new ArgumentNullException(nameof(postBankCardDTO), "Bank card data is required");

        _logger.LogInformation("Request to post new bank card");

        var newCard = new BankCard
        {
            Id = Guid.NewGuid(),
            ClientId = postBankCardDTO.ClientId,
            Expiry = postBankCardDTO.Expiry,
            Balance = 0,
            CardNumber = GenerateCardNumber()
        };

        await _context.BankCards.AddAsync(newCard);
        await _context.SaveChangesAsync();

        await _context.Entry(newCard).Reference(c => c.Client).LoadAsync();

        _logger.LogInformation(
            "Bank card with Id {CardId} created for ClientId {ClientId}",
            newCard.Id,
            newCard.ClientId
        );

        return newCard.ToReadBankCardDTO();
    }
    public async Task<ReadBankCardDTO> PutAsync(Guid id, PutBankCardDTO putBankCardDTO)
    {
        var oldCard = await _context.BankCards
            .Include(c => c.Client)
            .FirstOrDefaultAsync(c => c.Id == id)
            ?? throw new KeyNotFoundException($"Bank card with id {id} not found.");

        _logger.LogInformation("Request to update bank card with  Id {CardId}", oldCard.Id);

        oldCard.Expiry = putBankCardDTO.Expiry;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Bank card with Id {CardId} successfully updated", oldCard.Id);

        return oldCard.ToReadBankCardDTO();
    }
    public async Task<bool> DeleteAsync(Guid id)
    {
        _logger.LogInformation("Request to delete a bank card with Id {BankCardId}", id);

        var card = await _context.BankCards.FindAsync(id) ?? throw new KeyNotFoundException($"Bank card with ID {id} not found");

        _context.BankCards.Remove(card);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Bank card with Id {CardId} successfully deleted", card.Id);
        return true;
    }
}
