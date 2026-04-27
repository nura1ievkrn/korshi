using korshi.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace korshi.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Complex> Complexes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed: один тестовый ЖК
            builder.Entity<Complex>().HasData(new Complex
            {
                Id = 1,
                Name = "Nurly Tau",
                Address = "ул. Аль-Фараби, 77",
                City = "Алматы"
            });
        }
    }
}