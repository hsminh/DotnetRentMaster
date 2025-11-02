using RentMaster.Core.types.enums;

namespace RentMaster.partner.Storage.Interface;
public interface IFileStorage
{
    Task<object> UploadAsync(IFormFile file, string folder, FileScope scope);
    Task DeleteAsync(string publicId);
    string GetUrl(string publicId);
}