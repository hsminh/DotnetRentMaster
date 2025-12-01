using RentMaster.Core.Backend.Auth.Types.enums;
using RentMaster.Core.Backend.Auth.Types.Response;
using RentMaster.Core.types.enums;

namespace RentMaster.Core.Backend.Auth.Interface;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string gmail, string password, UserTypes type);
}