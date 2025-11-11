using RentMaster.Core.Repositories;
using RentMaster.Data;

namespace RentMaster.Management.Tenant.Repository;

public class TenantApprovalRepository : BaseRepository<Models.TenantApproval>
{
    public TenantApprovalRepository(AppDbContext context) : base(context)
    {
    }
}