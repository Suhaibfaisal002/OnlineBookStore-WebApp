using Microsoft.AspNetCore.Identity;
using Online_Booking_Store.Models;

namespace Online_Booking_Store.Services;

public class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(User user, string password) => _passwordHasher.HashPassword(user, password);

    public bool VerifyPassword(User user, string hashedPassword, string password) =>
        _passwordHasher.VerifyHashedPassword(user, hashedPassword, password) != PasswordVerificationResult.Failed;
}
