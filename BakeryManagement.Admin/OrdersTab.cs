using BakeryManagement.Data;

namespace BakeryManagement.Admin;

class OrdersTab : UserControl
{
    private readonly DataGridView _g = UI.Grid();
    private readonly TextBox _q = new() { Width = 180 };
    private readonly ComboBox _status = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 140 };

    public OrdersTab()
    {
        _status.Items.Add("(Tất cả)");
        _status.Items.AddRange(OrderStatus.All);
        _status.SelectedIndex = 0;

        var bar = UI.Bar();
        bar.Controls.Add(new Label { Text = "Tìm (mã/tên/SĐT):", AutoSize = true, Margin = new Padding(3, 9, 3, 0) });
        bar.Controls.Add(_q);
        bar.Controls.Add(_status);
        bar.Controls.Add(UI.Btn("Lọc", (s, e) => LoadData()));
        bar.Controls.Add(UI.Btn("🔍 Chi tiết", (s, e) => ShowDetail()));
        bar.Controls.Add(UI.Btn("🔄 Đổi trạng thái", (s, e) => ChangeStatus()));
        Controls.Add(_g);
        Controls.Add(bar);
        _g.CellDoubleClick += (s, e) => ShowDetail();
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            using var db = Db.Create();
            var st = _status.SelectedIndex <= 0 ? null : (string)_status.SelectedItem;

            _g.DataSource = new OrderService(db).All(_q.Text, st).Select(o => new
            {
                Id = o.OrderId,
                Ma = "#" + o.OrderId,
                Ngay = o.OrderDate.ToString("dd/MM/yyyy HH:mm"),
                KH = o.Customer.FullName,
                SDT = o.Phone,
                Tong = o.Total.ToString("N0"),
                TT = o.Status
            }).ToList();

            UI.Headers(_g, ("Ma", "Mã đơn"), ("Ngay", "Ngày đặt"), ("KH", "Khách hàng"),
                           ("SDT", "SĐT"), ("Tong", "Tổng tiền (₫)"), ("TT", "Trạng thái"));
        }
        catch (Exception ex) { UI.Error(ex); }
    }

    private void ShowDetail()
    {
        var id = UI.SelectedId(_g);
        if (id == null) return;

        using var db = Db.Create();
        var o = new OrderService(db).Get(id.Value);
        var lines = string.Join("\n", o.Details.Select(d =>
            $"  • {d.Product.Name} × {d.Quantity} = {d.Quantity * d.UnitPrice:N0} ₫"));

        UI.Info($"Đơn #{o.OrderId} – {o.Status}\nKhách: {o.Customer.FullName} ({o.Phone})\n" +
                $"Địa chỉ: {o.ShippingAddress}\nGhi chú: {o.Note}\n\n{lines}\n\nTổng: {o.Total:N0} ₫");
    }

    private void ChangeStatus()
    {
        var id = UI.SelectedId(_g);
        if (id == null) return;

        using var f = new FormDialog("Đổi trạng thái đơn #" + id);
        f.AddChoice("st", "Trạng thái mới", OrderStatus.All, (string)_g.CurrentRow.Cells["TT"].Value);
        f.Finish();
        if (f.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            using var db = Db.Create();
            new OrderService(db).UpdateStatus(id.Value, f.Choice("st"));
            LoadData();
        }
        catch (Exception ex) { UI.Error(ex); }
    }
}