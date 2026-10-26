using BakeryManagement.Data;

namespace BakeryManagement.Admin;

class PlaceholderTab : UserControl
{
    public PlaceholderTab(string name)
    {
        Controls.Add(new Label
        {
            Text = $"\"{name}\"",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Segoe UI", 12)
        });
    }
}

public class MainForm : Form
{
    public bool LoggedOut { get; private set; }

    public MainForm(User user)
    {
        var isAdmin = user.Role.RoleName == "Admin";

        Text = $"Quản trị tiệm bánh – {user.FullName} ({user.Role.RoleName})";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(900, 600);

        var tabs = new TabControl { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 10) };

        void AddTab(string title, Control content)
        {
            content.Dock = DockStyle.Fill;
            var page = new TabPage(title);
            page.Controls.Add(content);
            tabs.TabPages.Add(page);
        }

        // PHÂN QUYỀN: Admin thấy tất cả; Staff chỉ thấy Tổng quan, Đơn hàng, Khách hàng
        AddTab("📊 Tổng quan", new PlaceholderTab("Tổng quan"));
        if (isAdmin)
        {
            AddTab("🍰 Sản phẩm", new PlaceholderTab("Sản phẩm"));
            AddTab("🗂 Danh mục", new PlaceholderTab("Danh mục"));
        }
        AddTab("🧾 Đơn hàng", new PlaceholderTab("Đơn hàng"));
        AddTab("👤 Khách hàng", new PlaceholderTab("Khách hàng"));
        if (isAdmin) AddTab("🔑 Tài khoản", new PlaceholderTab("Tài khoản"));

        var menu = new MenuStrip();
        var logout = new ToolStripMenuItem("Đăng xuất");
        logout.Click += (s, e) => { LoggedOut = true; Close(); };
        menu.Items.Add(logout);

        Controls.Add(tabs);
        Controls.Add(menu);
        MainMenuStrip = menu;
    }
}