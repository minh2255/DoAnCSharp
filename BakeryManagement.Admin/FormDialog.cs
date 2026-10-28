namespace BakeryManagement.Admin;

class FormDialog : Form
{
    private int _y = 15;
    private readonly Dictionary<string, Control> _c = new();
    public Func<FormDialog, string> Validator;

    public FormDialog(string title)
    {
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;
        ClientSize = new Size(470, 100);
    }

    // Thêm một dòng: nhãn bên trái + ô nhập bên phải
    private void Row(string key, string label, Control c, int extra = 0)
    {
        Controls.Add(new Label { Text = label, Left = 15, Top = _y + 4, Width = 120 });
        c.Left = 140; c.Top = _y; c.Width = 310;
        Controls.Add(c);
        _c[key] = c;
        _y += 34 + extra;
    }

    public void AddText(string key, string label, string value = "", bool multiline = false, bool password = false)
    {
        var t = new TextBox { Text = value ?? "", Multiline = multiline, UseSystemPasswordChar = password };
        if (multiline) t.Height = 60;
        Row(key, label, t, multiline ? 26 : 0);
    }

    public void AddCombo(string key, string label, List<KeyValuePair<int, string>> items, int? selected)
    {
        var cb = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            DisplayMember = "Value",
            ValueMember = "Key",
            DataSource = items
        };
        if (selected.HasValue) cb.SelectedValue = selected.Value;
        Row(key, label, cb);
    }

    public void AddChoice(string key, string label, string[] items, string selected)
    {
        var cb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        cb.Items.AddRange(items);
        cb.SelectedItem = selected ?? items[0];
        Row(key, label, cb);
    }

    public void AddCheck(string key, string label, bool value) => Row(key, label, new CheckBox { Checked = value });

    // Các hàm đọc giá trị người dùng đã nhập
    public string Text_(string k) => ((TextBox)_c[k]).Text.Trim();
    public int Int(string k) => int.TryParse(Text_(k), out var v) ? v : -1;
    public decimal Dec(string k) => decimal.TryParse(Text_(k), out var v) ? v : -1;
    public int Combo(string k) => (int)((ComboBox)_c[k]).SelectedValue;
    public string Choice(string k) => (string)((ComboBox)_c[k]).SelectedItem;
    public bool Check(string k) => ((CheckBox)_c[k]).Checked;

    // Gọi sau khi thêm hết ô nhập: tạo nút Lưu/Hủy và chỉnh kích thước form
    public void Finish()
    {
        var ok = new Button { Text = "Lưu", Left = 270, Top = _y + 10, Width = 85 };
        var cancel = new Button { Text = "Hủy", Left = 365, Top = _y + 10, Width = 85, DialogResult = DialogResult.Cancel };

        ok.Click += (s, e) =>
        {
            var err = Validator?.Invoke(this);
            if (!string.IsNullOrEmpty(err))
            {
                MessageBox.Show(err, "Dữ liệu chưa hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            DialogResult = DialogResult.OK;
        };

        Controls.AddRange(new Control[] { ok, cancel });
        AcceptButton = ok; CancelButton = cancel;
        ClientSize = new Size(470, _y + 55);
    }
}