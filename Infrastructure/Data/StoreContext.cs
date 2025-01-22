using Core.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Data
{
    public class StoreContext(DbContextOptions options) : IdentityDbContext<AppUser>(options)
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Address> Addresses { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

           
            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=DESKTOP-RDVKRO2\\SQLEXPRESS;Database=Skinet;Trusted_Connection=SSPI;Encrypt=false;TrustServerCertificate=true");
            base.OnConfiguring(optionsBuilder);
        }
    }

    

}
