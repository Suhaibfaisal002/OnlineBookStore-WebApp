using Microsoft.AspNetCore.Http;

namespace Online_Booking_Store.Services;

public interface IFileStorageService
{
    Task<string?> SaveProductImageAsync(IFormFile? file, string? existingPath = null);
    void DeleteProductImage(string? existingPath);
    Task<string?> SaveProfileImageAsync(IFormFile? file, string? existingPath = null);
    void DeleteProfileImage(string? existingPath);
    Task<string?> SaveBrandLogoAsync(IFormFile? file);
}
