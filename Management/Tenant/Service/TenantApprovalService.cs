using RentMaster.Core.Services;
using RentMaster.Management.Tenant.Repository;

namespace RentMaster.Management.Tenant.Service;

public class TenantApprovalService : BaseService<Models.TenantApproval>
{

    public TenantApprovalService(TenantApprovalRepository repository)
        : base(repository)
    {
    }
}