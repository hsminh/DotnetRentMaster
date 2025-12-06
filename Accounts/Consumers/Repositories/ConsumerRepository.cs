using Microsoft.EntityFrameworkCore;
using RentMaster.Accounts.Models;
using RentMaster.Core.Repositories;
using RentMaster.Data;

namespace RentMaster.Accounts.Repositories
{
    public class ConsumerRepository : BaseRepository<Consumer>
    {
        private readonly AppDbContext _context;

        public ConsumerRepository(AppDbContext context) : base(context)
        {
            _context = context; 
        }

        public async Task<Consumer?> FindByUidAsync(Guid uid)
        {
            return await _context.Consumers
                .AsNoTracking() 
                .FirstOrDefaultAsync(c => c.Uid == uid);
        }
    }
}