using Microsoft.EntityFrameworkCore;
using Online_Booking_Store.Data;
using Online_Booking_Store.Infrastructure;
using Online_Booking_Store.Models;

namespace Online_Booking_Store.Services;

public class OrderService(ShradhaBookStoreContext context, ICartService cartService) : IOrderService
{
    public string GenerateProductCode(string categoryCode, string manufacturerName, int serial)
    {
        var acronym = new string(
            manufacturerName
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => char.ToUpperInvariant(x[0]))
                .Take(3)
                .ToArray());

        acronym = acronym.PadRight(3, '0');
        return $"{categoryCode}{acronym}{serial:00}";
    }

    public async Task<string> GenerateOrderNumber()
    {
        var totalOrders = await context.Orders.CountAsync();
        return (totalOrders + 1).ToString("D8");
    }

    public decimal CalculateDeliveryCharge(decimal distance, string deliveryType)
    {
        if (distance <= 3)
        {
            return 0;
        }

        return string.Equals(deliveryType, DeliveryTypes.Express, StringComparison.OrdinalIgnoreCase) ? 100 : 50;
    }

    public decimal CalculateDiscount(int totalQuantity, decimal totalAmount)
    {
        return totalQuantity switch
        {
            >= 50 => Math.Round(totalAmount * 0.15m, 2),
            >= 20 => Math.Round(totalAmount * 0.10m, 2),
            >= 10 => Math.Round(totalAmount * 0.05m, 2),
            _ => 0
        };
    }

    public async Task<Order> PlaceOrderAsync(string sessionId, PlaceOrderRequest request)
    {
        var cartItems = await cartService.GetCartItemsAsync(sessionId, request.UserId);
        if (cartItems.Count == 0)
        {
            throw new InvalidOperationException("Your cart is empty.");
        }

        if (cartItems.Any(x => x.Product is null || !x.Product.IsActive || x.Product.StockQuantity < x.Quantity))
        {
            throw new InvalidOperationException("One or more products are unavailable in the requested quantity.");
        }

        var totalQuantity = cartItems.Sum(x => x.Quantity);
        var totalProductAmount = cartItems.Sum(x => (x.Product?.Price ?? 0) * x.Quantity);
        var discount = CalculateDiscount(totalQuantity, totalProductAmount);
        var deliveryCharge = CalculateDeliveryCharge(request.Distance, request.DeliveryType);
        var grandTotal = totalProductAmount - discount + deliveryCharge;

        var order = new Order
        {
            OrderNumber = await GenerateOrderNumber(),
            UserId = request.UserId,
            OrderDate = DateTime.UtcNow,
            TotalProductAmount = totalProductAmount,
            DeliveryCharge = deliveryCharge,
            GrandTotal = grandTotal,
            Distance = request.Distance,
            DeliveryType = request.DeliveryType,
            DeliveryAddress = request.DeliveryAddress,
            OrderStatus = request.PaymentType == PaymentTypes.AfterDelivery ? OrderStatuses.Processing : OrderStatuses.Pending,
            PaymentType = request.PaymentType,
            PaymentStatus = PaymentStatuses.Pending,
            CancelCharge = 0,
            DiscountAmount = discount,
            Remarks = request.Remarks,
            CreatedDate = DateTime.UtcNow
        };

        foreach (var item in cartItems)
        {
            var unitPrice = item.Product!.Price;
            order.OrderItems.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = unitPrice,
                SubTotal = unitPrice * item.Quantity
            });

            item.Product.StockQuantity -= item.Quantity;
        }

        var paymentMode = request.PaymentType switch
        {
            PaymentTypes.BeforeDelivery => PaymentModes.DemandDraft,
            PaymentTypes.OnlinePayment => PaymentModes.CreditCard,
            _ => request.PaymentMode
        };

        order.Payments.Add(new Payment
        {
            PaymentDate = DateTime.UtcNow,
            Amount = grandTotal,
            PaymentMode = paymentMode,
            Status = PaymentStatuses.Pending,
            TransactionRef = request.PaymentType == PaymentTypes.OnlinePayment ? $"CC-{Guid.NewGuid():N}" : null
        });

        context.Orders.Add(order);
        await context.SaveChangesAsync();
        await cartService.ClearCartAsync(sessionId, request.UserId);

        return order;
    }

    public async Task<Order?> GetTrackedOrderAsync(string orderNumber, string email)
    {
        return await context.Orders
            .Include(x => x.User)
            .Include(x => x.OrderItems)
            .ThenInclude(x => x.Product)
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.OrderNumber == orderNumber && x.User != null && x.User.Email == email);
    }

    public async Task<List<Order>> GetOrderHistoryAsync(int userId)
    {
        return await context.Orders
            .Include(x => x.OrderItems)
            .ThenInclude(x => x.Product)
            .Where(x => x.UserId == userId)
            .OrderByDescending(x => x.OrderDate)
            .ToListAsync();
    }

    public async Task<CancelOrderResult> CancelOrderAsync(string orderNumber, string email)
    {
        var order = await context.Orders
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.OrderNumber == orderNumber);

        if (order is null || order.User?.Email != email)
        {
            return new CancelOrderResult { Message = "Order not found for the given email." };
        }

        if (order.OrderStatus == OrderStatuses.Delivered)
        {
            return new CancelOrderResult { Message = "Delivered orders cannot be cancelled." };
        }

        if (order.OrderStatus == OrderStatuses.Cancelled)
        {
            return new CancelOrderResult { Message = "This order has already been cancelled." };
        }

        var isWithin24Hours = DateTime.UtcNow <= order.OrderDate.AddHours(24);
        var cancelCharge = 0m;

        if (!isWithin24Hours && order.OrderStatus == OrderStatuses.Dispatched)
        {
            cancelCharge = order.DeliveryCharge * 2;
        }

        order.OrderStatus = OrderStatuses.Cancelled;
        order.CancelledDate = DateTime.UtcNow;
        order.CancelCharge = cancelCharge;
        order.Remarks = string.IsNullOrWhiteSpace(order.Remarks)
            ? "Order cancelled by customer."
            : $"{order.Remarks} | Order cancelled by customer.";

        await context.SaveChangesAsync();

        return new CancelOrderResult
        {
            Success = true,
            Message = cancelCharge > 0
                ? $"Order cancelled. Cancellation charge applied: {cancelCharge:C}."
                : "Order cancelled successfully.",
            Order = order
        };
    }

    public async Task<(bool Success, string Message)> UpdateOrderAsync(UpdateOrderRequest request)
    {
        var order = await context.Orders.FirstOrDefaultAsync(x => x.OrderNumber == request.OrderNumber && x.UserId == request.UserId);
        if (order is null)
        {
            return (false, "Order not found.");
        }

        if (order.OrderStatus is OrderStatuses.Dispatched or OrderStatuses.Delivered or OrderStatuses.Cancelled)
        {
            return (false, "This order can no longer be updated.");
        }

        order.DeliveryAddress = request.DeliveryAddress;
        order.Distance = request.Distance;
        order.DeliveryType = request.DeliveryType;
        order.PaymentType = request.PaymentType;
        order.DeliveryCharge = CalculateDeliveryCharge(request.Distance, request.DeliveryType);
        order.GrandTotal = order.TotalProductAmount - order.DiscountAmount + order.DeliveryCharge;
        order.Remarks = request.Remarks;

        var payment = await context.Payments.FirstOrDefaultAsync(x => x.OrderId == order.OrderId);
        if (payment is not null)
        {
            payment.Amount = order.GrandTotal;
            payment.PaymentMode = request.PaymentType switch
            {
                PaymentTypes.BeforeDelivery => PaymentModes.DemandDraft,
                PaymentTypes.OnlinePayment => PaymentModes.CreditCard,
                _ => request.PaymentMode
            };
        }

        await context.SaveChangesAsync();
        return (true, "Order updated successfully.");
    }

    public async Task UpdateOrderStatusAsync(int orderId, string status, string? remarks = null)
    {
        var order = await context.Orders
            .Include(x => x.Payments)
            .FirstOrDefaultAsync(x => x.OrderId == orderId);

        if (order is null)
        {
            return;
        }

        order.OrderStatus = status;
        if (!string.IsNullOrWhiteSpace(remarks))
        {
            order.Remarks = remarks;
        }

        if (status == OrderStatuses.Delivered && order.PaymentType == PaymentTypes.AfterDelivery)
        {
            order.PaymentStatus = PaymentStatuses.Paid;
            foreach (var payment in order.Payments)
            {
                payment.Status = PaymentStatuses.Completed;
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task<DashboardStats> GetDashboardStatsAsync()
    {
        var totalOrders = await context.Orders.CountAsync();
        var totalRevenue = totalOrders == 0
            ? 0
            : await context.Orders.SumAsync(x => x.GrandTotal);

        return new DashboardStats
        {
            TotalProducts = await context.Products.CountAsync(),
            TotalOrders = totalOrders,
            PendingOrders = await context.Orders.CountAsync(x => x.OrderStatus == OrderStatuses.Pending),
            ProcessingOrders = await context.Orders.CountAsync(x => x.OrderStatus == OrderStatuses.Processing),
            DispatchedOrders = await context.Orders.CountAsync(x => x.OrderStatus == OrderStatuses.Dispatched),
            DeliveredOrders = await context.Orders.CountAsync(x => x.OrderStatus == OrderStatuses.Delivered),
            CancelledOrders = await context.Orders.CountAsync(x => x.OrderStatus == OrderStatuses.Cancelled),
            TotalCustomers = await context.Users.CountAsync(x => x.Role == AppRoles.Customer),
            UnresolvedFeedbacks = await context.FeedbackEntries.CountAsync(x => !x.IsResolved),
            TotalRevenue = totalRevenue,
            RecentOrders = await context.Orders
                .Include(x => x.User)
                .OrderByDescending(x => x.OrderDate)
                .Take(8)
                .ToListAsync()
        };
    }
}
