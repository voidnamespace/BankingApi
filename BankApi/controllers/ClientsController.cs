namespace BankApi.Controllers;
using BankApi.DTOs;
using BankApi.Entities;
using BankApi.Enums;
using BankApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<List<ReadClientDTO>>> GetAll(int page = 1, int pageSize = 20)
        {
            var clients = await _clientService.GetAllAsync(page, pageSize);

            if (!clients.Any())
                return NotFound("Clients not found");

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
        public async Task<ActionResult<ReadClientDTO>> Post([FromBody] CreateClientDTO createClientDTO)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var postClient = await _clientService.PostAsync(createClientDTO);

            return CreatedAtAction(nameof(GetById), new { id = postClient.Id }, postClient);
        }

        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ReadClientDTO>> Put(Guid id, [FromBody] UpdateClientDTO updateClientDTO)
        {
            var userRole = User.FindFirst("role")?.Value ?? "User";

            var updatedClient = await _clientService.PutAsync(id, updateClientDTO, userRole);

            return Ok(updatedClient);
        }

        [Authorize]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete (Guid id)
        {
            try
            {
                await _clientService.DeleteAsync(id);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

    }

