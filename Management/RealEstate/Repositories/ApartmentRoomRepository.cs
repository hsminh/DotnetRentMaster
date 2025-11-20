using RentMaster.Core.Repositories;
using RentMaster.Data;
using RentMaster.Management.RealEstate.Models;

namespace RentMaster.Management.RealEstate.Repositories;

public class ApartmentRoomRepository : BaseRepository<ApartmentRoom>
{
    public ApartmentRoomRepository(AppDbContext context) : base(context)
    {
    }
}
