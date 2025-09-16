namespace BankApi.Controllers;
using BankApi.DTOs;
using BankApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class BankCardController : ControllerBase
{
    private readonly BankCardService _bankcardservice;

    public BankCardController (BankCardService bankcardservice)
    {
        _bankcardservice = bankcardservice;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<ReadBankCardDTO>>>GetAll(int page, int pageSize)
    {
        var bankcards = await _bankcardservice.GetAllAsync(page, pageSize);

        if (!bankcards.Any())
            return NotFound("Bank cards not found");
        return Ok(bankcards);
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    
    public async Task<ActionResult<ReadBankCardDTO>> GetById(Guid id)
    {
        var bankcard = await _bankcardservice.GetByIdAsync(id);

        if(bankcard == null)
            return NotFound($"Bank card with Id {id} not found");

        return Ok(bankcard);
    }
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<ReadBankCardDTO>> Post ([FromBody] PostBankCardDTO postBankCardDTO)
    {
        var bankcard = await _bankcardservice.PostAsync(postBankCardDTO);

        if (bankcard == null)
            return BadRequest("Invalid data");

        return CreatedAtAction(nameof(GetById), new { id = bankcard.Id }, bankcard);
    }
    [Authorize]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ReadBankCardDTO>> Put(Guid id, [FromBody] PutBankCardDTO postBankCardDTO)
    {
        var bankcard = await _bankcardservice.PutAsync(id, postBankCardDTO);

        if (bankcard == null)
            return NotFound("Bank card not found");

        return Ok(bankcard);
    }
}