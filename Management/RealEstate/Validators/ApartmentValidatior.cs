using FluentValidation;
using RentMaster.Addresses.Repostiories;
using RentMaster.Management.RealEstate.Types.Request;

namespace RentMaster.Management.RealEstate.Validators
{
    public class ApartmentValidator : AbstractValidator<ApartmentCreateRequest>
    {
        private readonly AddressDivisionRepository _addressRepository;

        public ApartmentValidator(
            AddressDivisionRepository addressRepository)
        {
            _addressRepository = addressRepository ??
                                 throw new ArgumentNullException(nameof(addressRepository));

            // Title
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title is required.")
                .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

            // Description
            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description cannot exceed 2000 characters.");

            // Price
            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than 0.")
                .LessThanOrEqualTo(1000000).WithMessage("Price cannot exceed 1,000,000.");

            // Area
            RuleFor(x => x.AreaLength)
                .GreaterThan(0).WithMessage("Area length must be greater than 0.")
                .LessThanOrEqualTo(1000).WithMessage("Area length cannot exceed 1000.");

            RuleFor(x => x.AreaWidth)
                .GreaterThan(0).WithMessage("Area width must be greater than 0.")
                .LessThanOrEqualTo(1000).WithMessage("Area width cannot exceed 1000.");

            // Province validation
            RuleFor(x => x.ProvinceDivisionUid)
                .NotEmpty().WithMessage("Province is required.")
                .Must(ProvinceExists).WithMessage("Invalid province ID.");

            // Ward validation
            RuleFor(x => x.WardDivisionUid)
                .NotEmpty().WithMessage("Ward is required.")
                .Must(WardExists).WithMessage("Invalid ward ID or ward does not belong to the specified province.");

            // Ward validation
            RuleFor(x => x.StreetUid)
                .NotEmpty().WithMessage("Street is required.")
                .Must(StreetExists).WithMessage("Invalid street ID or ward does not belong to the specified province.");

            // MetaData
            RuleFor(x => x.MetaData)
                .NotEmpty().WithMessage("MetaData is required.")
                .MaximumLength(4000).WithMessage("MetaData cannot exceed 4000 characters.");

            // Files
            RuleFor(x => x.Files)
                .Must(files => files == null || files.Count <= 10)
                .WithMessage("Maximum 10 files are allowed.")
                .Must(files => files == null || files.All(f =>
                    f.Length <= 5 * 1024 * 1024 && // 5MB
                    (f.ContentType.StartsWith("image/") || f.ContentType == "application/pdf")))
                .WithMessage("Only image and PDF files up to 5MB are allowed.");
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
            var ward = _addressRepository.Filter(d => d.Type == "ward");
            return ward.Any(p => p.Uid == wardUid);
        }
        // Street synchronous check
        private bool StreetExists(Guid? streetUid)
        {
            if (!streetUid.HasValue) return false;
            var street = _addressRepository.Filter(d => d.Type == "street");
            return street.Any(p => p.Uid == streetUid.Value);
        }


    }
}
