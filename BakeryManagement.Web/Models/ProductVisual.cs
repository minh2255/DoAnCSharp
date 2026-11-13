namespace BakeryManagement.Web.Models;

// Chọn biểu tượng và tông màu theo danh mục khi sản phẩm chưa có ảnh
public static class ProductVisual
{
    public static (string Emoji, string Tone) For(string category)
    {
        var c = (category ?? "").ToLowerInvariant();
        if (c.Contains("kem")) return ("🎂", "tone-cake");
        if (c.Contains("mì")) return ("🥖", "tone-bread");
        if (c.Contains("ngọt")) return ("🥐", "tone-pastry");
        if (c.Contains("cookie") || c.Contains("quy")) return ("🍪", "tone-cookie");
        if (c.Contains("uống")) return ("🧋", "tone-drink");
        return ("🍰", "tone-cake");
    }
}
