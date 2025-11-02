using RentMaster.Accounts.Repositories;

namespace RentMaster.Accounts.Validator
{
    public class LandLordValidator
    {
        private readonly LandLordRepository _repository;

        public LandLordValidator(LandLordRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> ValidateGmailAsync(string gmail, Guid? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(gmail))
                return false;

            var existing = await _repository.FilterAsync(x => x.Gmail == gmail);

            if (excludeId.HasValue)
                existing = existing.Where(x => x.Uid != excludeId.Value);

            return !existing.Any();
        }
    }
}
