using RentMaster.Accounts.Models;
using RentMaster.Core.Repositories;
using RentMaster.Data;

namespace RentMaster.Accounts.Repositories
{
    public class LandLordRepository: BaseRepository<LandLord>
    {
        public LandLordRepository(AppDbContext context) : base(context)
        {
        }
        
    }
}