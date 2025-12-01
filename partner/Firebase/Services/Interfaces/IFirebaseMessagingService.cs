using System.Threading.Tasks;

namespace RentMaster.partner.Firebase.Services.Interfaces
{
    public interface IFirebaseMessagingService
    {
        Task SendNotificationToUser(string userId, string title, string body, object data = null);
    }
}
