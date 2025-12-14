using RentMaster.Core.Services;
using RentMaster.Data;
using RentMaster.Management.ConsumerFavorite.Models.DTOs;
using RentMaster.Management.ConsumerFavorite.Repositories;
using RentMaster.Management.RealEstate.Services;

namespace RentMaster.Management.ConsumerFavorite.Services;

public class ConsumerFavoriteService : BaseService<Models.ConsumerFavorite>
{
    private readonly ConsumerFavoriteRepository _repository;
    private readonly AppDbContext _context;
    private ApartmentService _apartmentService;
    private ApartmentRoomService _apartmentRoomService;

    public ConsumerFavoriteService(ApartmentService apartmentService, ApartmentRoomService apartmentRoomService, ConsumerFavoriteRepository repository, AppDbContext context) 
        : base(repository)
    {
        _repository = repository;
        _apartmentService = apartmentService;
        _apartmentRoomService = apartmentRoomService;
        _context = context;
    }

    public async Task<Models.ConsumerFavorite> CreateFavoriteAsync(CreateConsumerFavoriteDto dto)
    {
        var consumerFavorite = new Models.ConsumerFavorite
        {
            Type = dto.Type,
            ApartmentUid = dto.ApartmentUid,
            ApartmentRoomUid = dto.ApartmentRoomUid,
            consumer_id = dto.ConsumerId
        };

        if (dto.Type == ApartmentType.FullApartment.ToString() && dto.ApartmentUid.HasValue)
        {
            var apartment = await _apartmentService.GetApartment(dto.ApartmentUid.Value);
            if (apartment == null)
            {
                throw new KeyNotFoundException($"Apartment with UID {dto.ApartmentUid} not found");
            }
            consumerFavorite.Apartment = apartment;
        }
        else if (dto.Type == ApartmentType.RoomBased.ToString() && dto.ApartmentRoomUid.HasValue)
        {
            var room = await _apartmentRoomService.GetApartmentRoom(dto.ApartmentRoomUid.Value);
            if (room == null)
            {
                throw new KeyNotFoundException($"Apartment room with UID {dto.ApartmentRoomUid} not found");
            }
            consumerFavorite.ApartmentRoom = room;
        }
        else
        {
            throw new ArgumentException("Invalid favorite type or missing UID");
        }

        return await _repository.CreateAsync(consumerFavorite);
    }
    public async Task<IEnumerable<Models.ConsumerFavorite>> GetAllAsync(Accounts.Models.Consumer consumer)
    {
        var favorites = await _repository.FilterAsync(a => a.consumer_id == consumer.Uid);
    
        foreach (var favorite in favorites)
        {
            if (favorite.ApartmentUid.HasValue)
            {
                favorite.Apartment = await _apartmentService.GetApartment(favorite.ApartmentUid.Value);
            }
            else if (favorite.ApartmentRoomUid.HasValue)
            {
                favorite.ApartmentRoom = await _apartmentRoomService.GetApartmentRoom(favorite.ApartmentRoomUid.Value);
            }
        }
    
        return favorites;
    }
}
