using Microsoft.Extensions.DependencyInjection;
using RentMaster.Management.RentalContract.Repositories;
using RentMaster.Management.RentalContract.Services;

namespace RentMaster.Management.RentalContract;

public static class DependencyInjection
{
    public static IServiceCollection AddRentalContractDependencies(this IServiceCollection services)
    {
        services.AddScoped<RentalContractRepository>();
        services.AddScoped<RentalContractService>();

        return services;
    }
}
