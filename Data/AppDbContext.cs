using Microsoft.EntityFrameworkCore;
using RentMaster.Accounts.Models;
using RentMaster.RealEstate.Models;

namespace RentMaster.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        public DbSet<Consumer> Consumers { get; set; }
        public DbSet<Admin> Admins { get; set; }
        public DbSet<LandLord> LandLords { get; set; }
        public DbSet<Apartment> Apartments { get; set; }
        public DbSet<ApartmentRoom> ApartmentRooms { get; set; }
    }
}