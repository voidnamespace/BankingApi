namespace BankApi.Services;
using BankApi.Data;
using BankApi.DTOs;
using BankApi.Entities;
using BankApi.Enums;
using BankApi.Extensions;
using Microsoft.EntityFrameworkCore;

public class ClientService
{
    private readonly AppDbContext _context;
    private readonly ILogger<ClientService> _logger;
    private readonly PasswordService _passwordService;

    public ClientService(AppDbContext context, ILogger<ClientService> logger, PasswordService passwordService)
    {
        _context = context;
        _logger = logger;
        _passwordService = passwordService;
    }
    public async Task<List<ReadClientDTO>> GetAllAsync(int page, int pageSize)
    {
        _logger.LogInformation("Request to receive all clients (page {page}, pageSize {pageSize})", page, pageSize);

        var clients = await _context.Clients
            .Include(c => c.BankCards)
            .OrderBy(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        if (clients.Count == 0)
            _logger.LogWarning("Clients not found");

        return clients.Select(c => c.ToReadClientDTO()).ToList();
    }
    public async Task<ReadClientDTO> GetByIdAsync(Guid id)
    {
        _logger.LogInformation("Request to receive client by Id {id}", id);

        var client = await _context.Clients
            .Include(c => c.BankCards)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
        {
            _logger.LogWarning("Client with Id {id} not found", id);
            throw new KeyNotFoundException("Client not found");
        }

        return client.ToReadClientDTO();
    }
    public async Task<ReadClientDTO> PostAsync(CreateClientDTO createClientDTO)
    {
        if (createClientDTO == null)
            throw new ArgumentNullException(nameof(createClientDTO), "Client data is required");

        _logger.LogInformation("Request to create client");

        var client = new Client
        {
            Id = Guid.NewGuid(),
            Name = createClientDTO.Name,
            Email = createClientDTO.Email,
            Password = _passwordService.HashPassword(createClientDTO.Password),
            Role = Role.Client
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Client  with Id {ClientId} successfully created", client.Id);

        return client.ToReadClientDTO();
    }
    public async Task<ReadClientDTO> PutAsync(Guid id, UpdateClientDTO updateClientDTO, string userRole)
    {
        var oldClient = await _context.Clients.FindAsync(id)
            ?? throw new KeyNotFoundException($"Client with Id {id} not found");

        oldClient.Name = updateClientDTO.Name ?? oldClient.Name;
        oldClient.Email = updateClientDTO.Email ?? oldClient.Email;

        if (userRole == "Admin")
        {
            oldClient.Role = updateClientDTO.Role ?? oldClient.Role;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Client with Id {ClientId} successfully updated", oldClient.Id);

        return oldClient.ToReadClientDTO();
    }
    public async Task<bool> DeleteAsync(Guid id)
    {
        var client = await _context.Clients.FindAsync(id);
        if (client == null)
        {
            _logger.LogWarning("Trying to delete a client with Id {ClientId}, but it is not found.", id);
            throw new KeyNotFoundException($"Client with Id {id} not found");
        }
        _logger.LogInformation("Request to delete client with Id {ClientId}", id);

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Client with Id {ClientId} deleted.", id);

        return true;
    }
    public async Task<Client> GetByEmailAsync(string email)
    {
        var client = await _context.Clients.FirstOrDefaultAsync(c => c.Email == email);
        if (client == null)
            throw new KeyNotFoundException("Client not found");

        return client;
    }
}
