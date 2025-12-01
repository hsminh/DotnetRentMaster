using RentMaster.Addresses.Models;
using RentMaster.Addresses.Repostiories;
namespace RentMaster.Addresses.Services;

public class AddressService
{
    private readonly AddressDivisionRepository _repo;

    public AddressService(AddressDivisionRepository repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<AddressDivision>> GetProvincesAsync()
    {
        return (await _repo.FilterAsync(d => d.Type == DivisionType.Province)).ToList();
    }

    public async Task<IReadOnlyList<AddressDivision>> GetChildrenByParentUidAsync(string parentUid, string? type = null)
    {
        var list = await _repo.FilterAsync(d =>
            d.ParentId == parentUid &&
            (type == null || d.Type.ToLower() == type.ToLower())
        );
        return list.ToList();
    }
}