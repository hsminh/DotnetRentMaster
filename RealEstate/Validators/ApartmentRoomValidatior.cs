using FluentValidation;
using RentMaster.RealEstate.Types.Request;

namespace RentMaster.RealEstate.Validators
{
    public class ApartmentRoomValidator : AbstractValidator<ApartmentRoomCreateRequest>
    {
        private readonly ApartmentRepository _apartmentRepository;

        public ApartmentRoomValidator(ApartmentRepository apartmentRoomRepository)
        {
            _apartmentRepository = apartmentRoomRepository ?? 
                                   throw new ArgumentNullException(nameof(apartmentRoomRepository));

            RuleFor(x => x.ApartmentUid)
                .NotEmpty().WithMessage("ApartmentUid is required.");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.");

            RuleFor(x => x)
                .Must(RoomDoesNotExist)
                .WithMessage("This room already exists in the apartment.");
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