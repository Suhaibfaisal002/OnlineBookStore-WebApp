using Online_Booking_Store.Models;

namespace Online_Booking_Store.Services;

public class DashboardStats
{
    public int TotalProducts { get; set; }
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int ProcessingOrders { get; set; }
    public int DispatchedOrders { get; set; }
    public int DeliveredOrders { get; set; }
    public int CancelledOrders { get; set; }
    public int TotalCustomers { get; set; }
    public int UnresolvedFeedbacks { get; set; }
    public decimal TotalRevenue { get; set; }
    public List<Order> RecentOrders { get; set; } = new();
}

public class PlaceOrderRequest
{
    public int UserId { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public string DeliveryType { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public string PaymentMode { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}

public class CancelOrderResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Order? Order { get; set; }
}

public class UpdateOrderRequest
{
    public int UserId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public decimal Distance { get; set; }
    public string DeliveryType { get; set; } = string.Empty;
    public string PaymentType { get; set; } = string.Empty;
    public string PaymentMode { get; set; } = string.Empty;
    public string? Remarks { get; set; }
}
