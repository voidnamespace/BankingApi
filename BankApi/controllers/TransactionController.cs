namespace BankApi.Controllers;

using BankApi.DTOs;
using BankApi.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly TransactionService _transactionService;


    public TransactionController(TransactionService transactionService)
    {
        _transactionService = transactionService;
    }


    [HttpPost("deposit")]
    public async Task<ActionResult<ReadBankCardDTO>> Deposit([FromBody] DepositRequestDTO dto)
    {
        var result = await _transactionService.DepositAsync(dto.CardId, dto.Amount);
        return Ok(result);
    }

    [HttpPost("withdrawal")]
    public async Task<ActionResult<ReadBankCardDTO>> Withdrawal([FromBody] WithdrawalRequestDTO dto)
    {
        var result = await _transactionService.WithdrawalAsync(dto.CardId, dto.Amount);
        return Ok(result);
    }
    
    [HttpPost("transfer")]
    public async Task<ActionResult<TransferResultDTO>> Transfer([FromBody] TransferRequestDTO dto)
    {
        var result = await _transactionService.TransferAsync(dto.FromCardId, dto.ToCardId, dto.Amount);
        return Ok(result);
    }
}

