
using System.Linq.Expressions;
using RentMaster.Core.Repositories;
using RentMaster.Data;
using RentMaster.RealEstate.Models;

namespace RentMaster.RealEstate.Repositories;

public class ApartmentRoomRepository : BaseRepository<ApartmentRoom>
{
    public ApartmentRoomRepository(AppDbContext context) : base(context)
    {
    }
}
