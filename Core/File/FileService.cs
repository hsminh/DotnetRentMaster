using CloudinaryDotNet.Actions;
using RentMaster.Core.Cloudinary;
using RentMaster.Core.types.enums;
using RentMaster.partner.Storage.Interface;

namespace RentMaster.Core.File;

public class FileService
{
    private readonly IFileStorage _storageProvider;
    private readonly FileScope _defaultScope = FileScope.Public;

    public FileService(IFileStorage storageProvider)
    {
        _storageProvider = storageProvider;
    }

    public async Task<(string PublicId, string Url)> UploadFileAsync(
        IFormFile file,
        Guid uploaderUid,
        FileType fileType = FileType.Image,
        FileScope? overrideScope = null)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File cannot be empty.");

        var scope = overrideScope ?? _defaultScope;
        var folder = $"rentmaster/{fileType.ToString().ToLower()}/{scope.ToString().ToLower()}/{uploaderUid}";

        var uploadResult = await _storageProvider.UploadAsync(file, folder, scope);

        string publicId = uploadResult switch
        {
            ImageUploadResult img => img.PublicId,
            RawUploadResult raw => raw.PublicId,
            _ => string.Empty
        };

        string url = uploadResult switch
        {
            ImageUploadResult img => img.Url?.ToString() ?? "",
            RawUploadResult raw => raw.Url?.ToString() ?? "",
            _ => ""
        };

        return (publicId, url);
    }

    public async Task DeleteFileAsync(string publicId)
    {
        await _storageProvider.DeleteAsync(publicId);
    }

    public string GetFileUrl(string publicId)
    {
        return _defaultScope == FileScope.Private
            ? GetPrivateFileUrl(publicId)
            : GetPublicFileUrl(publicId);
    }

    public string GetPublicFileUrl(string publicId)
    {
        return _storageProvider.GetUrl(publicId);
    }

    public string GetPrivateFileUrl(string publicId)
    {
        if (_storageProvider is CloudinaryStorage cloudStorage)
        {
            return cloudStorage.GetPrivateUrl(publicId, TimeSpan.FromHours(1));
        }

        throw new NotSupportedException("Private file URL generation is not supported for this storage provider.");
    }
}
