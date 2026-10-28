using BakeryManagement.Data;

namespace BakeryManagement.Admin;

class ProductsTab : UserControl
{
    private readonly DataGridView _g = UI.Grid();
    private readonly TextBox _q = new() { Width = 220 };

    public ProductsTab()
    {
        var bar = UI.Bar();
        bar.Controls.Add(new Label { Text = "Tìm kiếm:", AutoSize = true, Margin = new Padding(3, 9, 3, 0) });
        bar.Controls.Add(_q);
        bar.Controls.Add(UI.Btn("Tìm", (s, e) => LoadData()));
        bar.Controls.Add(UI.Btn("➕ Thêm", (s, e) => Edit(null)));
        bar.Controls.Add(UI.Btn("✏ Sửa", (s, e) => EditSelected()));
        bar.Controls.Add(UI.Btn("🗑 Xóa", (s, e) => DeleteSelected()));

        Controls.Add(_g);
        Controls.Add(bar);

        _q.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) LoadData(); };
        _g.CellDoubleClick += (s, e) => EditSelected();
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            using var db = Db.Create();
            var (items, _) = new ProductService(db).Search(_q.Text, null, 1, 5000, false);

            _g.DataSource = items.Select(p => new
            {
                Id = p.ProductId,
                Ten = p.Name,
                DanhMuc = p.Category.Name,
                Gia = p.Price.ToString("N0"),
                Kho = p.Stock,
                Ban = p.IsActive ? "Đang bán" : "Ngừng bán"
            }).ToList();

            UI.Headers(_g, ("Ten", "Tên sản phẩm"), ("DanhMuc", "Danh mục"),
                           ("Gia", "Giá (₫)"), ("Kho", "Tồn kho"), ("Ban", "Trạng thái"));
        }
        catch (Exception ex) { UI.Error(ex); }
    }

    private void EditSelected()
    {
        var id = UI.SelectedId(_g);
        if (id == null) { UI.Info("Hãy chọn một sản phẩm."); return; }
        Edit(id);
    }

    private void Edit(int? id)
    {
        using var db = Db.Create();
        var svc = new ProductService(db);
        var p = id.HasValue ? svc.Get(id.Value) : new Product();

        var cats = svc.Categories().Select(c => new KeyValuePair<int, string>(c.CategoryId, c.Name)).ToList();
        if (cats.Count == 0) { UI.Info("Cần tạo danh mục trước."); return; }

        using var f = new FormDialog(id.HasValue ? "Sửa sản phẩm" : "Thêm sản phẩm");
        f.AddText("name", "Tên sản phẩm", p.Name);
        f.AddCombo("cat", "Danh mục", cats, id.HasValue ? p.CategoryId : null);
        f.AddText("price", "Giá (₫)", p.Price.ToString("0"));
        f.AddText("stock", "Tồn kho", p.Stock.ToString());
        f.AddText("img", "Link ảnh", p.ImageUrl);
        f.AddText("desc", "Mô tả", p.Description, multiline: true);
        f.AddCheck("active", "Đang bán", id.HasValue ? p.IsActive : true);

        f.Validator = d =>
            string.IsNullOrWhiteSpace(d.Text_("name")) ? "Tên sản phẩm không được để trống."
            : d.Dec("price") < 0 ? "Giá phải là số không âm."
            : d.Int("stock") < 0 ? "Tồn kho phải là số nguyên không âm."
            : null;

        f.Finish();
        if (f.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            p.Name = f.Text_("name");
            p.CategoryId = f.Combo("cat");
            p.Price = f.Dec("price");
            p.Stock = f.Int("stock");
            p.ImageUrl = f.Text_("img");
            p.Description = f.Text_("desc");
            p.IsActive = f.Check("active");
            svc.Save(p);
            LoadData();
        }
        catch (Exception ex) { UI.Error(ex); }
    }

    private void DeleteSelected()
    {
        var id = UI.SelectedId(_g);
        if (id == null) { UI.Info("Hãy chọn một sản phẩm."); return; }
        if (!UI.Confirm("Xóa sản phẩm này?")) return;

        try
        {
            using var db = Db.Create();
            var deleted = new ProductService(db).Delete(id.Value);
            if (!deleted)
                UI.Info("Sản phẩm đã có trong đơn hàng nên được chuyển sang 'Ngừng bán'.");
            LoadData();
        }
        catch (Exception ex) { UI.Error(ex); }
    }
}