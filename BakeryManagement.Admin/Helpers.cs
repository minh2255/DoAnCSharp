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