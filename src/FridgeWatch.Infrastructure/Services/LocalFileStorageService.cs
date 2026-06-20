using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using FridgeWatch.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace FridgeWatch.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;
    private readonly string _baseUrlPrefix;
    private readonly ILogger<LocalFileStorageService> _logger;
    private const int ThumbnailMaxWidth = 200;
    private const int ThumbnailMaxHeight = 200;

    public LocalFileStorageService(IConfiguration configuration, ILogger<LocalFileStorageService> logger)
    {
        _logger = logger;
        _basePath = configuration["FileStorage:BasePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");
        _baseUrlPrefix = configuration["FileStorage:BaseUrlPrefix"] ?? "/uploads";

        Directory.CreateDirectory(Path.Combine(_basePath, "photos"));
        Directory.CreateDirectory(Path.Combine(_basePath, "thumbnails"));
    }

    public async Task<(string PhotoUrl, string ThumbnailUrl)> SaveAsync(Stream fileStream, string fileName, string contentType)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (ext != ".jpg" && ext != ".jpeg" && ext != ".png" && ext != ".gif" && ext != ".webp" && ext != ".bmp")
        {
            throw new Domain.Common.BusinessException("不支持的图片格式，仅支持 jpg/jpeg/png/gif/webp/bmp");
        }

        var id = Guid.NewGuid().ToString("N");
        var savedFileName = $"{id}{ext}";

        var photoPath = Path.Combine(_basePath, "photos", savedFileName);
        var thumbnailPath = Path.Combine(_basePath, "thumbnails", savedFileName);

        fileStream.Position = 0;
        using (var fs = new FileStream(photoPath, FileMode.Create))
        {
            await fileStream.CopyToAsync(fs);
        }

        try
        {
            fileStream.Position = 0;
            using var image = await Image.LoadAsync(fileStream);
            var clone = image.Clone(ctx => ctx.Resize(new ResizeOptions
            {
                Size = new Size(ThumbnailMaxWidth, ThumbnailMaxHeight),
                Mode = ResizeMode.Max
            }));
            await clone.SaveAsync(thumbnailPath);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "生成缩略图失败，将使用原图作为缩略图");
            fileStream.Position = 0;
            using (var fs = new FileStream(thumbnailPath, FileMode.Create))
            {
                await fileStream.CopyToAsync(fs);
            }
        }

        var photoUrl = $"{_baseUrlPrefix}/photos/{savedFileName}";
        var thumbnailUrl = $"{_baseUrlPrefix}/thumbnails/{savedFileName}";

        return (photoUrl, thumbnailUrl);
    }

    public Task DeleteAsync(string photoUrl, string thumbnailUrl)
    {
        if (!string.IsNullOrEmpty(photoUrl))
        {
            var photoFileName = Path.GetFileName(photoUrl);
            var photoPath = Path.Combine(_basePath, "photos", photoFileName);
            if (File.Exists(photoPath))
            {
                File.Delete(photoPath);
            }
        }

        if (!string.IsNullOrEmpty(thumbnailUrl))
        {
            var thumbnailFileName = Path.GetFileName(thumbnailUrl);
            var thumbnailPath = Path.Combine(_basePath, "thumbnails", thumbnailFileName);
            if (File.Exists(thumbnailPath))
            {
                File.Delete(thumbnailPath);
            }
        }

        return Task.CompletedTask;
    }
}
