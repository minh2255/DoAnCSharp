using BakeryManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryManagement.Admin;

class CustomersTab : UserControl
{
    private readonly DataGridView _g = UI.Grid();
    private readonly TextBox _q = new() { Width = 220 };
    public CustomersTab()
    {
        var bar = UI.Bar();
        bar.Controls.Add(new Label { Text = "Tìm kiếm:", AutoSize = true, Margin = new Padding(3, 9, 3, 0) });
        bar.Controls.Add(_q); bar.Controls.Add(UI.Btn("Tìm", (s, e) => LoadData()));
        bar.Controls.Add(UI.Btn("🔒 Khóa / 🔓 Mở khóa", (s, e) => Toggle()));
        bar.Controls.Add(UI.Btn("↻ Làm mới", (s, e) => LoadData()));      
        Controls.Add(_g); Controls.Add(bar);
        Controls.Add(_g); Controls.Add(bar);
        _q.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(); };
        LoadData();
    }
    private void LoadData()
    {
        try
        {
            using var db = Db.Create();
            var k = _q.Text.Trim();
            _g.DataSource = db.Customers.Where(c => k == "" || c.FullName.Contains(k) || c.Email.Contains(k) || c.Phone.Contains(k))
                .OrderByDescending(c => c.CreatedAt).AsEnumerable()
                .Select(c => new
                {
                    Id = c.CustomerId,
                    Ten = c.FullName,
                    Email = c.Email,
                    SDT = c.Phone,
                    DiaChi = c.Address,
                    Ngay = c.CreatedAt.ToString("dd/MM/yyyy"),
                    TT = c.IsActive ? "Hoạt động" : "Khóa"
                }).ToList();
            UI.Headers(_g, ("Ten", "Họ tên"), ("SDT", "SĐT"), ("DiaChi", "Địa chỉ"), ("Ngay", "Ngày đăng ký"), ("TT", "Trạng thái"));
            foreach (DataGridViewRow r in _g.Rows)
                if ((r.Cells["TT"].Value as string) == "Khóa") r.DefaultCellStyle.ForeColor = Color.Firebrick;
        }
        catch (Exception ex) { UI.Error(ex); }
    }
    private void Toggle()
    {
        var id = UI.SelectedId(_g);
        if (id == null) return;
        try
        {
            using var db = Db.Create();
            var c = db.Customers.Find(id.Value);
            if (c == null) return;
            var open = !c.IsActive;      // đang khóa thì mở, đang mở thì khóa

            var msg = $"{(open ? "Mở khóa" : "Khóa")} tài khoản của '{c.FullName}'?";
            if (!open)
            {
                var pending = db.Orders.Count(o => o.CustomerId == c.CustomerId
                    && o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled);
                if (pending > 0)
                    msg += $"\n\nKhách này đang có {pending} đơn chưa hoàn thành. Đơn vẫn được xử lý bình thường, " +
                           "nhưng khách sẽ không đăng nhập để theo dõi hay hủy đơn được.";
            }
            if (!UI.Confirm(msg)) return;

            c.IsActive = open;
            db.SaveChanges();
            LoadData();
        }
        catch (Exception ex) { UI.Error(ex); }
    }
}