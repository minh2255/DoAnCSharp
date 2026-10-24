using System.Security.Claims;
using System.Text.Json;
using BakeryManagement.Data;
using BakeryManagement.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BakeryManagement.Web.Controllers;

public class CartController : Controller
{
    private readonly ProductService _products;
    private readonly OrderService _orders;
    private readonly AuthService _auth;

    public CartController(ProductService products, OrderService orders, AuthService auth)
    { _products = products; _orders = orders; _auth = auth; }

    private List<CartItem> LoadCart()
    {
        var s = HttpContext.Session.GetString("cart");
        return s == null ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(s);
    }
    private void SaveCart(List<CartItem> cart) =>
        HttpContext.Session.SetString("cart", JsonSerializer.Serialize(cart));

    private List<CartLine> Lines()
    {
        var lines = new List<CartLine>();
        foreach (var i in LoadCart())
        {
            var p = _products.Get(i.ProductId);
            if (p != null && p.IsActive) lines.Add(new CartLine { Product = p, Qty = i.Qty });
        }
        return lines;
    }

    public IActionResult Index() => View(Lines());

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Add(int id, int qty = 1, string returnUrl = null)
    {
        var p = _products.Get(id);
        if (p == null || !p.IsActive) return NotFound();

        var cart = LoadCart();
        var item = cart.FirstOrDefault(x => x.ProductId == id);
        var newQty = Math.Max(1, (item?.Qty ?? 0) + qty);

        if (newQty > p.Stock)
        {
            TempData["Error"] = $"'{p.Name}' chỉ còn {p.Stock} sản phẩm.";
            newQty = p.Stock;
        }
        if (newQty > 0)
        {
            if (item == null) cart.Add(new CartItem { ProductId = id, Qty = newQty });
            else item.Qty = newQty;
            SaveCart(cart);
            TempData["Success"] = $"Đã thêm '{p.Name}' vào giỏ hàng.";
        }
        return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Update(int id, int qty)
    {
        var cart = LoadCart();
        var item = cart.FirstOrDefault(x => x.ProductId == id);
        var p = _products.Get(id);
        if (item != null && p != null)
        {
            if (qty <= 0) cart.Remove(item);
            else item.Qty = Math.Min(qty, p.Stock);   // không vượt tồn kho
            SaveCart(cart);
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult Remove(int id)
    {
        var cart = LoadCart();
        cart.RemoveAll(x => x.ProductId == id);
        SaveCart(cart);
        return RedirectToAction(nameof(Index));
    }

    [Authorize, HttpGet]
    public IActionResult Checkout()
    {
        var lines = Lines();
        if (lines.Count == 0) return RedirectToAction(nameof(Index));

        var c = _auth.GetCustomer(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)));
        return View(new CheckoutVm { Lines = lines, Address = c?.Address, Phone = c?.Phone });  // điền sẵn thông tin
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public IActionResult Checkout(CheckoutVm vm)
    {
        vm.Lines = Lines();
        if (vm.Lines.Count == 0) return RedirectToAction(nameof(Index));
        if (!ModelState.IsValid) return View(vm);

        try
        {
            var cid = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var order = _orders.PlaceOrder(cid,
                vm.Lines.Select(l => (l.Product.ProductId, l.Qty)).ToList(),
                vm.Address, vm.Phone, vm.Note);

            SaveCart(new List<CartItem>());   // đặt xong thì làm rỗng giỏ
            TempData["Success"] = $"Đặt hàng thành công! Mã đơn #{order.OrderId}.";
            return RedirectToAction("Index", "Orders");
        }
        catch (InvalidOperationException ex)   // ví dụ: hết hàng giữa chừng
        {
            ModelState.AddModelError("", ex.Message);
            return View(vm);
        }
    }
}