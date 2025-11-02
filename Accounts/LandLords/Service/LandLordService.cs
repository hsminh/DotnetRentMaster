using RentMaster.Accounts.Models;
using RentMaster.Accounts.Repositories;
using RentMaster.Accounts.Validator;
using RentMaster.Core.Services;

namespace RentMaster.Accounts.Services
{
    public class LandLordService : BaseService<LandLord>
    {
        private readonly LandLordValidator _validator;

        public LandLordService(LandLordRepository repository, LandLordValidator validator)
            : base(repository)
        {
            _validator = validator;
        }

        public override async Task<LandLord> CreateAsync(LandLord model)
        {
            var isValid = await _validator.ValidateGmailAsync(model.Gmail);
            if (!isValid)
                throw new Exception("Gmail already exists.");

            if (!string.IsNullOrEmpty(model.Password))
            {
                model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            }

            return await base.CreateAsync(model);
        }

        public override async Task UpdateAsync(LandLord model)
        {
            var isValid = await _validator.ValidateGmailAsync(model.Gmail, model.Uid);
            if (!isValid)
                throw new Exception("Gmail already exists for another user.");

            if (!string.IsNullOrEmpty(model.Password))
            {
                model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            }

            await base.UpdateAsync(model);
        }
    }
}