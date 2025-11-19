using RentMaster.Addresses.Models;
using RentMaster.Addresses.Repostiories;
using RentMaster.Core.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RentMaster.Addresses.Services
{
    public class AddressService : BaseService<AddressDivision>
    {
        private readonly AddressDivisionRepository _repository;

        public AddressService(AddressDivisionRepository repository) 
            : base(repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<AddressDivision>> GetAddressAsync(string type, string? parentCode = null)
        {   
            if (string.IsNullOrEmpty(parentCode))
            {
                return await _repository.FilterAsync(d => d.Type == type);
            }
            return await _repository.FilterAsync(d => d.Type == type && d.ParentId == parentCode);
        }
    }
}