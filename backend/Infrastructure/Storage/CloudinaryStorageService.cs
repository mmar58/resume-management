using CloudinaryDotNet;
using CloudinaryDotNet.Actions;

namespace backend.Infrastructure.Storage;

/// <summary>
/// Cloudinary implementation of IFileStorageService.
/// Images are stored in Cloudinary — never in the database or web server (Section 26).
/// </summary>
public class CloudinaryStorageService : IFileStorageService
{
    private readonly Cloudinary _cloudinary;
    private readonly ILogger<CloudinaryStorageService> _logger;

    public CloudinaryStorageService(IConfiguration config, ILogger<CloudinaryStorageService> logger)
    {
        _logger = logger;
        var cloudName = config["Cloudinary:CloudName"]
            ?? throw new InvalidOperationException("Cloudinary:CloudName is not configured.");
        var apiKey = config["Cloudinary:ApiKey"]
            ?? throw new InvalidOperationException("Cloudinary:ApiKey is not configured.");
        var apiSecret = config["Cloudinary:ApiSecret"]
            ?? throw new InvalidOperationException("Cloudinary:ApiSecret is not configured.");

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string folder, CancellationToken ct = default)
    {
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(fileName, fileStream),
            Folder = folder,
            UseFilename = false,
            UniqueFilename = true,
            Overwrite = false
        };

        var result = await _cloudinary.UploadAsync(uploadParams);

        if (result.Error != null)
        {
            _logger.LogError("Cloudinary upload failed: {Error}", result.Error.Message);
            throw new InvalidOperationException($"File upload failed: {result.Error.Message}");
        }

        return result.SecureUrl.ToString();
    }

    public async Task DeleteAsync(string publicIdOrUrl, CancellationToken ct = default)
    {
        // Extract public ID from URL if a full URL is provided
        var publicId = publicIdOrUrl.Contains("cloudinary.com")
            ? ExtractPublicId(publicIdOrUrl)
            : publicIdOrUrl;

        var deleteParams = new DeletionParams(publicId);
        var result = await _cloudinary.DestroyAsync(deleteParams);

        if (result.Result != "ok")
        {
            _logger.LogWarning("Cloudinary delete returned non-ok result for {PublicId}: {Result}", publicId, result.Result);
        }
    }

    private static string ExtractPublicId(string url)
    {
        // Extract from: https://res.cloudinary.com/{cloud}/image/upload/v{version}/{folder}/{public_id}.ext
        var uri = new Uri(url);
        var segments = uri.AbsolutePath.Split('/');
        var uploadIndex = Array.IndexOf(segments, "upload");
        if (uploadIndex < 0) return url;

        // Skip version segment (v12345)
        var start = uploadIndex + 1;
        if (start < segments.Length && segments[start].StartsWith('v') && int.TryParse(segments[start][1..], out _))
            start++;

        var pathWithExt = string.Join("/", segments[start..]);
        return Path.ChangeExtension(pathWithExt, null);
    }
}
