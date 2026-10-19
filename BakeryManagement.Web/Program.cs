using BakeryManagement.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<BakeryDbContext>(o => o.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddControllersWithViews();

var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BakeryDbContext>();
    DbInitializer.Seed(db);
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

// THỬ NGHIỆM (sẽ xóa ở giai đoạn sau)
app.MapGet("/test", (AuthService auth, ProductService products) =>
{
    var staff = auth.LoginStaff("admin", "Admin@123");
    var wrong = auth.LoginStaff("admin", "saimatkhau");
    var (items, total) = products.Search("bánh", null, 1, 5, true);
    return Results.Json(new
    {
        dangNhapDung = staff != null,
        quyen = staff?.Role.RoleName,
        dangNhapSai = wrong == null,
        soKetQuaTimBanh = total,
        monDauTien = items.Select(p => p.Name)
    });
});

app.MapGet("/test-order", (AuthService auth, ProductService products, OrderService orders) =>
{
    var c = auth.RegisterCustomer("Khách thử", $"thu{DateTime.Now.Ticks}@mail.com", "123456", "0900000000", "TP.HCM");
    var p = products.Search(null, null, 1, 1, true).Items[0];
    int truoc = p.Stock;

    var o = orders.PlaceOrder(c.CustomerId, new List<(int, int)> { (p.ProductId, 2) }, "123 Nguyễn Huệ", "0900000000", "thử");
    int sauDat = products.Get(p.ProductId).Stock;
    decimal tong = o.Total;

    string loiQuaSoLuong;
    try { orders.PlaceOrder(c.CustomerId, new List<(int, int)> { (p.ProductId, 99999) }, "x", "0900000000", ""); loiQuaSoLuong = "KHÔNG BÁO LỖI (sai!)"; }
    catch (InvalidOperationException ex) { loiQuaSoLuong = ex.Message; }

    orders.UpdateStatus(o.OrderId, OrderStatus.Cancelled);
    int sauHuy = products.Get(p.ProductId).Stock;

    return Results.Json(new { sanPham = p.Name, tonTruoc = truoc, tonSauDat = sauDat, tongTien = tong, loiQuaSoLuong, tonSauHuy = sauHuy });
});
app.Run();
