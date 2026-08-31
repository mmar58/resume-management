namespace backend.Infrastructure.Storage;

/// <summary>
/// Abstraction for cloud file storage. The concrete implementation
/// (Cloudinary) is registered in DI — swap without changing callers.
/// </summary>
public interface IFileStorageService
{
    /// <summary>Uploads a file stream and returns the public URL.</summary>
    Task<string> UploadAsync(Stream fileStream, string fileName, string folder, CancellationToken ct = default);

    /// <summary>Deletes a file by its public ID or URL.</summary>
    Task DeleteAsync(string publicIdOrUrl, CancellationToken ct = default);
}
