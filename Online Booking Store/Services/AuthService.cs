using Microsoft.EntityFrameworkCore;
using Online_Booking_Store.Data;
using Online_Booking_Store.Infrastructure;
using Online_Booking_Store.Models;

namespace Online_Booking_Store.Services;

public class AuthService(ShradhaBookStoreContext context, IPasswordService passwordService) : IAuthService
{
    public async Task<(bool Success, string Message, User? User)> RegisterCustomerAsync(
        string fullName,
        string email,
        string password,
        string? phone,
        string? address,
        string? city)
    {
        var normalizedEmail = email.Trim();
        if (await context.Users.AnyAsync(x => x.Email == normalizedEmail))
        {
            return (false, "An account with this email already exists.", null);
        }

        var user = new User
        {
            FullName = fullName.Trim(),
            Email = normalizedEmail,
            Phone = phone?.Trim(),
            Address = address?.Trim(),
            City = city?.Trim(),
            Role = AppRoles.Customer,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };
        user.PasswordHash = passwordService.HashPassword(user, password);

        context.Users.Add(user);
        await context.SaveChangesAsync();

        return (true, "Registration completed successfully.", user);
    }

    public async Task<User?> AuthenticateAsync(string emailOrUsername, string password)
    {
        var normalized = emailOrUsername.Trim();
        var user = await context.Users.FirstOrDefaultAsync(x =>
            x.Email == normalized || (x.Username != null && x.Username == normalized));

        if (user is null || !user.IsActive)
        {
            return null;
        }

        return passwordService.VerifyPassword(user, user.PasswordHash, password) ? user : null;
    }
}
