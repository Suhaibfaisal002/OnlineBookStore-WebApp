namespace Online_Booking_Store.Infrastructure;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Customer = "Customer";
}

public static class SessionKeys
{
    public const string UserId = "UserId";
    public const string UserName = "UserName";
    public const string UserRole = "UserRole";
    public const string GuestSessionId = "SessionID";
}

public static class DeliveryTypes
{
    public const string Standard = "Standard";
    public const string Express = "Express";
}

public static class OrderStatuses
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Dispatched = "Dispatched";
    public const string Delivered = "Delivered";
    public const string Cancelled = "Cancelled";
}

public static class PaymentTypes
{
    public const string BeforeDelivery = "BeforeDelivery";
    public const string AfterDelivery = "AfterDelivery";
    public const string OnlinePayment = "OnlinePayment";
}

public static class PaymentModes
{
    public const string Cash = "Cash";
    public const string Cheque = "Cheque";
    public const string CreditCard = "CreditCard";
    public const string DemandDraft = "DemandDraft";
}

public static class PaymentStatuses
{
    public const string Pending = "Pending";
    public const string Paid = "Paid";
    public const string Failed = "Failed";
    public const string Completed = "Completed";
}
