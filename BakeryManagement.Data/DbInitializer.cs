using Microsoft.EntityFrameworkCore;

namespace BakeryManagement.Data;

public static class DbInitializer
{
    public static void Seed(BakeryDbContext db)
    {
        db.Database.EnsureCreated();

        // Nâng cấp CSDL tạo từ trước khi có chức năng thanh toán: thêm các cột còn thiếu vào bảng Orders.
        // EnsureCreated không tự thêm cột cho CSDL đã có, nên kiểm tra và thêm bằng lệnh SQL.
        db.Database.ExecuteSqlRaw(@"
IF COL_LENGTH('dbo.Orders', 'PaymentMethod') IS NULL
BEGIN
    ALTER TABLE dbo.Orders ADD
        PaymentMethod   NVARCHAR(20) NOT NULL CONSTRAINT DF_Orders_PaymentMethod DEFAULT N'COD',
        PaymentStatus   NVARCHAR(30) NOT NULL CONSTRAINT DF_Orders_PaymentStatus DEFAULT N'Chưa thanh toán',
        PaidAt          DATETIME2 NULL,
        TransactionCode NVARCHAR(50) NULL;
    EXEC(N'UPDATE dbo.Orders SET PaymentStatus = N''Đã thanh toán'', PaidAt = OrderDate WHERE Status = N''Hoàn thành''');
END");

        // Vai tro
        if (!db.Roles.Any())
        {
            db.Roles.AddRange(new Role { RoleName = "Admin" }, new Role { RoleName = "Staff" });
            db.SaveChanges();
        }
        // Tai khoan mac dinh
        if (!db.Users.Any())
        {
            var admin = db.Roles.First(r => r.RoleName == "Admin");
            var staff = db.Roles.First(r => r.RoleName == "Staff");
            db.Users.AddRange(
                new User
                {
                    Username = "admin",
                    FullName = "Quản trị viên",
                    PasswordHash = PasswordHasher.Hash("Admin@123"),
                    RoleId = admin.RoleId
                },
                new User
                {
                    Username = "nhanvien",
                    FullName = "Nhân viên bán hàng",
                    PasswordHash = PasswordHasher.Hash("Nhanvien@123"),
                    RoleId = staff.RoleId
                });
            db.SaveChanges();
        }
        // Danh muc va san pham
        // --- Danh muc ---
        if (!db.Categories.Any())
        {
            db.Categories.AddRange(
                new Category { Name = "Bánh kem", Description = "Bánh sinh nhật, bánh kỷ niệm" },
                new Category { Name = "Bánh mì", Description = "Bánh mì ngọt và mặn nướng mỗi ngày" },
                new Category { Name = "Bánh ngọt", Description = "Cupcake, tart, croissant, su kem" },
                new Category { Name = "Cookie & Bánh quy", Description = "Cookie, macaron, bánh quy bơ" },
                new Category { Name = "Đồ uống", Description = "Cà phê, trà, nước ép" });
            db.SaveChanges();
        }

        if (!db.Products.Any())
        {
            // Lay danh muc tu DB theo ten 
            Category C(string name) => db.Categories.First(c => c.Name == name);
            Product P(string name, string desc, decimal price, int stock, Category c, string img) =>
                new Product { Name = name, Description = desc, Price = price, Stock = stock, CategoryId = c.CategoryId, ImageUrl = "/images/products/" + img, IsActive = true };

            var kem = C("Bánh kem"); var mi = C("Bánh mì"); var ngot = C("Bánh ngọt");
            var cookie = C("Cookie & Bánh quy"); var drink = C("Đồ uống");

            db.Products.AddRange(
                P("Bánh kem dâu tây", "Bánh bông lan kem tươi phủ dâu tây Đà Lạt.", 350000, 10, kem, "banh-kem-dau-tay.jpg"),
                P("Bánh kem socola", "Cốt bánh socola đậm vị, phủ ganache.", 320000, 12, kem, "banh-kem-socola.jpg"),
                P("Bánh tiramisu", "Tiramisu kiểu Ý với mascarpone và cà phê.", 45000, 25, kem, "banh-tiramisu.jpg"),
                P("Bánh mì bơ tỏi", "Bánh mì nướng bơ tỏi thơm giòn.", 20000, 40, mi, "banh-mi-bo-toi.jpg"),
                P("Bánh mì sữa", "Mềm xốp, vị sữa nhẹ.", 15000, 50, mi, "banh-mi-sua.jpg"),
                P("Bánh mì ruốc phô mai", "Bánh mì ngọt mặn nhân ruốc và phô mai.", 25000, 30, mi, "banh-mi-ruoc-pho-mai.jpg"),
                P("Croissant bơ", "Croissant nướng với bơ Pháp, nhiều lớp.", 30000, 35, ngot, "croissant.jpg"),
                P("Su kem", "Vỏ su giòn, nhân kem vani.", 18000, 60, ngot, "su-kem.jpg"),
                P("Cupcake vani", "Cupcake vani phủ kem bơ.", 25000, 40, ngot, "cupcake-vani.jpg"),
                P("Cookie socola chip", "Cookie giòn ngoài mềm trong.", 15000, 80, cookie, "cookie-socola.jpg"),
                P("Macaron hỗn hợp (hộp 6)", "6 vị macaron ngẫu nhiên.", 180000, 15, cookie, "macaron.jpg"),
                P("Cà phê sữa", "Cà phê phin sữa đặc.", 29000, 100, drink, "ca-phe-sua.jpg"),
                P("Trà đào cam sả", "Trà đào thanh mát.", 35000, 100, drink, "tra-dao-cam-sa.jpg"));
            db.SaveChanges();
        }
    }
}


