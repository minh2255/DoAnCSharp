using System.Security.Claims;
using BakeryManagement.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BakeryManagement.Web.Controllers;

[Authorize]
public class OrdersController : Controller
{
    private readonly OrderService _orders;
    public OrdersController(OrderService orders) => _orders = orders;

    private int CustomerId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

    public IActionResult Index() => View(_orders.ByCustomer(CustomerId));

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Cancel(int id)
    {
        try
        {
            _orders.CancelByCustomer(id, CustomerId);
            TempData["Success"] = $"Đã hủy đơn #{id}.";
        }
        catch (InvalidOperationException ex) { TempData["Error"] = ex.Message; }
        return RedirectToAction(nameof(Index));
    }
}