using Microsoft.EntityFrameworkCore;

namespace BakeryManagement.Data;

public class BakeryDbContext : DbContext
{
    public BakeryDbContext(DbContextOptions<BakeryDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Các cột phải duy nhất
        b.Entity<Role>().HasIndex(x => x.RoleName).IsUnique();
        b.Entity<User>().HasIndex(x => x.Username).IsUnique();
        b.Entity<Customer>().HasIndex(x => x.Email).IsUnique();
        b.Entity<Category>().HasIndex(x => x.Name).IsUnique();
        // Kiểu tiền tệ
        b.Entity<Product>().Property(x => x.Price).HasColumnType("decimal(18,0)");
        b.Entity<OrderDetail>().Property(x => x.UnitPrice).HasColumnType("decimal(18,0)");
        // Khóa chính phức hợp cho OrderDetail
        b.Entity<OrderDetail>().HasKey(x => new { x.OrderId, x.ProductId });
        // Các quan hệ và hành vi khi xóa
        b.Entity<Product>().HasOne(x => x.Category).WithMany(c => c.Products).HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<User>().HasOne(x => x.Role).WithMany(r => r.Users).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<Order>().HasOne(x => x.Customer).WithMany(c => c.Orders).HasForeignKey(x => x.CustomerId).OnDelete(DeleteBehavior.Restrict);
        b.Entity<OrderDetail>().HasOne(x => x.Order).WithMany(o => o.Details).HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<OrderDetail>().HasOne(x => x.Product).WithMany().HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Restrict);
    }
}