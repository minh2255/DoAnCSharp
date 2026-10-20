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

    public IActionResult Details(int id)
    {
        var p = _products.Get(id);
        return p == null || !p.IsActive ? NotFound() : View(p);
    }
}