namespace FridgeWatch.Application.Interfaces;

public interface IFileStorageService
{
    Task<(string PhotoUrl, string ThumbnailUrl)> SaveAsync(Stream fileStream, string fileName, string contentType);
    Task DeleteAsync(string photoUrl, string thumbnailUrl);
}
