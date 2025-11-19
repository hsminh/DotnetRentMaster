using FluentValidation;
using RentMaster.Addresses.Repostiories;
using RentMaster.Management.RealEstate.Types.Request;

namespace RentMaster.Management.RealEstate.Validators
{
    public class ApartmentRoomValidator : AbstractValidator<ApartmentRoomCreateRequest>
    {
        private readonly ApartmentRepository _apartmentRepository;
        private readonly AddressDivisionRepository _addressRepository;

        public ApartmentRoomValidator(ApartmentRepository apartmentRoomRepository,AddressDivisionRepository addressRepository)
        {
            _apartmentRepository = apartmentRoomRepository ?? 
                                   throw new ArgumentNullException(nameof(apartmentRoomRepository));
            _addressRepository = addressRepository ??
                                 throw new ArgumentNullException(nameof(addressRepository));
            RuleFor(x => x.ApartmentUid)
                .NotEmpty().WithMessage("ApartmentUid is required.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x)
                .Must(RoomDoesNotExist)
                .WithMessage("This room already exists in the apartment.");
        }

        // Province synchronous check
        private bool ProvinceExists(Guid provinceUid)
        {
            var provinces = _addressRepository.Filter(d => d.Type == "province");
            return provinces.Any(p => p.Uid == provinceUid);
        }

        // Ward synchronous check
        private bool WardExists(Guid wardUid)
        {
            var provinces = _addressRepository.Filter(d => d.Type == "ward");
            return provinces.Any(p => p.Uid == wardUid);
        }
        
        private bool RoomDoesNotExist(ApartmentRoomCreateRequest request)
        {
            if (request == null)
                return false;

            var exists = _apartmentRepository.Filter(
                a => a.Uid == request.ApartmentUid
            );

            return exists.Count()!=0;
        }
    }
}