using RentMaster.Addresses.Models;
using RentMaster.Core.Repositories;
using RentMaster.Data;

namespace RentMaster.Addresses.Repostiories
{
    public class AddressDivisionRepository : BaseRepository<AddressDivision>
    {
        public AddressDivisionRepository(AppDbContext context) : base(context)
        {
        }
    }
}