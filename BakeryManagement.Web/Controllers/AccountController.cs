using System.Security.Claims;
using BakeryManagement.Data;
using BakeryManagement.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace BakeryManagement.Web.Controllers;

public class AccountController : Controller
{
    private readonly AuthService _auth;
    public AccountController(AuthService auth) => _auth = auth;

    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        ViewBag.ReturnUrl = returnUrl;
        return View(new LoginVm());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm, string returnUrl = null)
    {
        if (!ModelState.IsValid) return View(vm);

        var c = _auth.LoginCustomer(vm.Email.Trim().ToLower(), vm.Password);
        if (c == null)
        {
            ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
            return View(vm);
        }
        await SignIn(c);
        return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Home");
    }

    [HttpGet] public IActionResult Register() => View(new RegisterVm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        try
        {
            var c = _auth.RegisterCustomer(vm.FullName, vm.Email, vm.Password, vm.Phone, vm.Address);
            await SignIn(c);   // đăng ký xong tự đăng nhập luôn
            return RedirectToAction("Index", "Home");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("Email", ex.Message);   // lỗi "email đã tồn tại"
            return View(vm);
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        return RedirectToAction("Index", "Home");
    }

    // Tạo cookie đăng nhập chứa ID và tên khách
    private Task SignIn(Customer c)
    {
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, c.CustomerId.ToString()),
            new Claim(ClaimTypes.Name, c.FullName)
        }, "Cookies");
        return HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(identity));
    }
}