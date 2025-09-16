namespace BankApi.DTOs;

    public class TransferResultDTO
    {
        public required ReadBankCardDTO FromCard { get; set; } 
        public required ReadBankCardDTO ToCard { get; set; } 
    }
