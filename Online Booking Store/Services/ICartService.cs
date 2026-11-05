using Online_Booking_Store.Models;

namespace Online_Booking_Store.Services;

public interface ICartService
{
    Task AddToCartAsync(string sessionId, int? userId, int productId, int quantity);
    Task<List<CartItem>> GetCartItemsAsync(string sessionId, int? userId);
    Task UpdateQuantityAsync(int cartId, int quantity, string sessionId, int? userId);
    Task RemoveItemAsync(int cartId, string sessionId, int? userId);
    Task MergeCartAsync(string sessionId, int userId);
    Task ClearCartAsync(string sessionId, int? userId);
}
