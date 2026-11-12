using Microsoft.EntityFrameworkCore;

namespace BakeryManagement.Data;

public class AuthService
{
    private readonly BakeryDbContext _db;
    public AuthService(BakeryDbContext db) => _db = db;   // DI cấp DbContext vào đây

    // Đăng nhập nhân viên/quản trị (Desktop). Sai thì trả về null.
    public User LoginStaff(string username, string password)
    {
        var u = _db.Users.Include(x => x.Role)
            .FirstOrDefault(x => x.Username == username && x.IsActive);
        return u != null && PasswordHasher.Verify(password, u.PasswordHash) ? u : null;
    }

    // Đăng nhập khách hàng (Web)
    public Customer LoginCustomer(string email, string password)
    {
        var c = _db.Customers.FirstOrDefault(x => x.Email == email);
        if (c == null || !PasswordHasher.Verify(password, c.PasswordHash)) return null;
        if (!c.IsActive) throw new InvalidOperationException("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ cửa hàng.");
        return c;
    }

    public void SetCustomerActive(int id, bool active)
    {
        var c = _db.Customers.Find(id) ?? throw new InvalidOperationException("Không tìm thấy khách hàng.");
        c.IsActive = active;
        _db.SaveChanges();
    }

    public Customer RegisterCustomer(string fullName, string email, string password, string phone, string address)
    {
        email = email.Trim().ToLower();
        if (_db.Customers.Any(x => x.Email == email))
            throw new InvalidOperationException("Email đã được đăng ký.");

        var c = new Customer
        {
            FullName = fullName.Trim(),
            Email = email,
            Phone = phone ?? "",
            Address = address ?? "",
            PasswordHash = PasswordHasher.Hash(password)
        };
        _db.Customers.Add(c);
        _db.SaveChanges();
        return c;
    }

    public Customer GetCustomer(int id) => _db.Customers.Find(id);

    public void UpdateProfile(int id, string fullName, string phone, string address)
    {
        var c = _db.Customers.Find(id) ?? throw new InvalidOperationException("Không tìm thấy tài khoản.");
        if (string.IsNullOrWhiteSpace(fullName)) throw new ArgumentException("Họ tên không được để trống.");
        c.FullName = fullName.Trim();
        c.Phone = string.IsNullOrWhiteSpace(phone) ? "" : phone.Trim();
        c.Address = string.IsNullOrWhiteSpace(address) ? "" : address.Trim();
        _db.SaveChanges();
    }

    public void ChangePassword(int id, string currentPassword, string newPassword)
    {
        var c = _db.Customers.Find(id) ?? throw new InvalidOperationException("Không tìm thấy tài khoản.");
        if (!PasswordHasher.Verify(currentPassword, c.PasswordHash))
            throw new InvalidOperationException("Mật khẩu hiện tại không đúng.");
        SetPassword(c, newPassword);
    }

    public Customer FindCustomerByEmail(string email)
        => _db.Customers.FirstOrDefault(x => x.Email == email.Trim().ToLower());

    /// Đặt mật khẩu mới (dùng cho quên mật khẩu, đã xác thực bằng token).
    public void ResetPassword(int id, string newPassword)
    {
        var c = _db.Customers.Find(id) ?? throw new InvalidOperationException("Không tìm thấy tài khoản.");
        SetPassword(c, newPassword);
    }

    private void SetPassword(Customer c, string newPassword)
    {
        if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            throw new ArgumentException("Mật khẩu tối thiểu 6 ký tự.");
        c.PasswordHash = PasswordHasher.Hash(newPassword);
        _db.SaveChanges();
    }
}