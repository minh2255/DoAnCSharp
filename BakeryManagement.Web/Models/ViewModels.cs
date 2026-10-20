using BakeryManagement.Data;

namespace BakeryManagement.Web.Models;

public class ProductListVm
{
    public List<Product> Items { get; set; }
    public List<Category> Categories { get; set; }
    public string Keyword { get; set; }
    public int? CategoryId { get; set; }
    public int Page { get; set; }
    public int TotalPages { get; set; }
}
