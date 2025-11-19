using RentMaster.Core.Repositories;
using RentMaster.Data;
using RentMaster.Management.RealEstate.Models;

public class ApartmentRepository: BaseRepository<Apartment>
{
    public ApartmentRepository(AppDbContext context) : base(context)
    {
    }
}
