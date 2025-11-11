using RentMaster.Core.Repositories;
using RentMaster.Data;

namespace RentMaster.Management.Tenant.Repository;

public class TenantRepository : BaseRepository<Models.Tenant>
{
    public TenantRepository(AppDbContext context) : base(context)
    {
    }
}