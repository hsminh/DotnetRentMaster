using RentMaster.Core.Repositories;
using RentMaster.Data;

namespace RentMaster.Management.RentalContract.Repositories;

public class RentalContractRepository : BaseRepository<Models.RentalContract>
{
    public RentalContractRepository(AppDbContext context) : base(context)
    {
    }
}
