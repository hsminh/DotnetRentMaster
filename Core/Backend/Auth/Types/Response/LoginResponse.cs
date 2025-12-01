namespace RentMaster.Core.Backend.Auth.Types.Response;

public class LoginResponse
{
    public string Token { get; set; }
    public object User { get; set; }
    
    public LoginResponse(string token, object user)
    {
        Token = token;
        User = user;
    }
}
