using BakeryManagement.Data;

namespace BakeryManagement.Admin;

class CustomersTab : UserControl
{
    private readonly DataGridView _g = UI.Grid();
    private readonly TextBox _q = new() { Width = 220 };

    public CustomersTab()
    {
        var bar = UI.Bar();
        bar.Controls.Add(new Label { Text = "Tìm kiếm:", AutoSize = true, Margin = new Padding(3, 9, 3, 0) });
        bar.Controls.Add(_q);
        bar.Controls.Add(UI.Btn("Tìm", (s, e) => LoadData()));
        Controls.Add(_g);
        Controls.Add(bar);
        _q.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(); };
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            using var db = Db.Create();
            var k = _q.Text.Trim();
            _g.DataSource = db.Customers
                .Where(c => k == "" || c.FullName.Contains(k) || c.Email.Contains(k) || c.Phone.Contains(k))
                .OrderByDescending(c => c.CreatedAt).AsEnumerable()
                .Select(c => new
                {
                    Id = c.CustomerId,
                    Ten = c.FullName,
                    Email = c.Email,
                    SDT = c.Phone,
                    DiaChi = c.Address,
                    Ngay = c.CreatedAt.ToString("dd/MM/yyyy")
                }).ToList();

            UI.Headers(_g, ("Ten", "Họ tên"), ("SDT", "SĐT"), ("DiaChi", "Địa chỉ"), ("Ngay", "Ngày đăng ký"));
        }
        catch (Exception ex) { UI.Error(ex); }
    }
}