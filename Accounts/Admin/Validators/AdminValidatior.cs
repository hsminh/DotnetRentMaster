using RentMaster.Accounts.Admin.Repositories;

namespace RentMaster.Accounts.Admin.Validators
{
    public class AdminValidator
    {
        private readonly AdminRepository _repository;

        public AdminValidator(AdminRepository repository)
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