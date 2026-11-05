using Online_Booking_Store.Models;

namespace Online_Booking_Store.Services;

public interface IOrderService
{
    string GenerateProductCode(string categoryCode, string manufacturerName, int serial);
    Task<string> GenerateOrderNumber();
    decimal CalculateDeliveryCharge(decimal distance, string deliveryType);
    decimal CalculateDiscount(int totalQuantity, decimal totalAmount);
    Task<Order> PlaceOrderAsync(string sessionId, PlaceOrderRequest request);
    Task<Order?> GetTrackedOrderAsync(string orderNumber, string email);
    Task<List<Order>> GetOrderHistoryAsync(int userId);
    Task<CancelOrderResult> CancelOrderAsync(string orderNumber, string email);
    Task<(bool Success, string Message)> UpdateOrderAsync(UpdateOrderRequest request);
    Task UpdateOrderStatusAsync(int orderId, string status, string? remarks = null);
    Task<DashboardStats> GetDashboardStatsAsync();
}
