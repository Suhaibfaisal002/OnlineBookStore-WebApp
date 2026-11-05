using Microsoft.AspNetCore.Http;

namespace Online_Booking_Store.Services;

public class FileStorageService(IWebHostEnvironment environment) : IFileStorageService
{
    private readonly string _productImagesPath = Path.Combine(environment.WebRootPath, "Images", "Products");
    private readonly string _profileImagesPath = Path.Combine(environment.WebRootPath, "Images", "Profiles");
    private readonly string _brandImagesPath = Path.Combine(environment.WebRootPath, "Images", "Brand");
    private readonly IWebHostEnvironment _environment = environment;

    public async Task<string?> SaveProductImageAsync(IFormFile? file, string? existingPath = null)
    {
        if (file is null || file.Length == 0)
        {
            return existingPath;
        }

        Directory.CreateDirectory(_productImagesPath);
        DeleteProductImage(existingPath);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_productImagesPath, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/Images/Products/{fileName}";
    }

    public void DeleteProductImage(string? existingPath)
    {
        if (string.IsNullOrWhiteSpace(existingPath))
        {
            return;
        }

        var trimmed = existingPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_environment.WebRootPath, trimmed);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    public async Task<string?> SaveProfileImageAsync(IFormFile? file, string? existingPath = null)
    {
        if (file is null || file.Length == 0)
        {
            return existingPath;
        }

        Directory.CreateDirectory(_profileImagesPath);
        DeleteProfileImage(existingPath);

        var extension = Path.GetExtension(file.FileName);
        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(_profileImagesPath, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/Images/Profiles/{fileName}";
    }

    public void DeleteProfileImage(string? existingPath)
    {
        if (string.IsNullOrWhiteSpace(existingPath))
        {
            return;
        }

        var trimmed = existingPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_environment.WebRootPath, trimmed);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    public async Task<string?> SaveBrandLogoAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0)
        {
            return null;
        }

        Directory.CreateDirectory(_brandImagesPath);

        var fileName = "store-logo.png";
        var fullPath = Path.Combine(_brandImagesPath, fileName);

        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/Images/Brand/{fileName}";
    }
}
