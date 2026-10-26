using BakeryManagement.Data;

namespace BakeryManagement.Admin;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        try
        {
            using var db = Db.Create();
            DbInitializer.Seed(db);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Không kết nối được SQL Server:\n" + ex.Message +
                "\n\nKiểm tra ConnectionString trong appsettings.json.",
                "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        while (true)
        {
            using var login = new LoginForm();
            if (login.ShowDialog() != DialogResult.OK) return;
            using var main = new MainForm(login.CurrentUser);
            Application.Run(main);

            if (!main.LoggedOut) return;
        }
    }
}