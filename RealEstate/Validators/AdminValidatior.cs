using RentMaster.RealEstate.Services;

namespace RentMaster.RealEstate.Validators
{
    public class ApartmentValidator
    {
        private readonly ApartmentService _service;

        public ApartmentValidator(ApartmentService service)
        {
            this._service = service;
        }

        // public async Task<bool> ValidateGmailAsync(string gmail, Guid? excludeId = null)
        // {
        //     if (string.IsNullOrWhiteSpace(gmail))
        //         return false;
        //
        //     // var existing = await _service.FilterAsync(x => x.Gmail == gmail);
        //
        //     // if (excludeId.HasValue)
        //     //     existing = existing.Where(x => x.Uid != excludeId.Value);
        //     //
        //     // return !existing.Any();
        // }
    }
}