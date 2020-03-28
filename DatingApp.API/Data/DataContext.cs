using Microsoft.EntityFrameworkCore;

using DatingApp.API.Models;

namespace DatingApp.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions options) : base(options){}

        public DbSet<Value> Values {get; set;}
    }
}