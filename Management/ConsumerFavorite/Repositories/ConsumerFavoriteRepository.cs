using Microsoft.EntityFrameworkCore;
using RentMaster.Core.Repositories;
using RentMaster.Data;

namespace RentMaster.Management.ConsumerFavorite.Repositories;

public class ConsumerFavoriteRepository : BaseRepository<Models.ConsumerFavorite>
{
    private readonly AppDbContext _context;
    private readonly DbSet<Models.ConsumerFavorite> _dbSet;

    public ConsumerFavoriteRepository(AppDbContext context) : base(context)
    {
        _context = context;
        _dbSet = _context.Set<Models.ConsumerFavorite>();
    }

}
