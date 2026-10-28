using BakeryManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryManagement.Admin;

class CategoriesTab : UserControl
{
    private readonly DataGridView _g = UI.Grid();

    public CategoriesTab()
    {
        var bar = UI.Bar();
        bar.Controls.Add(UI.Btn("➕ Thêm", (s, e) => Edit(null)));
        bar.Controls.Add(UI.Btn("✏ Sửa", (s, e) => { var id = UI.SelectedId(_g); if (id != null) Edit(id); }));
        bar.Controls.Add(UI.Btn("🗑 Xóa", (s, e) => Delete()));
        Controls.Add(_g);
        Controls.Add(bar);
        _g.CellDoubleClick += (s, e) => { var id = UI.SelectedId(_g); if (id != null) Edit(id); };
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            using var db = Db.Create();
            _g.DataSource = db.Categories.Include(c => c.Products).OrderBy(c => c.Name).AsEnumerable()
                .Select(c => new { Id = c.CategoryId, Ten = c.Name, MoTa = c.Description, SL = c.Products.Count }).ToList();
            UI.Headers(_g, ("Ten", "Tên danh mục"), ("MoTa", "Mô tả"), ("SL", "Số sản phẩm"));
        }
        catch (Exception ex) { UI.Error(ex); }
    }

    private void Edit(int? id)
    {
        using var db = Db.Create();
        var c = id.HasValue ? db.Categories.Find(id.Value) : new Category();

        using var f = new FormDialog(id.HasValue ? "Sửa danh mục" : "Thêm danh mục");
        f.AddText("name", "Tên danh mục", c.Name);
        f.AddText("desc", "Mô tả", c.Description, multiline: true);
        f.Validator = d => string.IsNullOrWhiteSpace(d.Text_("name")) ? "Tên danh mục không được để trống." : null;
        f.Finish();
        if (f.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            c.Name = f.Text_("name");
            c.Description = f.Text_("desc");
            new ProductService(db).SaveCategory(c);
            LoadData();
        }
        catch (Exception ex) { UI.Error(ex); }
    }

    private void Delete()
    {
        var id = UI.SelectedId(_g);
        if (id == null || !UI.Confirm("Xóa danh mục này?")) return;
        try
        {
            using var db = Db.Create();
            new ProductService(db).DeleteCategory(id.Value);
            LoadData();
        }
        catch (Exception ex) { UI.Error(ex); }
    }
}