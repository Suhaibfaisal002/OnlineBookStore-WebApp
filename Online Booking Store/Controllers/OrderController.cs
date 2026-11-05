using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Online_Booking_Store.Data;
using Online_Booking_Store.Infrastructure;
using Online_Booking_Store.Services;
using Online_Booking_Store.ViewModels;

namespace Online_Booking_Store.Controllers;

public class OrderController(
    IOrderService orderService,
    ICartService cartService,
    ShradhaBookStoreContext context) : AppController
{
    [SessionAuthorize(AppRoles.Customer)]
    [HttpGet]
    public async Task<IActionResult> PlaceOrder()
    {
        var items = await cartService.GetCartItemsAsync(GuestSessionId, CurrentUserId);
        if (items.Count == 0)
        {
            TempData["WarningMessage"] = "Your cart is empty.";
            return RedirectToAction("Index", "Cart");
        }

        return View(await BuildOrderViewModelAsync());
    }

    [SessionAuthorize(AppRoles.Customer)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmOrder(OrderViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("PlaceOrder", await BuildOrderViewModelAsync(model));
        }

        try
        {
            var order = await orderService.PlaceOrderAsync(GuestSessionId, new PlaceOrderRequest
            {
                UserId = CurrentUserId!.Value,
                DeliveryAddress = model.DeliveryAddress,
                Distance = model.Distance,
                DeliveryType = model.DeliveryType,
                PaymentType = model.PaymentType,
                PaymentMode = model.PaymentMode,
                Remarks = model.Remarks
            });

            TempData["SuccessMessage"] = "Order placed successfully.";
            return RedirectToAction(nameof(OrderSuccess), new { orderNumber = order.OrderNumber });
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return View("PlaceOrder", await BuildOrderViewModelAsync(model));
        }
    }

    public IActionResult OrderSuccess(string orderNumber)
    {
        ViewBag.OrderNumber = orderNumber;
        return View();
    }

    [HttpGet]
    public IActionResult TrackOrder()
    {
        return View(new TrackOrderViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TrackResult(TrackOrderViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("TrackOrder", model);
        }

        var order = await orderService.GetTrackedOrderAsync(model.OrderNumber, model.Email);
        if (order is null)
        {
            TempData["ErrorMessage"] = "No order found for the provided details.";
            return RedirectToAction(nameof(TrackOrder));
        }

        return View(order);
    }

    [HttpGet]
    public async Task<IActionResult> CancelOrder(string orderNumber, string email)
    {
        var order = await orderService.GetTrackedOrderAsync(orderNumber, email);
        if (order is null)
        {
            TempData["ErrorMessage"] = "Order not found.";
            return RedirectToAction(nameof(TrackOrder));
        }

        return View(order);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmCancel(string orderNumber, string email)
    {
        var result = await orderService.CancelOrderAsync(orderNumber, email);
        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return RedirectToAction(nameof(TrackOrder));
    }

    [SessionAuthorize(AppRoles.Customer)]
    public async Task<IActionResult> OrderHistory()
    {
        var orders = await orderService.GetOrderHistoryAsync(CurrentUserId!.Value);
        return View(orders);
    }

    [SessionAuthorize(AppRoles.Customer)]
    [HttpGet]
    public async Task<IActionResult> UpdateOrder(string orderNumber)
    {
        var order = await context.Orders.FirstOrDefaultAsync(x => x.OrderNumber == orderNumber && x.UserId == CurrentUserId);
        if (order is null)
        {
            TempData["ErrorMessage"] = "Order not found.";
            return RedirectToAction(nameof(OrderHistory));
        }

        if (order.OrderStatus is OrderStatuses.Dispatched or OrderStatuses.Delivered or OrderStatuses.Cancelled)
        {
            TempData["ErrorMessage"] = "This order can no longer be updated.";
            return RedirectToAction(nameof(OrderHistory));
        }

        var model = new UpdateOrderViewModel
        {
            OrderNumber = order.OrderNumber,
            DeliveryAddress = order.DeliveryAddress,
            Distance = order.Distance,
            DeliveryType = order.DeliveryType,
            PaymentType = order.PaymentType,
            Remarks = order.Remarks
        };

        return View(model);
    }

    [SessionAuthorize(AppRoles.Customer)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateOrder(UpdateOrderViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await orderService.UpdateOrderAsync(new UpdateOrderRequest
        {
            UserId = CurrentUserId!.Value,
            OrderNumber = model.OrderNumber,
            DeliveryAddress = model.DeliveryAddress,
            Distance = model.Distance,
            DeliveryType = model.DeliveryType,
            PaymentType = model.PaymentType,
            PaymentMode = model.PaymentMode,
            Remarks = model.Remarks
        });

        TempData[result.Success ? "SuccessMessage" : "ErrorMessage"] = result.Message;
        return result.Success
            ? RedirectToAction(nameof(OrderHistory))
            : View(model);
    }

    private async Task<OrderViewModel> BuildOrderViewModelAsync(OrderViewModel? existingModel = null)
    {
        var cartItems = await cartService.GetCartItemsAsync(GuestSessionId, CurrentUserId);
        var totalQuantity = cartItems.Sum(x => x.Quantity);
        var totalAmount = cartItems.Sum(x => (x.Product?.Price ?? 0) * x.Quantity);
        var distance = existingModel?.Distance ?? 0;
        var deliveryType = existingModel?.DeliveryType ?? DeliveryTypes.Standard;
        var deliveryCharge = orderService.CalculateDeliveryCharge(distance, deliveryType);
        var discount = orderService.CalculateDiscount(totalQuantity, totalAmount);

        return new OrderViewModel
        {
            DeliveryAddress = existingModel?.DeliveryAddress ??
                              (await context.Users.Where(x => x.UserId == CurrentUserId).Select(x => x.Address ?? string.Empty).FirstOrDefaultAsync()) ??
                              string.Empty,
            Distance = distance,
            DeliveryType = deliveryType,
            PaymentType = existingModel?.PaymentType ?? PaymentTypes.AfterDelivery,
            PaymentMode = existingModel?.PaymentMode ?? PaymentModes.Cash,
            Remarks = existingModel?.Remarks,
            Items = cartItems.Select(x => new OrderSummaryItemViewModel
            {
                ProductName = x.Product?.ProductName ?? "Unknown Product",
                Quantity = x.Quantity,
                UnitPrice = x.Product?.Price ?? 0,
                SubTotal = (x.Product?.Price ?? 0) * x.Quantity
            }).ToList(),
            TotalQuantity = totalQuantity,
            TotalProductAmount = totalAmount,
            DiscountAmount = discount,
            DeliveryCharge = deliveryCharge,
            GrandTotal = totalAmount - discount + deliveryCharge,
            DeliveryTypes = new List<SelectListItem>
            {
                new(DeliveryTypes.Standard, DeliveryTypes.Standard, deliveryType == DeliveryTypes.Standard),
                new(DeliveryTypes.Express, DeliveryTypes.Express, deliveryType == DeliveryTypes.Express)
            }
        };
    }
}
