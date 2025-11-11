using RentMaster.Management.Tenant.Repository;
using RentMaster.Management.Tenant.Service;

namespace RentMaster.Management.Tenant
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddTenantModule(this IServiceCollection services)
        {
            services.AddScoped<TenantRepository>();
            services.AddScoped<TenantService>();
            services.AddScoped<TenantApprovalRepository>();
            services.AddScoped<TenantApprovalService>();
            return services;
        }
    }
}