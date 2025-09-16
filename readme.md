# Banking API

A simple banking API for managing clients, cards, and transactions.

## Entities
- `Client` — a bank client  
- `BankCard` — a client’s bank card  
- `Transaction` — a record of a transfer or card operation  

## Features
- CRUD operations for clients and cards  
- Deposit and withdrawal of funds  
- Transfers between cards  
- View transaction history  
- JWT authentication for clients and admin  

## Testing
- Unit tests for card operation services (deposit, withdrawal, transfer)  
- InMemory database used to isolate tests  

## Technologies
- ASP.NET Core Web API  
- Entity Framework Core (SQLite for development, InMemory for testing)  
- xUnit for testing
