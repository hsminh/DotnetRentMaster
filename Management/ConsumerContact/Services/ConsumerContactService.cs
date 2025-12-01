using Microsoft.EntityFrameworkCore;
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
        // Check if consumer already has an active contact for this apartment
        var hasActiveContact = await _repository.ConsumerHasActiveContact(
            consumerUid, apartmentUid);

        if (hasActiveContact)
        {
            throw new ValidationException("Consumer", "Consumer already has an active contact for this apartment");
        }

        // Get apartment to get the type
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

    public async Task<IEnumerable<Models.ConsumerContact>> GetConsumerContacts(Guid consumerUid)
    {
        return await _repository.GetByConsumerIdAsync(consumerUid);
    }

    public async Task<IEnumerable<Models.ConsumerContact>> GetApartmentContacts(Guid apartmentUid, string type)
    {
        return await _repository.GetByApartmentIdAsync(apartmentUid, type);
    }
}
