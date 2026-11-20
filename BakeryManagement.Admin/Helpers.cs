using System.Text.Json;
using BakeryManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryManagement.Admin;

// Tạo DbContext từ chuỗi kết nối trong appsettings.json
static class Db
{
    private static string _conn;

    public static BakeryDbContext Create()
    {
        _conn ??= JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.json")))
            .RootElement.GetProperty("ConnectionStrings").GetProperty("Default").GetString();

        var options = new DbContextOptionsBuilder<BakeryDbContext>().UseSqlServer(_conn).Options;
        return new BakeryDbContext(options);
    }
}

// Các hàm giao diện dùng chung, để các màn hình sau viết ngắn gọn
static class UI
{
    public static DataGridView Grid() => new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        RowHeadersVisible = false,
        BackgroundColor = Color.White,
        BorderStyle = BorderStyle.None
    };

    public static FlowLayoutPanel Bar() =>
        new() { Dock = DockStyle.Top, Height = 44, Padding = new Padding(6, 6, 0, 0) };

    public static Button Btn(string text, EventHandler onClick)
    {
        var b = new Button { Text = text, AutoSize = true, Height = 30, Margin = new Padding(3) };
        b.Click += onClick;
        return b;
    }

    // Đổi tên cột hiển thị sang tiếng Việt, ẩn cột Id
    public static void Headers(DataGridView g, params (string Col, string Text)[] map)
    {
        foreach (var (c, t) in map) if (g.Columns.Contains(c)) g.Columns[c].HeaderText = t;
        if (g.Columns.Contains("Id")) g.Columns["Id"].Visible = false;
    }

    public static int? SelectedId(DataGridView g) =>
        g.CurrentRow == null ? null : Convert.ToInt32(g.CurrentRow.Cells["Id"].Value);

    public static void Error(Exception ex) => MessageBox.Show(ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    public static void Info(string m) => MessageBox.Show(m, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
    public static bool Confirm(string m) =>
        MessageBox.Show(m, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
}

// Lưu ảnh sản phẩm vào thư mục wwwroot/images/products của Web để Web hiển thị được.
static class ImageStore
{
    // Ưu tiên khóa "ImagesFolder" trong appsettings.json; nếu không có thì dò ngược từ thư mục chạy để tìm BakeryManagement.Web/wwwroot.
    public static string FindFolder()
    {
        try
        {
            var cfg = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.json"))).RootElement;
            if (cfg.TryGetProperty("ImagesFolder", out var p) && !string.IsNullOrWhiteSpace(p.GetString()))
                return p.GetString();
        }
        catch { }

        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var wwwroot = Path.Combine(dir.FullName, "BakeryManagement.Web", "wwwroot");
            if (Directory.Exists(wwwroot)) return Path.Combine(wwwroot, "images", "products");
            dir = dir.Parent;
        }
        return null;
    }

    // Chép ảnh vào thư mục ảnh, trả về đường dẫn lưu vào ImageUrl, ví dụ /images/products/banh-moi.jpg
    public static string Import(string sourceFile)
    {
        var folder = FindFolder()
            ?? throw new InvalidOperationException(
                "Không tìm thấy thư mục ảnh của Web. Hãy thêm dòng \"ImagesFolder\" vào appsettings.json của Admin, trỏ tới BakeryManagement.Web\\wwwroot\\images\\products.");
        Directory.CreateDirectory(folder);

        var ext = Path.GetExtension(sourceFile).ToLowerInvariant();
        var name = Path.GetFileNameWithoutExtension(sourceFile);
        var safe = new string(name.Select(c => char.IsLetterOrDigit(c) && c < 128 ? char.ToLowerInvariant(c) : '-').ToArray()).Trim('-');
        if (safe == "") safe = "anh";

        var file = safe + ext;
        var i = 1;
        var srcBytes = File.ReadAllBytes(sourceFile);
        // Trùng tên mà nội dung khác thì đổi tên; trùng cả nội dung thì dùng lại tệp cũ
        while (File.Exists(Path.Combine(folder, file)) && !File.ReadAllBytes(Path.Combine(folder, file)).AsSpan().SequenceEqual(srcBytes))
            file = $"{safe}-{i++}{ext}";
        var dest = Path.Combine(folder, file);
        if (!File.Exists(dest)) File.WriteAllBytes(dest, srcBytes);
        return "/images/products/" + file;
    }
}