using RentMaster.Core.Services;
using RentMaster.Management.Tenant.Repository;

namespace RentMaster.Management.Tenant.Service;

public class TenantService : BaseService<Models.Tenant>
{

    public TenantService(TenantRepository repository)
        : base(repository)
    {
    }
}