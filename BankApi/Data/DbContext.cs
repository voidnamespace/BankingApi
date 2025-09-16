using BankApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Data
{
    public class AppDbContext : DbContext
    {

        public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options)
        {
        }
        
        public DbSet<Client> Clients { get; set; } = null!;
        public DbSet<BankCard> BankCards { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=banking.db");
            }
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        { 
          base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Client>()
                .HasMany(c => c.BankCards)
                .WithOne(c => c.Client)
                .HasForeignKey(c => c.ClientId);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.FromCard)
                .WithMany(c => c.TransactionsFrom)
                .HasForeignKey(t => t.FromCardId)
                .OnDelete(DeleteBehavior.Restrict);
         
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.ToCard)
                .WithMany(c => c.TransactionsTo)
                .HasForeignKey(t => t.ToCardId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
