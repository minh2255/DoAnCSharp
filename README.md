## 1. Công nghệ

| Thành phần | Công nghệ |
|---|---|
| Ngôn ngữ, nền tảng | C#, .NET 10 |
| Ứng dụng Web | ASP.NET Core MVC, Razor, Bootstrap 5, Session, Cookie Authentication |
| Ứng dụng Desktop | Windows Forms |
| Truy cập dữ liệu | Entity Framework Core (Code First) |
| Cơ sở dữ liệu | Microsoft SQL Server (LocalDB khi phát triển) |
| Bảo mật | Băm mật khẩu PBKDF2-SHA256 kèm salt, Anti-forgery token, Authorize |

## 2. Cấu trúc solution

```
BakeryManagement.sln
├─ BakeryManagement.Data     Thư viện dùng chung: Entity, DbContext, Service nghiệp vụ
├─ BakeryManagement.Web      Website khách hàng (ASP.NET Core MVC)
├─ BakeryManagement.Admin    Ứng dụng quản trị (Windows Forms)
└─ Database/BakeryDb.sql     Script đối chiếu lược đồ cơ sở dữ liệu
```

Web và Admin cùng tham chiếu **BakeryManagement.Data** và cùng trỏ vào một cơ sở dữ liệu `BakeryDb`, nên dữ liệu luôn đồng bộ giữa hai ứng dụng.

## 3. Yêu cầu cài đặt

- Windows 10 hoặc 11 (bắt buộc cho ứng dụng Windows Forms)
- Visual Studio 2022 bản 17.14 trở lên hoặc Visual Studio 2026, kèm workload **.NET desktop development** và **ASP.NET and web development**
- **.NET 10 SDK**
- SQL Server LocalDB (đi kèm Visual Studio) hoặc SQL Server Express/Developer
- Kết nối Internet khi chạy Web (tải Bootstrap từ CDN)

## 4. Cài đặt và chạy

1. Tải mã nguồn:
   ```bash
   git clone https://github.com/minh2255/DoAnCSharp.git
   cd BakeryManagement
   ```
2. Mở `BakeryManagement.sln` bằng Visual Studio.
3. Kiểm tra **chuỗi kết nối** ở hai tệp, hai nơi phải **giống hệt nhau**:
   - `BakeryManagement.Web/appsettings.json`
   - `BakeryManagement.Admin/appsettings.json`

   Mặc định (LocalDB):
   ```json
   "ConnectionStrings": {
     "Default": "Server=(localdb)\\MSSQLLocalDB;Database=BakeryDb;Trusted_Connection=True;TrustServerCertificate=True"
   }
   ```
   Nếu dùng SQL Server Express, đổi thành:
   ```json
   "Default": "Server=.\\SQLEXPRESS;Database=BakeryDb;Trusted_Connection=True;TrustServerCertificate=True"
   ```
4. Chạy lần đầu: cơ sở dữ liệu `BakeryDb`, 7 bảng và dữ liệu mẫu được **tự động tạo** (2 vai trò, 2 tài khoản, 5 danh mục, 13 sản phẩm). Chạy lại nhiều lần không bị nhân đôi dữ liệu.
5. Chạy cả hai ứng dụng cùng lúc: chuột phải solution, chọn **Configure Startup Projects**, chọn **Multiple startup projects**, đặt **Start** cho `BakeryManagement.Web` và `BakeryManagement.Admin`, rồi nhấn **F5**.

Chạy bằng dòng lệnh:

```bash
dotnet build
dotnet run --project BakeryManagement.Web
dotnet run --project BakeryManagement.Admin
```

## 5. Tài khoản mặc định

**Ứng dụng quản trị (Desktop)**

| Tài khoản | Mật khẩu | Quyền |
|---|---|---|
| `admin` | `Admin@123` | Toàn quyền (6 tab) |
| `nhanvien` | `Nhanvien@123` | Tổng quan, Đơn hàng, Khách hàng (3 tab) |

**Website:** khách hàng tự đăng ký tại trang Đăng ký (mật khẩu tối thiểu 6 ký tự).

> Đổi mật khẩu các tài khoản mặc định trước khi triển khai thật.

## 6. Chức năng chính

**Website khách hàng**
- Trang chủ, danh mục, tìm kiếm, lọc theo danh mục, phân trang, chi tiết sản phẩm
- Giỏ hàng (thêm, sửa số lượng, xóa), kiểm tra tồn kho
- Đăng ký, đăng nhập, đăng xuất
- Đặt hàng (thanh toán khi nhận hàng), xem lịch sử đơn, hủy đơn đang chờ xác nhận
- Giao diện responsive

**Ứng dụng quản trị**
- Đăng nhập và phân quyền Admin / Staff
- Quản lý sản phẩm (thêm, sửa, xóa hoặc ngừng bán, tìm kiếm)
- Quản lý danh mục, khách hàng (xem, tìm), tài khoản nhân viên (thêm, sửa, đặt lại mật khẩu, khóa, xóa)
- Quản lý đơn hàng: tìm, lọc theo trạng thái, xem chi tiết, đổi trạng thái (hủy đơn tự hoàn lại tồn kho)
- Tổng quan: số đơn, doanh thu, sản phẩm bán chạy, cảnh báo sắp hết hàng

## 7. Luồng demo gợi ý

1. **Web:** đăng ký tài khoản, tìm bánh, thêm vào giỏ, thanh toán và đặt hàng.
2. **Desktop:** đăng nhập `admin`, mở tab **Đơn hàng**, thấy đơn mới, xem chi tiết rồi đổi trạng thái sang *Đang giao*.
3. **Web:** vào **Đơn hàng của tôi**, thấy trạng thái mới (hai ứng dụng dùng chung dữ liệu).
4. Hủy đơn: tồn kho của sản phẩm được hoàn lại.
5. **Desktop:** sửa giá một sản phẩm, tải lại trang Web để thấy giá mới.
6. Đăng nhập `nhanvien`: chỉ thấy 3 tab (phân quyền).


---

© 2026 – Đồ án môn Ngôn ngữ lập trình C#. Mục đích học tập.
