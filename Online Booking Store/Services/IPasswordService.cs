using Online_Booking_Store.Models;

namespace Online_Booking_Store.Services;

public interface IPasswordService
{
    string HashPassword(User user, string password);
    bool VerifyPassword(User user, string hashedPassword, string password);
}
