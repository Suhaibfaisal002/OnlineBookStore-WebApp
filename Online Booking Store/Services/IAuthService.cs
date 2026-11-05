using Online_Booking_Store.Models;

namespace Online_Booking_Store.Services;

public interface IAuthService
{
    Task<(bool Success, string Message, User? User)> RegisterCustomerAsync(
        string fullName,
        string email,
        string password,
        string? phone,
        string? address,
        string? city);

    Task<User?> AuthenticateAsync(string emailOrUsername, string password);
}
