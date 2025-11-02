using RentMaster.Accounts.Repositories;
using RentMaster.Accounts.Validator;
using RentMaster.Core.Services;
using RentMaster.Core.Exceptions;

namespace RentMaster.Accounts.Services
{
    public class AdminService : BaseService<Models.Admin>
    {
        private readonly AdminValidator _validator;

        public AdminService(AdminRepository repository, AdminValidator validator)
            : base(repository)
        {
            _validator = validator;
        }

        public override async Task<Models.Admin> CreateAsync(Models.Admin model)
        {
            var isValid = await _validator.ValidateGmailAsync(model.Gmail);
            if (!isValid)
                throw new ValidationException("gmail", "Gmail already exists.");
            
            if (!string.IsNullOrEmpty(model.Password))
            {
                model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            }
            return await base.CreateAsync(model);
        }

        public override async Task UpdateAsync(Models.Admin model)
        {
            var isValid = await _validator.ValidateGmailAsync(model.Gmail, model.Uid);
            if (!isValid)
                throw new ValidationException("gmail", "Gmail already exists for another user.");

            await base.UpdateAsync(model);
        }
    }
}