using System.ComponentModel.DataAnnotations;
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

public class CartItem { public int ProductId { get; set; } public int Qty { get; set; } }

public class CartLine
{
    public Product Product { get; set; }
    public int Qty { get; set; }
    public decimal Total => Product.Price * Qty;
}

public class LoginVm
{
    [Required(ErrorMessage = "Nhập email"), EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Nhập mật khẩu"), DataType(DataType.Password)]
    public string Password { get; set; }
}

public class RegisterVm
{
    [Required(ErrorMessage = "Nhập họ tên")] public string FullName { get; set; }

    [Required(ErrorMessage = "Nhập email"), EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Nhập mật khẩu"), MinLength(6, ErrorMessage = "Tối thiểu 6 ký tự"), DataType(DataType.Password)]
    public string Password { get; set; }

    [Compare("Password", ErrorMessage = "Mật khẩu nhập lại không khớp"), DataType(DataType.Password)]
    public string ConfirmPassword { get; set; }

    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")] public string Phone { get; set; }
    public string Address { get; set; }
}

public class CheckoutVm
{
    [Required(ErrorMessage = "Nhập địa chỉ giao hàng")] public string Address { get; set; }

    [Required(ErrorMessage = "Nhập số điện thoại"), Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    public string Phone { get; set; }

    public string Note { get; set; }
    public List<CartLine> Lines { get; set; } = new();
}