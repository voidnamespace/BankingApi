namespace BankApi.Controllers;
using BankApi.DTOs;
using BankApi.Entities;
using BankApi.Enums;
using BankApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SQLitePCL;


[ApiController]
[Route("api/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly ClientService _clientService;

    public ClientsController(ClientService clientService)
    {
        _clientService = clientService;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<ReadClientDTO>>> GetAll([FromServices] CacheService cacheService, int page = 1, int pageSize = 20)
    {
        string cacheKey = $"clients:all:{page}:{pageSize}";


        var cachedClients = await cacheService.GetAsync<List<Client>>(cacheKey);
        if (cachedClients != null)
            return Ok(cachedClients);

        var clients = await _clientService.GetAllAsync(page, pageSize);

        if (!clients.Any())
            return NotFound("Clients not found");

        await cacheService.SetAsync(cacheKey, clients, TimeSpan.FromMinutes(1));

        return Ok(clients);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReadClientDTO>> GetById(Guid id)
    {
        var getClient = await _clientService.GetByIdAsync(id);

        var userIdClaim = User.FindFirst("id")?.Value;
        var userRole = User.FindFirst("role")?.Value;

        if (userRole != "Admin" && userIdClaim != getClient.Id.ToString())
            return Forbid();

        return Ok(getClient);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<ReadClientDTO>> Post([FromBody] CreateClientDTO createClientDTO, [FromServices] CacheService cacheService)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var postClient = await _clientService.PostAsync(createClientDTO);

        await cacheService.RemoveAsync("clients:all");

        return CreatedAtAction(nameof(GetById), new { id = postClient.Id }, postClient);
    }

    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ReadClientDTO>> Put(Guid id, [FromBody] UpdateClientDTO updateClientDTO, [FromServices] CacheService cacheService)
    {
        var userRole = User.FindFirst("role")?.Value ?? "User";

        var updatedClient = await _clientService.PutAsync(id, updateClientDTO, userRole);

        await cacheService.RemoveAsync($"client:{id}");

        return Ok(updatedClient);
    }

    [Authorize]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromServices] CacheService cacheService)
    {
        try
        {
            await _clientService.DeleteAsync(id);
            await cacheService.RemoveAsync($"client:{id}");
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

}

