using Microsoft.EntityFrameworkCore;

namespace TestABan.Models
{
    public class ClienteContext : DbContext
    {
        public ClienteContext(DbContextOptions<ClienteContext> options) : base(options)
        {            
        }

        public ClienteContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        public string DbPath { get; }

        public DbSet<Cliente> Clientes { get; set; } = null!;
    }
}
