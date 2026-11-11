using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BakeryManagement.Data;
using BakeryManagement.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

namespace BakeryManagement.Web.Controllers;

public class AccountController : Controller
{
    private readonly AuthService _auth;
    private readonly IDataProtectionProvider _dp;
    private readonly IWebHostEnvironment _env;
    public AccountController(AuthService auth, IDataProtectionProvider dp, IWebHostEnvironment env)
    { _auth = auth; _dp = dp; _env = env; }

    private int CurrentId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));

    // Đăng nhập / đăng ký / đăng xuất
    [HttpGet] public IActionResult Login(string returnUrl = null) { ViewBag.ReturnUrl = returnUrl; return View(new LoginVm()); }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm, string returnUrl = null)
    {
        if (!ModelState.IsValid) return View(vm);
        try
        {
            var c = _auth.LoginCustomer(vm.Email.Trim().ToLower(), vm.Password);
            if (c == null) { ModelState.AddModelError("", "Email hoặc mật khẩu không đúng."); return View(vm); }
            await SignIn(c);
            return Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Home");
        }
        catch (InvalidOperationException ex) { ModelState.AddModelError("", ex.Message); return View(vm); }
    }

    [HttpGet] public IActionResult Register() => View(new RegisterVm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        try
        {
            var c = _auth.RegisterCustomer(vm.FullName, vm.Email, vm.Password, vm.Phone, vm.Address);
            await SignIn(c);
            return RedirectToAction("Index", "Home");
        }
        catch (InvalidOperationException ex) { ModelState.AddModelError("Email", ex.Message); return View(vm); }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("Cookies");
        return RedirectToAction("Index", "Home");
    }

    // Hồ sơ của tôi
    [Authorize, HttpGet]
    public IActionResult Profile()
    {
        var c = _auth.GetCustomer(CurrentId);
        if (c == null) return NotFound();
        return View(new ProfileVm { Email = c.Email, FullName = c.FullName, Phone = c.Phone, Address = c.Address });
    }

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(ProfileVm vm)
    {
        var c = _auth.GetCustomer(CurrentId);
        if (c == null) return NotFound();
        vm.Email = c.Email;                       // email không đổi được
        if (!ModelState.IsValid) return View(vm);
        try
        {
            _auth.UpdateProfile(c.CustomerId, vm.FullName, vm.Phone, vm.Address);
            await SignIn(_auth.GetCustomer(c.CustomerId));   // cập nhật tên hiển thị trên thanh menu
            TempData["Success"] = "Đã cập nhật hồ sơ.";
            return RedirectToAction(nameof(Profile));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        { ModelState.AddModelError("", ex.Message); return View(vm); }
    }

    // Đổi mật khẩu
    [Authorize, HttpGet] public IActionResult ChangePassword() => View(new ChangePasswordVm());

    [Authorize, HttpPost, ValidateAntiForgeryToken]
    public IActionResult ChangePassword(ChangePasswordVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        try
        {
            _auth.ChangePassword(CurrentId, vm.CurrentPassword, vm.NewPassword);
            TempData["Success"] = "Đổi mật khẩu thành công.";
            return RedirectToAction(nameof(Profile));
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        { ModelState.AddModelError("", ex.Message); return View(vm); }
    }

    // Quên mật khẩu
    [HttpGet] public IActionResult ForgotPassword() => View(new ForgotPasswordVm());

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(ForgotPasswordVm vm)
    {
        if (!ModelState.IsValid) return View(vm);
        var c = _auth.FindCustomerByEmail(vm.Email);
        ViewBag.Sent = true;      // luôn báo như nhau để không lộ email nào đã đăng ký
        // Chưa có máy chủ email, nên khi phát triển hiển thị liên kết trên màn hình thay cho email.
        if (c != null && _env.IsDevelopment())
            ViewBag.DevLink = Url.Action(nameof(ResetPassword), "Account", new { token = MakeToken(c) }, Request.Scheme);
        return View(vm);
    }

    [HttpGet]
    public IActionResult ResetPassword(string token)
    {
        if (!TryReadToken(token, out _))
        {
            TempData["Error"] = "Liên kết không hợp lệ hoặc đã hết hạn.";
            return RedirectToAction(nameof(ForgotPassword));
        }
        return View(new ResetPasswordVm { Token = token });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public IActionResult ResetPassword(ResetPasswordVm vm)
    {
        if (!TryReadToken(vm.Token, out var c))
        {
            TempData["Error"] = "Liên kết không hợp lệ hoặc đã hết hạn.";
            return RedirectToAction(nameof(ForgotPassword));
        }
        if (!ModelState.IsValid) return View(vm);
        _auth.ResetPassword(c.CustomerId, vm.NewPassword);
        TempData["Success"] = "Đã đặt lại mật khẩu. Hãy đăng nhập bằng mật khẩu mới.";
        return RedirectToAction(nameof(Login));
    }

    // Hỗ trợ
    // Token: mã khách + dấu vân tay của mật khẩu hiện tại, hết hạn sau 15 phút.
    // Đổi mật khẩu xong thì dấu vân tay đổi nên token cũ tự vô hiệu (dùng một lần).
    private Microsoft.AspNetCore.DataProtection.ITimeLimitedDataProtector Protector
        => _dp.CreateProtector("PasswordReset").ToTimeLimitedDataProtector();

    private static string Fingerprint(Customer c)
        => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(c.PasswordHash)))[..16];

    private string MakeToken(Customer c)
        => Protector.Protect($"{c.CustomerId}|{Fingerprint(c)}", TimeSpan.FromMinutes(15));

    private bool TryReadToken(string token, out Customer c)
    {
        c = null;
        if (string.IsNullOrEmpty(token)) return false;
        try
        {
            var parts = Protector.Unprotect(token).Split('|');
            c = _auth.GetCustomer(int.Parse(parts[0]));
            return c != null && Fingerprint(c) == parts[1];
        }
        catch { return false; }
    }

    private Task SignIn(Customer c)
    {
        var id = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, c.CustomerId.ToString()),
            new Claim(ClaimTypes.Name, c.FullName)
        }, "Cookies");
        return HttpContext.SignInAsync("Cookies", new ClaimsPrincipal(id));
    }
}