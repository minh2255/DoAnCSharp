using Microsoft.EntityFrameworkCore;

namespace BakeryManagement.Data;

public class ProductService
{
    private readonly BakeryDbContext _db;
    public ProductService(BakeryDbContext db) => _db = db;

    public List<Category> Categories() => _db.Categories.OrderBy(c => c.Name).ToList();

    // Tìm kiếm + lọc danh mục + phân trang. Trả về (danh sách, tổng số kết quả)
    public (List<Product> Items, int Total) Search(string keyword, int? categoryId, int page, int pageSize, bool onlyActive)
    {
        var q = _db.Products.Include(p => p.Category).AsQueryable();

        if (onlyActive) q = q.Where(p => p.IsActive);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            q = q.Where(p => p.Name.Contains(k) || p.Description.Contains(k));
        }

        if (categoryId.HasValue) q = q.Where(p => p.CategoryId == categoryId);

        var total = q.Count();
        var items = q.OrderBy(p => p.Name)
                     .Skip((page - 1) * pageSize)
                     .Take(pageSize)
                     .ToList();
        return (items, total);
    }

    // Lấy sản phẩm theo ID
    public Product Get(int id) =>
        _db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductId == id);

    // Lấy n sản phẩm mới nhất đang bán
    public List<Product> Latest(int n) =>
        _db.Products.Include(p => p.Category).Where(p => p.IsActive).OrderByDescending(p => p.ProductId).Take(n).ToList();

    // Thêm mới hoặc cập nhật sản phẩm
    public void Save(Product p)
    {
        if (string.IsNullOrWhiteSpace(p.Name)) throw new ArgumentException("Tên bánh không được để trống.");
        if (p.Price < 0) throw new ArgumentException("Giá không hợp lệ.");
        if (p.Stock < 0) throw new ArgumentException("Tồn kho không hợp lệ.");

        if (p.ProductId == 0) _db.Products.Add(p);   // chưa có ID = thêm mới
        else _db.Products.Update(p);                 // có ID = cập nhật
        _db.SaveChanges();
    }

    // Xóa sản phẩm(true: xóa hẳn hoặc sản phẩm không tồn tại, false: sản phẩm có trong đơn hàng, chỉ chuyển sang ngừng bán)
    public bool Delete(int id)
    {
        var p = _db.Products.Find(id);
        if (p == null) return true;

        if (_db.OrderDetails.Any(d => d.ProductId == id))
        {
            p.IsActive = false;         // đã có trong đơn hàng => giữ lại để không mất lịch sử
            _db.SaveChanges();
            return false;
        }
        _db.Products.Remove(p);
        _db.SaveChanges();
        return true;
    }

    // Thêm mới hoặc cập nhật danh mục
    public void SaveCategory(Category c)
    {
        if (string.IsNullOrWhiteSpace(c.Name)) throw new ArgumentException("Tên danh mục không được để trống.");
        if (_db.Categories.Any(x => x.Name == c.Name && x.CategoryId != c.CategoryId))
            throw new InvalidOperationException("Tên danh mục đã tồn tại.");

        if (c.CategoryId == 0) _db.Categories.Add(c); else _db.Categories.Update(c);
        _db.SaveChanges();
    }

    // Xóa danh mục nếu không còn sản phẩm
    public void DeleteCategory(int id)
    {
        if (_db.Products.Any(p => p.CategoryId == id))
            throw new InvalidOperationException("Danh mục còn sản phẩm, không thể xóa.");
        var c = _db.Categories.Find(id);
        if (c != null) { _db.Categories.Remove(c); _db.SaveChanges(); }
    }
}

