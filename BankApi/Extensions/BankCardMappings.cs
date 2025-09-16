namespace BankApi.Extensions;
using BankApi.Entities;
using BankApi.DTOs;

public static class BankCardMappings
{
    public static ReadBankCardDTO ToReadBankCardDTO(this BankCard card)
    {
        if (card.Client == null)
            throw new InvalidOperationException("Client must be included when mapping BankCard to DTO");

        return new ReadBankCardDTO
        {
            Id = card.Id,
            Number = card.CardNumber,
            Expiry = card.Expiry,
            Owner = new ClientDTOForBankCard
            {
                Id = card.Client.Id,
                Name = card.Client.Name
            }
        };
    }
    public static ReadClientDTO ToReadClientDTO(this Client client)
    {
        if (client == null)
            throw new ArgumentNullException("client");

        return new ReadClientDTO
        {
            Id = client.Id,
            Name = client.Name,
            Email = client.Email,
            Role = client.Role,
            BankCardIds = client.BankCards?.Select(b => b.Id).ToList() ?? new List<Guid>()
        };
    }
}
