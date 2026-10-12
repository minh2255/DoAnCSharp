using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BakeryManagement.Data;

// Danh mục và sản phẩm 
public class Category
{
    public int CategoryId { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; }

    [StringLength(300)]
    public string Description { get; set; }

    public List<Product> Products { get; set; } = new();
}

public class Product
{
    public int ProductId { get; set; }

    [Required, StringLength(150)]
    public string Name { get; set; }

    [StringLength(1000)] public string Description { get; set; }
    public decimal Price { get; set; }
    [StringLength(300)] public string ImageUrl { get; set; }
    public int Stock { get; set; }
    public bool IsActive { get; set; } = true;

    public int CategoryId { get; set; }
    public Category Category { get; set; }
}

//Tài khoản và khách hàng
public class Role
{
    public int RoleId { get; set; }
    [Required, StringLength(50)] public string RoleName { get; set; }
    public List<User> Users { get; set; } = new();
}

// Tài khoản nhân viên/quản trị: dùng cho ứng dụng Desktop
public class User
{
    public int UserId { get; set; }
    [Required, StringLength(50)] public string Username { get; set; }
    [Required, StringLength(200)] public string PasswordHash { get; set; }
    [Required, StringLength(100)] public string FullName { get; set; }
    public bool IsActive { get; set; } = true;

    public int RoleId { get; set; }
    public Role Role { get; set; }
}

// Khách hàng đăng ký trên Web
public class Customer
{
    public int CustomerId { get; set; }
    [Required, StringLength(100)] public string FullName { get; set; }
    [Required, StringLength(100)] public string Email { get; set; }
    [Required, StringLength(200)] public string PasswordHash { get; set; }
    [StringLength(20)] public string Phone { get; set; }
    [StringLength(250)] public string Address { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public List<Order> Orders { get; set; } = new();
}

// Đơn hàng
public static class OrderStatus
{
    public const string Pending = "Chờ xác nhận";
    public const string Preparing = "Đang chuẩn bị";
    public const string Delivering = "Đang giao";
    public const string Completed = "Hoàn thành";
    public const string Cancelled = "Đã hủy";
    public static readonly string[] All = { Pending, Preparing, Delivering, Completed, Cancelled };
}

public class Order
{
    public int OrderId { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.Now;
    [Required, StringLength(30)] public string Status { get; set; } = OrderStatus.Pending;
    [Required, StringLength(250)] public string ShippingAddress { get; set; }
    [Required, StringLength(20)] public string Phone { get; set; }
    [StringLength(300)] public string Note { get; set; }

    public List<OrderDetail> Details { get; set; } = new();

    // Tổng tiền tính từ chi tiết, KHÔNG lưu vào DB
    [NotMapped]
    public decimal Total => Details.Sum(d => d.Quantity * d.UnitPrice);
}

public class OrderDetail
{
    public int OrderId { get; set; }
    public Order Order { get; set; }

    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }   // giá tại thời điểm đặt hàng
}
