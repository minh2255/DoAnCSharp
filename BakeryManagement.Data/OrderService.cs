using Microsoft.EntityFrameworkCore;

namespace BakeryManagement.Data;

public class OrderService
{
    private readonly BakeryDbContext _db;
    public OrderService(BakeryDbContext db) => _db = db;

    // paymentMethod: COD (trả khi nhận) hoặc Online (đã trả qua cổng, kèm mã giao dịch)
    public Order PlaceOrder(int customerId, List<(int ProductId, int Qty)> items,
                            string address, string phone, string note,
                            string paymentMethod = PaymentMethods.Cod, string? transactionCode = null)
    {
        if (items.Count == 0) throw new InvalidOperationException("Giỏ hàng trống.");
        if (paymentMethod != PaymentMethods.Cod && paymentMethod != PaymentMethods.Online)
            throw new InvalidOperationException("Phương thức thanh toán không hợp lệ.");
        var paidOnline = paymentMethod == PaymentMethods.Online;

        using var tx = _db.Database.BeginTransaction();   // bắt đầu transaction

        var order = new Order
        {
            CustomerId = customerId,
            ShippingAddress = address,
            Phone = phone,
            Note = note ?? "",
            OrderDate = DateTime.Now,
            Status = OrderStatus.Pending,
            PaymentMethod = paymentMethod,
            // Đơn trực tuyến chỉ được tạo SAU KHI cổng báo thanh toán thành công nên ghi luôn là đã thanh toán
            PaymentStatus = paidOnline ? PaymentStatuses.Paid : PaymentStatuses.Unpaid,
            PaidAt = paidOnline ? DateTime.Now : null,
            TransactionCode = paidOnline ? transactionCode : null
        };

        foreach (var (pid, qty) in items)
        {
            var p = _db.Products.Find(pid);
            if (p == null || !p.IsActive) throw new InvalidOperationException("Có sản phẩm không còn được bán.");
            if (qty <= 0) throw new InvalidOperationException("Số lượng không hợp lệ.");
            if (p.Stock < qty) throw new InvalidOperationException($"'{p.Name}' chỉ còn {p.Stock} sản phẩm.");

            p.Stock -= qty;   // trừ tồn kho
            order.Details.Add(new OrderDetail { ProductId = pid, Quantity = qty, UnitPrice = p.Price });
        }

        _db.Orders.Add(order);
        _db.SaveChanges();
        tx.Commit();          // mọi thứ OK mới xác nhận
        return order;
    }

    public List<Order> ByCustomer(int customerId) =>
        _db.Orders.Include(o => o.Details).ThenInclude(d => d.Product)
        .Where(o => o.CustomerId == customerId)
        .OrderByDescending(o => o.OrderDate).ToList();

    public List<Order> All(string keyword = null, string status = null)
    {
        var q = _db.Orders.Include(o => o.Customer).Include(o => o.Details).AsQueryable();

        if (!string.IsNullOrWhiteSpace(status)) q = q.Where(o => o.Status == status);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var k = keyword.Trim();
            q = q.Where(o => o.Customer.FullName.Contains(k) || o.Phone.Contains(k) || o.OrderId.ToString() == k);
        }
        return q.OrderByDescending(o => o.OrderDate).ToList();
    }

    public Order Get(int id) =>
        _db.Orders.Include(o => o.Customer).Include(o => o.Details).ThenInclude(d => d.Product)
        .FirstOrDefault(o => o.OrderId == id);

    public void UpdateStatus(int orderId, string newStatus)
    {
        var o = _db.Orders.Include(x => x.Details).FirstOrDefault(x => x.OrderId == orderId)
                ?? throw new InvalidOperationException("Không tìm thấy đơn hàng.");

        if (o.Status == OrderStatus.Cancelled)
            throw new InvalidOperationException("Đơn đã hủy, không thể đổi trạng thái.");
        if (o.Status == OrderStatus.Completed)
            throw new InvalidOperationException("Đơn đã hoàn thành, không thể đổi trạng thái.");
        if (!OrderStatus.CanChange(o.Status, newStatus))
            throw new InvalidOperationException($"Không thể chuyển từ '{o.Status}' sang '{newStatus}'.");

        // Đơn COD: hoàn thành nghĩa là đã thu tiền khi giao
        if (newStatus == OrderStatus.Completed && o.PaymentStatus == PaymentStatuses.Unpaid)
        {
            o.PaymentStatus = PaymentStatuses.Paid;
            o.PaidAt = DateTime.Now;
        }

        // Đơn đã trả tiền trực tuyến mà bị hủy thì đánh dấu cần hoàn tiền cho khách
        if (newStatus == OrderStatus.Cancelled && o.PaymentStatus == PaymentStatuses.Paid)
            o.PaymentStatus = PaymentStatuses.Refund;

        if (newStatus == OrderStatus.Cancelled)
            foreach (var d in o.Details)
                _db.Products.Find(d.ProductId).Stock += d.Quantity;   // hoàn lại tồn kho

        o.Status = newStatus;
        _db.SaveChanges();
    }

    // Khách tự hủy: chỉ hủy được đơn CỦA MÌNH và còn "Chờ xác nhận"
    public void CancelByCustomer(int orderId, int customerId)
    {
        var o = _db.Orders.FirstOrDefault(x => x.OrderId == orderId && x.CustomerId == customerId)
                ?? throw new InvalidOperationException("Không tìm thấy đơn hàng.");
        if (o.Status != OrderStatus.Pending)
            throw new InvalidOperationException("Chỉ hủy được đơn đang chờ xác nhận.");
        UpdateStatus(orderId, OrderStatus.Cancelled);
    }

    // Thống kê cho Dashboard
    // Revenue: doanh thu ĐÃ THU = tổng các đơn có trạng thái thanh toán "Đã thanh toán"
    //          (đơn trực tuyến đã trả + đơn COD đã hoàn thành).
    // Pending: giá trị đơn ĐANG XỬ LÝ = đơn chưa hoàn thành, chưa hủy và chưa thanh toán (tiền sắp thu).
    // Đơn đã trả tiền mà bị hủy chuyển sang "Cần hoàn tiền" nên tự động không còn tính vào doanh thu.
    public (int Orders, decimal Revenue, decimal Pending, int LowStock, int Customers) Stats()
    {
        var orders = _db.Orders.Count();
        var revenue = _db.OrderDetails
            .Where(d => d.Order.PaymentStatus == PaymentStatuses.Paid)
            .Select(d => (decimal?)(d.Quantity * d.UnitPrice)).Sum() ?? 0;
        var pending = _db.OrderDetails
            .Where(d => d.Order.Status != OrderStatus.Cancelled
                     && d.Order.Status != OrderStatus.Completed
                     && d.Order.PaymentStatus == PaymentStatuses.Unpaid)
            .Select(d => (decimal?)(d.Quantity * d.UnitPrice)).Sum() ?? 0;
        var low = _db.Products.Count(p => p.IsActive && p.Stock <= 5);
        return (orders, revenue, pending, low, _db.Customers.Count());
    }

    public List<(string Name, int Qty)> TopProducts(int n = 5) =>
        _db.OrderDetails.Where(d => d.Order.Status != OrderStatus.Cancelled)
        .GroupBy(d => d.Product.Name)
        .Select(g => new { g.Key, Qty = g.Sum(x => x.Quantity) })
        .OrderByDescending(x => x.Qty).Take(n)
        .AsEnumerable().Select(x => (x.Key, x.Qty)).ToList();
}