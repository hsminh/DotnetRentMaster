using RentMaster.Core.Backend.Auth.Types.enums;

namespace RentMaster.Core.Backend.Auth.Interface;

public interface IAuthService
{
    Task<string?> LoginAsync(string gmail, string password, UserTypes type);
}