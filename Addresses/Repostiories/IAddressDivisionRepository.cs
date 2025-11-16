
using RentMaster.Addresses.Models;

namespace RentMaster.Addresses.Data
{
    public interface IAddressDivisionRepository
    {
        Task<AddressDivision> GetByIdAsync(string id);
        Task AddAsync(AddressDivision division);
        Task UpdateAsync(string id, AddressDivision division);
        Task DeleteAsync(string id);
    }
}
