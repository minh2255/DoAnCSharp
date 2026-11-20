namespace BakeryManagement.Admin;

class FormDialog : Form
{
    private int _y;
    // Quy đổi kích thước điểm ảnh theo độ phóng to màn hình (100% = 96 DPI) để không bị cắt chữ/nút
    private int S(int v) => (int)Math.Round(v * DeviceDpi / 96.0);
    private readonly Dictionary<string, Control> _c = new();
    public Func<FormDialog, string> Validator;

    public FormDialog(string title)
    {
        _y = S(15);
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false; MinimizeBox = false; ShowInTaskbar = false;
        ClientSize = new Size(S(470), S(100));
    }

    // Thêm một dòng: nhãn bên trái + ô nhập bên phải
    private void Row(string key, string label, Control c, int extra = 0)
    {
        Controls.Add(new Label { Text = label, Left = S(15), Top = _y + S(4), Width = S(120), Height = S(24) });
        c.Left = S(140); c.Top = _y; c.Width = S(310);
        Controls.Add(c);
        _c[key] = c;
        _y += S(34) + S(extra);
    }

    public void AddText(string key, string label, string value = "", bool multiline = false, bool password = false, string hint = null)
    {
        // hint: dòng chữ mờ gợi ý hiện trong ô khi ô đang trống
        var t = new TextBox { Text = value ?? "", Multiline = multiline, UseSystemPasswordChar = password, PlaceholderText = hint ?? "" };
        if (multiline) t.Height = S(60);
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

        // Chỉ chọn mục khi hộp đã có dữ liệu; làm lại khi hộp được tạo/hiện ra
        void Apply()
        {
            if (!selected.HasValue || cb.Items.Count == 0) return;
            var idx = items.FindIndex(i => i.Key == selected.Value);
            if (idx >= 0 && idx < cb.Items.Count) cb.SelectedIndex = idx;
        }
        cb.HandleCreated += (s, e) => Apply();
        cb.VisibleChanged += (s, e) => { if (cb.Visible) Apply(); };

        Row(key, label, cb);
    }

    public void AddChoice(string key, string label, string[] items, string selected)
    {
        var cb = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        cb.Items.AddRange(items);
        cb.SelectedItem = selected ?? items[0];
        Row(key, label, cb);
    }

    // Dòng chọn ảnh: ô hiển thị đường dẫn + nút "Chọn ảnh..." mở hộp thoại chọn tệp (không cần gõ link)
    public void AddImage(string key, string label, string value)
    {
        var t = new TextBox { Text = value ?? "", ReadOnly = true };
        var b = new Button { Text = "Chọn ảnh..." };
        Controls.Add(new Label { Text = label, Left = S(15), Top = _y + S(4), Width = S(120), Height = S(24) });
        t.Left = S(140); t.Top = _y; t.Width = S(205);
        b.Left = S(350); b.Top = _y - S(1); b.Width = S(100); b.Height = S(30);
        b.Click += (s, e) =>
        {
            using var dlg = new OpenFileDialog { Title = "Chọn ảnh sản phẩm", Filter = "Ảnh|*.jpg;*.jpeg;*.png;*.webp;*.gif" };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            try { t.Text = ImageStore.Import(dlg.FileName); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Không chép được ảnh", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
        };
        Controls.Add(t); Controls.Add(b);
        _c[key] = t;
        _y += S(34);
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
        var ok = new Button { Text = "Lưu", Left = S(270), Top = _y + S(10), Width = S(85), Height = S(32) };
        var cancel = new Button { Text = "Hủy", Left = S(365), Top = _y + S(10), Width = S(85), Height = S(32), DialogResult = DialogResult.Cancel };

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
        ClientSize = new Size(S(470), _y + S(60));
    }
}