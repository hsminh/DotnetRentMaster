using RentMaster.Accounts.Consumers.Types;
using RentMaster.Accounts.Models;
using RentMaster.Accounts.Repositories;
using RentMaster.Accounts.Validator;
using RentMaster.Controllers;
using RentMaster.Core.Exceptions;
using RentMaster.Core.Services;
using RentMaster.Core.File;
using RentMaster.Core.types.enums;

namespace RentMaster.Accounts.Services
{
    public class ConsumerService : BaseService<Consumer>
    {
        private readonly ConsumerValidator _validator;
        private readonly ConsumerRepository _consumerRepository;
        private readonly FileService _fileService;
        public ConsumerService(ConsumerRepository repository, ConsumerValidator validator, FileService fileService)
            : base(repository)
        {
            _validator = validator;
            _consumerRepository = repository;
            _fileService = fileService;
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

        public async Task<Consumer> UpdateConsumerProfile(Guid uid, ConsumerRequest request)
        {
            var existingConsumer = await _consumerRepository.FindByUidAsync(uid);
            if (existingConsumer == null)
                throw new KeyNotFoundException("Consumer not found");

            if (request.Avatar != null)
            {
                var uploadResult = await _fileService.UploadFileAsync(
                    request.Avatar,
                    uid, 
                    FileType.Image,
                    FileScope.Public
                );
                existingConsumer.Avatar = uploadResult.Url;
            }
            existingConsumer.FirstName = request.FirstName;
            existingConsumer.LastName = request.LastName;
            existingConsumer.PhoneNumber = request.PhoneNumber;
            if (string.IsNullOrEmpty(request.Password) || request.Password == "************")
            {
                existingConsumer.Password = existingConsumer.Password;
            }
            else
            {
                existingConsumer.Password = BCrypt.Net.BCrypt.HashPassword(request.Password);
            }
            await base.UpdateAsync(existingConsumer);
            return existingConsumer;
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