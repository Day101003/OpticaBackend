using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Models;

namespace ProyectoFinal.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        { }
        public DbSet<Availability> Availability { get; set; }
        public DbSet<Categories> Categories { get; set; }
        public DbSet<Clients> Clients { get; set; }
        public DbSet<Images> Images { get; set; }
        public DbSet<Products> Products { get; set; }
        public DbSet<Quotes> Quotes { get; set; } 
        public DbSet<Users> Users { get; set; }
    }
}

