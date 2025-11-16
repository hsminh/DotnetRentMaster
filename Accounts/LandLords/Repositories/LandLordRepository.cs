using RentMaster.Accounts.LandLords.Models;
using RentMaster.Core.Repositories;
using RentMaster.Data;

namespace RentMaster.Accounts.LandLords.Repositories
{
    public class LandLordRepository: BaseRepository<LandLord>
    {
        public LandLordRepository(AppDbContext context) : base(context)
        {
        }
        
    }
}