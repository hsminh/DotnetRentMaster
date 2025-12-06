using Microsoft.EntityFrameworkCore;
using RentMaster.Accounts.LandLords.Models;
using RentMaster.Core.Exceptions;
using RentMaster.Core.Services;
using RentMaster.Data;
using RentMaster.Management.ConsumerContact.Repositories;
using RentMaster.Management.RealEstate.Models;

namespace RentMaster.Management.ConsumerContact.Services;

public class ConsumerContactService : BaseService<Models.ConsumerContact>
{
    private readonly ConsumerContactRepository _repository;
    private readonly AppDbContext _context;

    public ConsumerContactService(ConsumerContactRepository repository, AppDbContext context) 
        : base(repository)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<Models.ConsumerContact> AddConsumerToApartment(
        Guid consumerUid, 
        Guid landlordUid, 
        Guid apartmentUid
        )
    {
        var hasActiveContact = await _repository.ConsumerHasActiveContact(
            consumerUid, apartmentUid);

        if (hasActiveContact)
        {
            throw new ValidationException("Consumer", "Consumer already has an active contact for this apartment");
        }

        var apartment = await _context.Apartments
            .FirstOrDefaultAsync(a => a.Uid == apartmentUid) 
            ?? throw new ValidationException("Apartment", "Apartment not found");

        var consumerContact = new Models.ConsumerContact
        {
            Consumer_Uid = consumerUid,
            Landlord_Uid = landlordUid,
            Apartment_UID = apartmentUid,
            Type = apartment.Type.ToString(),
            CreatedAt = DateTime.UtcNow
        };

        return await _repository.CreateAsync(consumerContact);
    }
    
    public async Task<Models.ConsumerContact?> GetConsumerContactDetails(Guid contactId, Guid landlordId)
    {
        var contact = await _context.ConsumerContacts
            .Include(c => c.Consumer)
            .FirstOrDefaultAsync(c => c.Uid == contactId && c.Landlord_Uid == landlordId);

        if (contact == null)
            return null;

        if (contact.Type.Equals("FullApartment", StringComparison.OrdinalIgnoreCase))
        {
            contact.Apartment = await _context.Apartments
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Uid == contact.Apartment_UID);
        }
        else if (contact.Type.Equals("ApartmentRoom", StringComparison.OrdinalIgnoreCase))
        {
            contact.ApartmentRoom = await _context.ApartmentRooms
                .AsNoTracking()
                .FirstOrDefaultAsync(ar => ar.Uid == contact.Apartment_UID);
        }

        return contact;
    }

    public async Task<IEnumerable<Models.ConsumerContact>> GetConsumerContacts(LandLord landlord)
    {
        var query = _context.ConsumerContacts
            .AsNoTracking()
            .Where(c => c.Landlord_Uid == landlord.Uid);

        var contacts = await query
            .Include(c => c.Consumer)
            .ToListAsync();

    
        foreach (var contact in contacts)
        {
            if (contact.Type.Equals("FullApartment", StringComparison.OrdinalIgnoreCase))
            {
                contact.Apartment = await _context.Apartments
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Uid == contact.Apartment_UID);
            }
            else if (contact.Type.Equals("ApartmentRoom", StringComparison.OrdinalIgnoreCase))
            {
                contact.ApartmentRoom = await _context.ApartmentRooms
                    .AsNoTracking()
                    .FirstOrDefaultAsync(ar => ar.Uid == contact.Apartment_UID);
            }
        
        }

        return contacts;
    }

}
