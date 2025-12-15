using RentMaster.Core.Repositories;
using RentMaster.Data;

namespace RentMaster.Management.RentalContract.Repositories;

public class RentalContractMonthlyPaymentRepository : BaseRepository<Models.RentalContractMonthlyPayment>
{
    public RentalContractMonthlyPaymentRepository(AppDbContext context) : base(context)
    {
    }
}
