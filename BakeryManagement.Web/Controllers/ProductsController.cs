using BakeryManagement.Data;
using BakeryManagement.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace BakeryManagement.Web.Controllers;

public class ProductsController : Controller
{
    private const int PageSize = 9;
    private readonly ProductService _products;
    public ProductsController(ProductService products) => _products = products;

    // /Products?q=bánh&cat=2&page=1
    public IActionResult Index(string q, int? cat, int page = 1)
    {
        if (page < 1) page = 1;

        var (items, total) = _products.Search(q, cat, page, PageSize, onlyActive: true);

        return View(new ProductListVm
        {
            Items = items,
            Categories = _products.Categories(),
            Keyword = q,
            CategoryId = cat,
            Page = page,
            TotalPages = Math.Max(1, (int)Math.Ceiling(total / (double)PageSize))
        });
    }

    // q, cat, page: bộ lọc của trang danh sách mà khách vừa rời đi, để nút "Quay lại danh sách" dựng lại đúng kết quả.
    public IActionResult Details(int id, string q, int? cat, int page = 1)
    {
        var p = _products.Get(id);
        if (p == null || !p.IsActive) return NotFound();
        ViewBag.BackQ = q;
        ViewBag.BackCat = cat;
        ViewBag.BackPage = page > 1 ? page : (int?)null;
        return View(p);
    }
}