/* =====================================================================
   Ngọt Ngào Bakery - Script lược đồ cơ sở dữ liệu BakeryDb (SQL Server)
   - Tạo database (nếu chưa có), 7 bảng, khóa, chỉ mục và dữ liệu mẫu.
   - Chạy lại nhiều lần được: các bảng cũ sẽ bị XÓA rồi tạo lại.
   - Tài khoản Desktop mặc định: admin / Admin@123, nhanvien / Nhanvien@123
   - Ứng dụng cũng tự tạo CSDL bằng EF Core (EnsureCreated); script này
     dùng để đối chiếu hoặc dựng nhanh CSDL.
   ===================================================================== */
IF DB_ID(N'BakeryDb') IS NULL
    CREATE DATABASE [BakeryDb];
GO
USE [BakeryDb];
GO

/* ---------- Xóa bảng cũ (theo thứ tự khóa ngoại) ---------- */
DROP TABLE IF EXISTS dbo.OrderDetails;
DROP TABLE IF EXISTS dbo.Orders;
DROP TABLE IF EXISTS dbo.Products;
DROP TABLE IF EXISTS dbo.Users;
DROP TABLE IF EXISTS dbo.Customers;
DROP TABLE IF EXISTS dbo.Categories;
DROP TABLE IF EXISTS dbo.Roles;
GO

/* ---------- Tạo bảng ---------- */
CREATE TABLE dbo.Roles (
    RoleId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL
);
CREATE UNIQUE INDEX IX_Roles_RoleName ON dbo.Roles(RoleName);

CREATE TABLE dbo.Users (
    UserId       INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    Username     NVARCHAR(50)  NOT NULL,
    PasswordHash NVARCHAR(200) NOT NULL,
    FullName     NVARCHAR(100) NOT NULL,
    IsActive     BIT NOT NULL,
    RoleId       INT NOT NULL,
    CONSTRAINT FK_Users_Roles_RoleId FOREIGN KEY (RoleId) REFERENCES dbo.Roles(RoleId)
);
CREATE UNIQUE INDEX IX_Users_Username ON dbo.Users(Username);
CREATE INDEX IX_Users_RoleId ON dbo.Users(RoleId);

CREATE TABLE dbo.Categories (
    CategoryId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Categories PRIMARY KEY,
    Name        NVARCHAR(100) NOT NULL,
    Description NVARCHAR(300) NOT NULL
);
CREATE UNIQUE INDEX IX_Categories_Name ON dbo.Categories(Name);

CREATE TABLE dbo.Products (
    ProductId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Products PRIMARY KEY,
    Name        NVARCHAR(150)  NOT NULL,
    Description NVARCHAR(1000) NOT NULL,
    Price       DECIMAL(18,0)  NOT NULL,
    ImageUrl    NVARCHAR(300)  NOT NULL,
    Stock       INT NOT NULL,
    IsActive    BIT NOT NULL,
    CategoryId  INT NOT NULL,
    CONSTRAINT FK_Products_Categories_CategoryId FOREIGN KEY (CategoryId) REFERENCES dbo.Categories(CategoryId)
);
CREATE INDEX IX_Products_CategoryId ON dbo.Products(CategoryId);

CREATE TABLE dbo.Customers (
    CustomerId   INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Customers PRIMARY KEY,
    FullName     NVARCHAR(100) NOT NULL,
    Email        NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(200) NOT NULL,
    Phone        NVARCHAR(20)  NOT NULL,
    Address      NVARCHAR(250) NOT NULL,
    CreatedAt    DATETIME2(7)  NOT NULL,
    IsActive     BIT NOT NULL CONSTRAINT DF_Customers_IsActive DEFAULT (1)
);
CREATE UNIQUE INDEX IX_Customers_Email ON dbo.Customers(Email);

CREATE TABLE dbo.Orders (
    OrderId         INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Orders PRIMARY KEY,
    CustomerId      INT NOT NULL,
    OrderDate       DATETIME2(7) NOT NULL,
    Status          NVARCHAR(30)  NOT NULL,
    ShippingAddress NVARCHAR(250) NOT NULL,
    Phone           NVARCHAR(20)  NOT NULL,
    Note            NVARCHAR(300) NOT NULL,
    CONSTRAINT FK_Orders_Customers_CustomerId FOREIGN KEY (CustomerId) REFERENCES dbo.Customers(CustomerId)
);
CREATE INDEX IX_Orders_CustomerId ON dbo.Orders(CustomerId);

CREATE TABLE dbo.OrderDetails (
    OrderId   INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity  INT NOT NULL,
    UnitPrice DECIMAL(18,0) NOT NULL,
    CONSTRAINT PK_OrderDetails PRIMARY KEY (OrderId, ProductId),
    CONSTRAINT FK_OrderDetails_Orders_OrderId   FOREIGN KEY (OrderId)   REFERENCES dbo.Orders(OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetails_Products_ProductId FOREIGN KEY (ProductId) REFERENCES dbo.Products(ProductId)
);
CREATE INDEX IX_OrderDetails_ProductId ON dbo.OrderDetails(ProductId);
GO

/* ---------- Dữ liệu mẫu ---------- */
SET IDENTITY_INSERT dbo.Roles ON;
INSERT dbo.Roles (RoleId, RoleName) VALUES (1, N'Admin'), (2, N'Staff');
SET IDENTITY_INSERT dbo.Roles OFF;

SET IDENTITY_INSERT dbo.Users ON;
INSERT dbo.Users (UserId, Username, PasswordHash, FullName, IsActive, RoleId) VALUES
 (1, N'admin',    N'azUBjYwkz4wxjea79jDPGA==.rfoY5U6d7L56k14PzJqE8zOBIVHapgJ64Bkqh2n0Obs=', N'Quản trị viên', 1, 1),
 (2, N'nhanvien', N'V5B8WLFquiKmlmQzWeeMxQ==.Qhk2OIJkkN8IexuUYd2zR6ls9gb+P5EqRQ3+IWhtc2Y=', N'Nhân viên bán hàng', 1, 2);
SET IDENTITY_INSERT dbo.Users OFF;

SET IDENTITY_INSERT dbo.Categories ON;
INSERT dbo.Categories (CategoryId, Name, Description) VALUES
 (1, N'Bánh kem', N'Bánh sinh nhật, bánh kỷ niệm'),
 (2, N'Bánh mì', N'Bánh mì ngọt và mặn nướng mỗi ngày'),
 (3, N'Bánh ngọt', N'Cupcake, tart, croissant, su kem'),
 (4, N'Cookie & Bánh quy', N'Cookie, macaron, bánh quy bơ'),
 (5, N'Đồ uống', N'Cà phê, trà, nước ép');
SET IDENTITY_INSERT dbo.Categories OFF;

SET IDENTITY_INSERT dbo.Products ON;
INSERT dbo.Products (ProductId, Name, Description, Price, ImageUrl, Stock, IsActive, CategoryId) VALUES
 (1,  N'Bánh kem dâu tây', N'Bánh bông lan kem tươi phủ dâu tây Đà Lạt.', 350000, N'', 10, 1, 1),
 (2,  N'Bánh kem socola', N'Cốt bánh socola đậm vị, phủ ganache.', 320000, N'', 12, 1, 1),
 (3,  N'Bánh tiramisu', N'Tiramisu kiểu Ý với mascarpone và cà phê.', 45000, N'', 25, 1, 1),
 (4,  N'Bánh mì bơ tỏi', N'Bánh mì nướng bơ tỏi thơm giòn.', 20000, N'', 40, 1, 2),
 (5,  N'Bánh mì sữa', N'Mềm xốp, vị sữa nhẹ.', 15000, N'', 50, 1, 2),
 (6,  N'Bánh mì ruốc phô mai', N'Bánh mì ngọt mặn nhân ruốc và phô mai.', 25000, N'', 30, 1, 2),
 (7,  N'Croissant bơ', N'Croissant nướng với bơ Pháp, nhiều lớp.', 30000, N'', 35, 1, 3),
 (8,  N'Su kem', N'Vỏ su giòn, nhân kem vani.', 18000, N'', 60, 1, 3),
 (9,  N'Cupcake vani', N'Cupcake vani phủ kem bơ.', 25000, N'', 40, 1, 3),
 (10, N'Cookie socola chip', N'Cookie giòn ngoài mềm trong.', 15000, N'', 80, 1, 4),
 (11, N'Macaron hỗn hợp (hộp 6)', N'6 vị macaron ngẫu nhiên.', 180000, N'', 15, 1, 4),
 (12, N'Cà phê sữa', N'Cà phê phin sữa đặc.', 29000, N'', 100, 1, 5),
 (13, N'Trà đào cam sả', N'Trà đào thanh mát.', 35000, N'', 100, 1, 5);
SET IDENTITY_INSERT dbo.Products OFF;
GO
/* Bảng Customers, Orders, OrderDetails để trống: khách hàng tự đăng ký trên Web. */
