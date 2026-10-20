using System.Diagnostics;
using BakeryManagement.Data;
using BakeryManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BakeryManagement.Web.Controllers;

public class HomeController : Controller
{
    private readonly ProductService _products;
    public HomeController(ProductService products) => _products = products;   // DI

    public IActionResult Index()
    {
        ViewBag.Categories = _products.Categories();   // gửi thêm danh mục sang View
        return View(_products.Latest(8));              // Model = 8 sản phẩm mới nhất
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}