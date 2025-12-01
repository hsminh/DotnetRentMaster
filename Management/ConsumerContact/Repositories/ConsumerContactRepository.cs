using Microsoft.EntityFrameworkCore;
using RentMaster.Core.Repositories;
using RentMaster.Data;

namespace RentMaster.Management.ConsumerContact.Repositories;

public class ConsumerContactRepository : BaseRepository<Models.ConsumerContact>
{
    private readonly AppDbContext _context;
    private readonly DbSet<Models.ConsumerContact> _dbSet;

    public ConsumerContactRepository(AppDbContext context) : base(context)
    {
        _context = context;
        _dbSet = _context.Set<Models.ConsumerContact>();
    }

    public async Task<bool> ConsumerHasActiveContact(Guid consumerUid, Guid apartmentUid)
    {
        return await _dbSet.AnyAsync(cc => 
            cc.Consumer_Uid == consumerUid && 
            cc.Apartment_UID == apartmentUid);
    }

    public async Task<IEnumerable<Models.ConsumerContact>> GetByConsumerIdAsync(Guid consumerUid)
    {
        return await _dbSet
            .Where(cc => cc.Consumer_Uid == consumerUid)
            .ToListAsync();
    }

    public async Task<IEnumerable<Models.ConsumerContact>> GetByApartmentIdAsync(Guid apartmentUid, string type)
    {
        return await _dbSet
            .Where(cc => cc.Apartment_UID == apartmentUid && cc.Type == type)
            .ToListAsync();
    }
}
