using RentMaster.Addresses.Commands;
using RentMaster.Addresses.Repostiories;
using RentMaster.Addresses.Services;

namespace RentMaster.Addresses
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddressModule(this IServiceCollection services)
        {
            services.AddScoped<AddressService>();
            services.AddScoped<AddressDivisionRepository>();
            services.AddScoped<IAddressImportService, AddressImportService>();
            services.AddTransient<AddressDataSeeder>();
            return services;
        }
    }
}