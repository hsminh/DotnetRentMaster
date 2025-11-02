using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using RentMaster.Core.types.enums;
using RentMaster.partner.Storage.Interface;

namespace RentMaster.Core.Cloudinary;

public class CloudinaryStorage : IFileStorage
{
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;

    public CloudinaryStorage(IConfiguration configuration)
    {
        var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_NAME") 
                        ?? configuration["Cloudinary:CloudName"];
        var apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY") 
                     ?? configuration["Cloudinary:ApiKey"];
        var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET") 
                        ?? configuration["Cloudinary:ApiSecret"];

        if (string.IsNullOrEmpty(cloudName) || string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiSecret))
            throw new InvalidOperationException("Missing Cloudinary credentials");

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new CloudinaryDotNet.Cloudinary(account)
        {
            Api = { Secure = true }
        };
    }

    public async Task<object> UploadAsync(IFormFile file, string folder, FileScope scope = FileScope.Public)
    {
        await using var stream = file.OpenReadStream();
        bool isImage = file.ContentType.StartsWith("image/");

        var uploadParams = isImage
            ? new ImageUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = true
            }
            : new RawUploadParams
            {
                File = new FileDescription(file.FileName, stream),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = true
            };

        var result = await _cloudinary.UploadAsync(uploadParams);
        return result;
    }

    public async Task DeleteAsync(string publicId)
    {
        var delParams = new DeletionParams(publicId);
        await _cloudinary.DestroyAsync(delParams);
    }

    public string GetUrl(string publicId)
    {
        return _cloudinary.Api.UrlImgUp.Secure(true).BuildUrl(publicId);
    }

    public string GetPrivateUrl(string publicId, TimeSpan expiry)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + (long)expiry.TotalSeconds;
        var signature = _cloudinary.Api.SignParameters(new SortedDictionary<string, object>
        {
            { "public_id", publicId },
            { "timestamp", timestamp }
        });

        return $"{_cloudinary.Api.UrlImgUp.Secure(true).BuildUrl(publicId)}?e={timestamp}&s={signature}";
    }
}
