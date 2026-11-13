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
        bar.Controls.Add(UI.Btn("↻ Làm mới", (s, e) => LoadData()));
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

    // Số Admin đang hoạt động, không tính người có mã userId
    private static int OtherActiveAdmins(BakeryDbContext db, int userId) =>
        db.Users.Count(x => x.UserId != userId && x.IsActive && x.Role.RoleName == "Admin");

    private void Edit(int? id)
    {
        using var db = Db.Create();
        var u = id.HasValue ? db.Users.Find(id.Value) : new User();
        var roles = db.Roles.ToList().Select(r => new KeyValuePair<int, string>(r.RoleId, r.RoleName)).ToList();

        // Trạng thái ban đầu của người này (ghi lại trước khi sửa)
        var wasActiveAdmin = id.HasValue && u.IsActive
            && db.Roles.Any(r => r.RoleId == u.RoleId && r.RoleName == "Admin");

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
            var newRoleId = f.Combo("role");
            var newActive = f.Check("active");
            var stillActiveAdmin = newActive && roles.First(r => r.Key == newRoleId).Value == "Admin";

            if (db.Users.Any(x => x.Username == name && x.UserId != u.UserId))
                throw new InvalidOperationException("Tài khoản đã tồn tại.");
            if (u.UserId == _currentUserId && !newActive)
                throw new InvalidOperationException("Không thể tự khóa tài khoản đang đăng nhập.");
            if (u.UserId == _currentUserId && wasActiveAdmin && !stillActiveAdmin)
                throw new InvalidOperationException("Không thể tự hạ quyền của chính mình.");
            if (wasActiveAdmin && !stillActiveAdmin && OtherActiveAdmins(db, u.UserId) == 0)
                throw new InvalidOperationException("Phải còn ít nhất một Admin đang hoạt động.");

            u.Username = name;
            u.FullName = f.Text_("name");
            u.RoleId = newRoleId;
            u.IsActive = newActive;
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
            var u = db.Users.Include(x => x.Role).First(x => x.UserId == id.Value);
            if (u.IsActive && u.Role.RoleName == "Admin" && OtherActiveAdmins(db, u.UserId) == 0)
                throw new InvalidOperationException("Phải còn ít nhất một Admin đang hoạt động, không thể xóa Admin cuối cùng.");

            db.Users.Remove(u);
            db.SaveChanges();
            LoadData();
        }
        catch (Exception ex) { UI.Error(ex); }
    }
}