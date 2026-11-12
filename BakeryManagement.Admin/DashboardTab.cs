using BakeryManagement.Data;

namespace BakeryManagement.Admin;

class DashboardTab : UserControl
{
    private readonly Label _stats = new()
    {
        Dock = DockStyle.Top,
        Height = 110,
        Font = new Font("Segoe UI", 14),
        Padding = new Padding(15)
    };
    private readonly ListBox _top = new() { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11) };

    public DashboardTab()
    {
        var hdr = new Label { 
            Text = "Top sản phẩm bán chạy", 
            Dock = DockStyle.Top, Height = 30, 
            Font = new Font("Segoe UI", 11, FontStyle.Bold), 
            Padding = new Padding(15, 5, 0, 0) 
        };
        var bar = UI.Bar();
        bar.Controls.Add(UI.Btn("↻ Làm mới", (s, e) => LoadData()));
        Controls.Add(_top); 
        Controls.Add(hdr); 
        Controls.Add(_stats); 
        Controls.Add(bar);
        LoadData();
    }

    public void LoadData()
    {
        try
        {
            using var db = Db.Create();
            var s = new OrderService(db);

            var st = s.Stats();
            _stats.Text = $"Tổng số đơn: {st.Orders}      Doanh thu: {st.Revenue:N0} ₫\n" +
                          $"Khách hàng: {st.Customers}      Sản phẩm sắp hết (≤5): {st.LowStock}";

            _top.Items.Clear();
            foreach (var (name, qty) in s.TopProducts())
                _top.Items.Add($"{name} – đã bán {qty}");
        }
        catch (Exception ex) { UI.Error(ex); }
    }
}