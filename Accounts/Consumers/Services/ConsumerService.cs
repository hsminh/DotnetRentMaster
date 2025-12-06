using RentMaster.Accounts.Models;
using RentMaster.Accounts.Repositories;
using RentMaster.Accounts.Validator;
using RentMaster.Core.Exceptions;
using RentMaster.Core.Services;

namespace RentMaster.Accounts.Services
{
    public class ConsumerService : BaseService<Consumer>
    {
        private readonly ConsumerValidator _validator;
        private readonly ConsumerRepository _consumerRepository;

        public ConsumerService(ConsumerRepository repository, ConsumerValidator validator)
            : base(repository)
        {
            _validator = validator;
            _consumerRepository = repository;
        }

        public override async Task<Consumer> CreateAsync(Consumer model)
        {
            var isEmailValid = await _validator.ValidateGmailAsync(model.Gmail);
            if (!isEmailValid)
                throw new ValidationException("gmail", "Gmail already exists.");
            
            if (!string.IsNullOrEmpty(model.Password))
            {
                model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            }
            return await base.CreateAsync(model);
        }

        public override async Task UpdateAsync(Consumer model)
        {
            var existingConsumer = await _consumerRepository.FindByUidAsync(model.Uid);
            if (existingConsumer == null)
                throw new ValidationException("user", "Not Found");

            model.Gmail = existingConsumer.Gmail;
            model.Status = existingConsumer.Status;
            model.IsVerified = existingConsumer.IsVerified;
    
            if (string.IsNullOrEmpty(model.Password) || model.Password == "************")
            {
                model.Password = existingConsumer.Password;
            }
            else
            {
                model.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            }
            await base.UpdateAsync(model);
        }

        public async Task<bool> CheckAndVerifyAsync(Consumer consumer)
        {
            bool isDataComplete = !string.IsNullOrEmpty(consumer.FirstName) && 
                                  !string.IsNullOrEmpty(consumer.LastName) && 
                                  !string.IsNullOrEmpty(consumer.PhoneNumber);

            if (isDataComplete)
            {
                consumer.IsVerified = true;
            
                try
                {
                    await _consumerRepository.UpdateAsync(consumer);
                    return true; 
                }
                catch
                {
                    return false; 
                }
            }
            return false; 
        }
    }
}