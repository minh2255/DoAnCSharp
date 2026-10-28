using BakeryManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace BakeryManagement.Admin;

class UsersTab : UserControl
{
    private readonly DataGridView _g = UI.Grid();
    private readonly int _currentUserId;

    public UsersTab(int currentUserId)
    {
        _currentUserId = currentUserId;
        var bar = UI.Bar();
        bar.Controls.Add(UI.Btn("➕ Thêm", (s, e) => Edit(null)));
        bar.Controls.Add(UI.Btn("✏ Sửa / Đặt lại MK", (s, e) => { var id = UI.SelectedId(_g); if (id != null) Edit(id); }));
        bar.Controls.Add(UI.Btn("🗑 Xóa", (s, e) => Delete()));
        Controls.Add(_g);
        Controls.Add(bar);
        LoadData();
    }

    private void LoadData()
    {
        try
        {
            using var db = Db.Create();
            _g.DataSource = db.Users.Include(u => u.Role).OrderBy(u => u.Username).AsEnumerable()
                .Select(u => new
                {
                    Id = u.UserId,
                    TK = u.Username,
                    Ten = u.FullName,
                    Quyen = u.Role.RoleName,
                    TT = u.IsActive ? "Hoạt động" : "Khóa"
                }).ToList();
            UI.Headers(_g, ("TK", "Tài khoản"), ("Ten", "Họ tên"), ("Quyen", "Quyền"), ("TT", "Trạng thái"));
        }
        catch (Exception ex) { UI.Error(ex); }
    }

    private void Edit(int? id)
    {
        using var db = Db.Create();
        var u = id.HasValue ? db.Users.Find(id.Value) : new User();
        var roles = db.Roles.ToList().Select(r => new KeyValuePair<int, string>(r.RoleId, r.RoleName)).ToList();

        using var f = new FormDialog(id.HasValue ? "Sửa tài khoản" : "Thêm tài khoản");
        f.AddText("user", "Tài khoản", u.Username);
        f.AddText("name", "Họ tên", u.FullName);
        f.AddText("pass", id.HasValue ? "Mật khẩu mới (để trống = giữ)" : "Mật khẩu", "", password: true);
        f.AddCombo("role", "Quyền", roles, id.HasValue ? u.RoleId : null);
        f.AddCheck("active", "Hoạt động", id.HasValue ? u.IsActive : true);

        f.Validator = d =>
            string.IsNullOrWhiteSpace(d.Text_("user")) ? "Nhập tài khoản."
            : string.IsNullOrWhiteSpace(d.Text_("name")) ? "Nhập họ tên."
            : (!id.HasValue || d.Text_("pass") != "") && d.Text_("pass").Length < 6 ? "Mật khẩu tối thiểu 6 ký tự."
            : null;

        f.Finish();
        if (f.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            var name = f.Text_("user");
            if (db.Users.Any(x => x.Username == name && x.UserId != u.UserId))
                throw new InvalidOperationException("Tài khoản đã tồn tại.");
            if (u.UserId == _currentUserId && !f.Check("active"))
                throw new InvalidOperationException("Không thể tự khóa tài khoản đang đăng nhập.");

            u.Username = name;
            u.FullName = f.Text_("name");
            u.RoleId = f.Combo("role");
            u.IsActive = f.Check("active");
            if (f.Text_("pass") != "") u.PasswordHash = PasswordHasher.Hash(f.Text_("pass"));

            if (!id.HasValue) db.Users.Add(u);
            db.SaveChanges();
            LoadData();
        }
        catch (Exception ex) { UI.Error(ex); }
    }

    private void Delete()
    {
        var id = UI.SelectedId(_g);
        if (id == null) return;
        if (id == _currentUserId) { UI.Info("Không thể xóa tài khoản đang đăng nhập."); return; }
        if (!UI.Confirm("Xóa tài khoản này?")) return;

        try
        {
            using var db = Db.Create();
            db.Users.Remove(db.Users.Find(id.Value));
            db.SaveChanges();
            LoadData();
        }
        catch (Exception ex) { UI.Error(ex); }
    }
}