using Microsoft.EntityFrameworkCore;
using RentMaster.Core.Backend.Auth.Types.enums;
using RentMaster.Core.Exceptions;
using RentMaster.Core.Services;
using RentMaster.Data;
using RentMaster.Management.ConsumerContact.Repositories;

namespace RentMaster.Management.ConsumerContact.Services;

public class ConsumerContactService : BaseService<Models.ConsumerContact>
{
    private readonly ConsumerContactRepository _repository;
    private readonly AppDbContext _context;

    public ConsumerContactService(
        ConsumerContactRepository repository,
        AppDbContext context
    ) : base(repository)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<Models.ConsumerContact> AddConsumerToApartment(
        Guid consumerUid,
        Guid landlordUid,
        Guid apartmentUid,
        string type
    )
    {
        var hasActiveContact = await _repository.ConsumerHasActiveContact(
            consumerUid, apartmentUid
        );

        if (hasActiveContact)
        {
            throw new ValidationException(
                "Consumer",
                "Consumer already has an active contact for this apartment"
            );
        }

        await ValidateApartmentExist(apartmentUid, type);

        var consumerContact = new Models.ConsumerContact
        {
            Consumer_Uid = consumerUid,
            Landlord_Uid = landlordUid,
            Apartment_UID = apartmentUid,
            Type = type,
            CreatedAt = DateTime.UtcNow
        };

        return await _repository.CreateAsync(consumerContact);
    }

    public async Task<Models.ConsumerContact?> GetConsumerContactDetails(
        Guid contactId,
        Guid landlordId
    )
    {
        var contact = await _context.ConsumerContacts
            .Include(c => c.Consumer)
            .FirstOrDefaultAsync(
                c => c.Uid == contactId && c.Landlord_Uid == landlordId
            );

        if (contact == null)
            return null;

        await LoadRealEstateUnitAsync(contact);
        return contact;
    }

    public async Task<IEnumerable<Models.ConsumerContact>> GetConsumerContactsFiltered(
        Guid viewerUid,
        UserTypes role,
        string? type = null,
        string? status = null
    )
    {
        var query = _context.ConsumerContacts
            .AsNoTracking()
            .AsQueryable();

        query = role switch
        {
            UserTypes.LandLord =>
                query.Where(c => c.Landlord_Uid == viewerUid),

            UserTypes.Consumer =>
                query.Where(c => c.Consumer_Uid == viewerUid),

            _ => query
        };

        if (!string.IsNullOrWhiteSpace(type))
        {
            query = query.Where(
                c => c.Type.ToLower() == type.ToLower()
            );
        }

        if (!string.IsNullOrWhiteSpace(status)
            && Enum.TryParse<enums.JoinApartmentStatus>(
                status,
                true,
                out var parsedStatus
            ))
        {
            query = query.Where(c => c.Status == parsedStatus);
        }

        var contacts = await query
            .Include(c => c.Consumer)
            .ToListAsync();

        await LoadRealEstateUnitsAsync(contacts);
        return contacts;
    }


    private async Task ValidateApartmentExist(Guid apartmentUid, string type)
    {
        if (type.Equals(
                ApartmentType.FullApartment.ToString(),
                StringComparison.OrdinalIgnoreCase
            ))
        {
            var apartment = await _context.Apartments
                .FirstOrDefaultAsync(a => a.Uid == apartmentUid);

            if (apartment == null)
                throw new ValidationException("Apartment", "Apartment not found");
        }
        else if (type.Equals(
                ApartmentType.RoomBased.ToString(),
                StringComparison.OrdinalIgnoreCase
            ))
        {
            var room = await _context.ApartmentRooms
                .FirstOrDefaultAsync(r => r.Uid == apartmentUid);

            if (room == null)
                throw new ValidationException("Room", "Room not found");
        }
        else
        {
            throw new ValidationException(
                "Type",
                "Invalid type. Must be 'FullApartment' or 'RoomBased'"
            );
        }
    }

    private async Task LoadRealEstateUnitsAsync(
        IEnumerable<Models.ConsumerContact> contacts
    )
    {
        foreach (var contact in contacts)
        {
            await LoadRealEstateUnitAsync(contact);
        }
    }

    private async Task LoadRealEstateUnitAsync(
        Models.ConsumerContact contact
    )
    {
        if (contact.Type.Equals(
                ApartmentType.FullApartment.ToString(),
                StringComparison.OrdinalIgnoreCase
            ))
        {
            contact.Apartment = await _context.Apartments
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    a => a.Uid == contact.Apartment_UID
                );
        }
        else if (contact.Type.Equals(
                ApartmentType.RoomBased.ToString(),
                StringComparison.OrdinalIgnoreCase
            ))
        {
            contact.ApartmentRoom = await _context.ApartmentRooms
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    ar => ar.Uid == contact.Apartment_UID
                );
        }
    }
}
