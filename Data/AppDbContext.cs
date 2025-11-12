using Microsoft.EntityFrameworkCore;
using RentMaster.Accounts.Models;
using RentMaster.Management.Tenant.Models;
using RentMaster.RealEstate.Models;
using RentMaster.Management.AddressDivision.Models;

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
        public DbSet<Tenant> Tenant { get; set; }
        public DbSet<AddressDivision> AddressDivisions { get; set; }
    }
}