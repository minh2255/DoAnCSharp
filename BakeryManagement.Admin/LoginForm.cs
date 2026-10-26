using BakeryManagement.Data;

namespace BakeryManagement.Admin;

public class LoginForm : Form
{
    private readonly TextBox _user = new() { Left = 130, Top = 70, Width = 220 };
    private readonly TextBox _pass = new() { Left = 130, Top = 108, Width = 220, UseSystemPasswordChar = true };

    public User CurrentUser { get; private set; }

    public LoginForm()
    {
        Text = "Đăng nhập – Quản trị tiệm bánh";
        ClientSize = new Size(400, 220);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;

        var title = new Label
        {
            Text = "🥐 NGỌT NGÀO BAKERY",
            Font = new Font("Segoe UI", 15, FontStyle.Bold),
            ForeColor = Color.SaddleBrown,
            Left = 20,
            Top = 18,
            AutoSize = true
        };
        var ok = new Button { Text = "Đăng nhập", Left = 130, Top = 150, Width = 105, Height = 32 };
        var exit = new Button { Text = "Thoát", Left = 245, Top = 150, Width = 105, Height = 32, DialogResult = DialogResult.Cancel };

        ok.Click += (s, e) => DoLogin();
        AcceptButton = ok;    // nhấn Enter = bấm Đăng nhập
        CancelButton = exit;  // nhấn Esc = bấm Thoát

        Controls.AddRange(new Control[]
        {
            title,
            new Label { Text = "Tài khoản", Left = 30, Top = 74, AutoSize = true },
            new Label { Text = "Mật khẩu", Left = 30, Top = 112, AutoSize = true },
            _user, _pass, ok, exit
        });
    }

    private void DoLogin()
    {
        if (string.IsNullOrWhiteSpace(_user.Text) || string.IsNullOrEmpty(_pass.Text))
        {
            UI.Info("Vui lòng nhập tài khoản và mật khẩu.");
            return;
        }
        try
        {
            using var db = Db.Create();
            var u = new AuthService(db).LoginStaff(_user.Text.Trim(), _pass.Text);

            if (u == null)
            {
                UI.Error(new Exception("Sai tài khoản/mật khẩu hoặc tài khoản bị khóa."));
                _pass.Clear();
                return;
            }
            CurrentUser = u;
            DialogResult = DialogResult.OK;
        }
        catch (Exception ex) { UI.Error(ex); }
    }
}