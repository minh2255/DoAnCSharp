using System.Security.Cryptography;

namespace BakeryManagement.Data;

public static class PasswordHasher
{
    private const int Iterations = 100_000;   // số vòng lặp: càng nhiều càng chậm với kẻ tấn công

    // Băm mật khẩu -> "salt.hash" (cả hai ở dạng Base64)
    public static string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
        return Convert.ToBase64String(salt) + "." + Convert.ToBase64String(hash);
    }

    // Kiểm tra mật khẩu người dùng nhập có khớp chuỗi đã lưu không
    public static bool Verify(string password, string stored)
    {
        var parts = (stored ?? "").Split('.');
        if (parts.Length != 2) return false;

        var salt = Convert.FromBase64String(parts[0]);
        var expected = Convert.FromBase64String(parts[1]);

        // Băm lại mật khẩu nhập vào với CÙNG salt đã lưu
        var actual = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, HashAlgorithmName.SHA256, 32);
        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}
