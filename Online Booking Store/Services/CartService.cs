using Microsoft.EntityFrameworkCore;
using Online_Booking_Store.Data;
using Online_Booking_Store.Models;

namespace Online_Booking_Store.Services;

public class CartService(ShradhaBookStoreContext context) : ICartService
{
    public async Task AddToCartAsync(string sessionId, int? userId, int productId, int quantity)
    {
        quantity = Math.Max(quantity, 1);

        var cartItem = await context.Cart.FirstOrDefaultAsync(x =>
            x.ProductId == productId &&
            (userId.HasValue ? x.UserId == userId : x.UserId == null && x.SessionId == sessionId));

        if (cartItem is null)
        {
            context.Cart.Add(new CartItem
            {
                ProductId = productId,
                Quantity = quantity,
                SessionId = sessionId,
                UserId = userId,
                AddedDate = DateTime.UtcNow
            });
        }
        else
        {
            cartItem.Quantity += quantity;
        }

        await context.SaveChangesAsync();
    }

    public async Task<List<CartItem>> GetCartItemsAsync(string sessionId, int? userId)
    {
        return await context.Cart
            .Include(x => x.Product)
            .ThenInclude(x => x!.Category)
            .Where(x => userId.HasValue ? x.UserId == userId : x.UserId == null && x.SessionId == sessionId)
            .OrderByDescending(x => x.AddedDate)
            .ToListAsync();
    }

    public async Task UpdateQuantityAsync(int cartId, int quantity, string sessionId, int? userId)
    {
        var cartItem = await context.Cart.FirstOrDefaultAsync(x =>
            x.CartId == cartId &&
            (userId.HasValue ? x.UserId == userId : x.UserId == null && x.SessionId == sessionId));

        if (cartItem is null)
        {
            return;
        }

        if (quantity <= 0)
        {
            context.Cart.Remove(cartItem);
        }
        else
        {
            cartItem.Quantity = quantity;
        }

        await context.SaveChangesAsync();
    }

    public async Task RemoveItemAsync(int cartId, string sessionId, int? userId)
    {
        var cartItem = await context.Cart.FirstOrDefaultAsync(x =>
            x.CartId == cartId &&
            (userId.HasValue ? x.UserId == userId : x.UserId == null && x.SessionId == sessionId));

        if (cartItem is null)
        {
            return;
        }

        context.Cart.Remove(cartItem);
        await context.SaveChangesAsync();
    }

    public async Task MergeCartAsync(string sessionId, int userId)
    {
        var guestItems = await context.Cart
            .Where(x => x.UserId == null && x.SessionId == sessionId)
            .ToListAsync();

        foreach (var guestItem in guestItems)
        {
            var existingUserItem = await context.Cart.FirstOrDefaultAsync(x => x.UserId == userId && x.ProductId == guestItem.ProductId);
            if (existingUserItem is null)
            {
                guestItem.UserId = userId;
            }
            else
            {
                existingUserItem.Quantity += guestItem.Quantity;
                context.Cart.Remove(guestItem);
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task ClearCartAsync(string sessionId, int? userId)
    {
        var items = await context.Cart
            .Where(x => userId.HasValue ? x.UserId == userId : x.UserId == null && x.SessionId == sessionId)
            .ToListAsync();

        if (items.Count == 0)
        {
            return;
        }

        context.Cart.RemoveRange(items);
        await context.SaveChangesAsync();
    }
}
