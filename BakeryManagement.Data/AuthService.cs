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
        return c != null && PasswordHasher.Verify(password, c.PasswordHash) ? c : null;
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
            Phone = phone,
            Address = address,
            PasswordHash = PasswordHasher.Hash(password)
        };
        _db.Customers.Add(c);
        _db.SaveChanges();
        return c;
    }

    public Customer GetCustomer(int id) => _db.Customers.Find(id);
}