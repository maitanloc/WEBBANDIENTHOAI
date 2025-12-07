using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using WEBBANDIENTHOAI.Repository.Admin;

namespace WEBBANDIENTHOAI.Repository.Admin
{
    public interface IExportReceiptRepository
    {
        Task<PagedResult<ExportReceipt>> GetAllAsync(string search = "", string fromDate = "", string toDate = "", string sortBy = "newest", int page = 1, int pageSize = 10);
        Task<ExportReceipt> GetByIdAsync(int id);
        Task<ExportReceipt> GetByReceiptNumberAsync(string receiptNumber);
        Task<bool> CreateExportReceiptAsync(ExportReceiptCreateDto dto);
        Task<string> GenerateReceiptNumberAsync();
        Task<List<ExportReceipt>> GetByOrderIdAsync(int orderId);
        Task<bool> CanExportForOrderAsync(int orderId);
    }

    public class ExportReceiptRepository : IExportReceiptRepository
    {
        private readonly AppDbContext _context;

        public ExportReceiptRepository(AppDbContext context)
        {
            _context = context;
        }

        // Lấy danh sách phiếu xuất với filter, search, pagination
        public async Task<PagedResult<ExportReceipt>> GetAllAsync(
            string search = "",
            string fromDate = "",
            string toDate = "",
            string sortBy = "newest",
            int page = 1,
            int pageSize = 10)
        {
            try
            {
                var query = _context.ExportReceipts
                    .Include(e => e.Customer)
                    .Include(e => e.Order)
                    .Include(e => e.CreatedByUser)
                    .AsQueryable();

                // Tìm kiếm
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(e =>
                        e.ReceiptNumber.Contains(search) ||
                        e.Customer.FullName.Contains(search) ||
                        (e.OrderId.HasValue && e.OrderId.ToString().Contains(search)));
                }

                // Lọc theo ngày
                if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out var from))
                {
                    query = query.Where(e => e.ExportDate.Date >= from.Date);
                }

                if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out var to))
                {
                    query = query.Where(e => e.ExportDate.Date <= to.Date);
                }

                // Sắp xếp
                query = sortBy switch
                {
                    "oldest" => query.OrderBy(e => e.ExportDate),
                    "highest" => query.OrderByDescending(e => e.TotalValue),
                    _ => query.OrderByDescending(e => e.ExportDate)
                };

                var totalCount = await query.CountAsync();
                var items = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .AsNoTracking()
                    .ToListAsync();

                return new PagedResult<ExportReceipt>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy danh sách phiếu xuất: {ex.Message}");
            }
        }

        // Lấy phiếu xuất theo ID
        public async Task<ExportReceipt> GetByIdAsync(int id)
        {
            try
            {
                return await _context.ExportReceipts
                    .Include(e => e.Customer)
                    .Include(e => e.Order)
                    .Include(e => e.CreatedByUser)
                    .Include(e => e.ExportReceiptDetails)
                        .ThenInclude(d => d.Product)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.ExportReceiptId == id);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy phiếu xuất: {ex.Message}");
            }
        }

        // Lấy phiếu xuất theo mã phiếu
        public async Task<ExportReceipt> GetByReceiptNumberAsync(string receiptNumber)
        {
            try
            {
                return await _context.ExportReceipts
                    .Include(e => e.ExportReceiptDetails)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.ReceiptNumber == receiptNumber);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy phiếu xuất: {ex.Message}");
            }
        }

        // Tạo phiếu xuất bằng stored procedure
        public async Task<bool> CreateExportReceiptAsync(ExportReceiptCreateDto dto)
        {
            try
            {
                // Tạo DataTable cho TVP (Table-Valued Parameter)
                var itemsTable = new DataTable();
                itemsTable.Columns.Add("ProductId", typeof(int));
                itemsTable.Columns.Add("StockCode", typeof(string));
                itemsTable.Columns.Add("SKU", typeof(string));
                itemsTable.Columns.Add("Name", typeof(string));
                itemsTable.Columns.Add("Quantity", typeof(int));
                itemsTable.Columns.Add("UnitPrice", typeof(decimal));

                foreach (var item in dto.Items)
                {
                    itemsTable.Rows.Add(
                        item.ProductId.HasValue ? (object)item.ProductId.Value : DBNull.Value,
                        item.StockCode,
                        item.SKU ?? string.Empty,
                        item.Name ?? string.Empty,
                        item.Quantity,
                        item.UnitPrice
                    );
                }

                // Gọi stored procedure
                var receiptNumberParam = new SqlParameter("@ReceiptNumber", dto.ReceiptNumber);
                var customerIdParam = new SqlParameter("@CustomerId", dto.CustomerId.HasValue ? (object)dto.CustomerId.Value : DBNull.Value);
                var orderIdParam = new SqlParameter("@OrderId", dto.OrderId.HasValue ? (object)dto.OrderId.Value : DBNull.Value);
                var createdByUserIdParam = new SqlParameter("@CreatedByUserId", dto.CreatedByUserId);
                var notesParam = new SqlParameter("@Notes", string.IsNullOrEmpty(dto.Notes) ? (object)DBNull.Value : dto.Notes);
                var itemsParam = new SqlParameter("@Items", SqlDbType.Structured)
                {
                    TypeName = "dbo.ExportItemType",
                    Value = itemsTable
                };

                await _context.Database.ExecuteSqlRawAsync(
                    "EXEC dbo.usp_CreateExportReceipt @ReceiptNumber, @CustomerId, @OrderId, @CreatedByUserId, @Notes, @Items",
                    receiptNumberParam,
                    customerIdParam,
                    orderIdParam,
                    createdByUserIdParam,
                    notesParam,
                    itemsParam
                );

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tạo phiếu xuất: {ex.Message}", ex);
            }
        }

        // Generate mã phiếu xuất tự động
        public async Task<string> GenerateReceiptNumberAsync()
        {
            try
            {
                var today = DateTime.Now.ToString("yyyyMMdd");
                var prefix = $"PX-{today}-";

                var lastReceipt = await _context.ExportReceipts
                    .Where(e => e.ReceiptNumber.StartsWith(prefix))
                    .OrderByDescending(e => e.ReceiptNumber)
                    .FirstOrDefaultAsync();

                if (lastReceipt == null)
                {
                    return $"{prefix}0001";
                }

                var lastNumber = lastReceipt.ReceiptNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumber, out var number))
                {
                    return $"{prefix}{(number + 1):D4}";
                }

                return $"{prefix}0001";
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tạo mã phiếu: {ex.Message}");
            }
        }

        // Lấy danh sách phiếu xuất theo OrderId
        public async Task<List<ExportReceipt>> GetByOrderIdAsync(int orderId)
        {
            try
            {
                return await _context.ExportReceipts
                    .Where(e => e.OrderId == orderId)
                    .Include(e => e.ExportReceiptDetails)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy phiếu xuất theo đơn hàng: {ex.Message}");
            }
        }

        // Kiểm tra đơn hàng có thể xuất không
        public async Task<bool> CanExportForOrderAsync(int orderId)
        {
            try
            {
                // Kiểm tra đơn hàng tồn tại
                var order = await _context.Orders.FindAsync(orderId);
                if (order == null) return false;

                // Kiểm tra trạng thái đơn hàng = 3 (Completed)
                if (order.StatusId != 3) return false;

                // Kiểm tra đã xuất hàng chưa
                var hasExported = await _context.ExportReceipts
                    .AnyAsync(e => e.OrderId == orderId);

                return !hasExported;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi kiểm tra đơn hàng: {ex.Message}");
            }
        }
    }
}

    public interface IExportReceiptRepository
{
    Task<PagedResult<ExportReceipt>> GetAllAsync(string search = "", string fromDate = "", string toDate = "", string sortBy = "newest", int page = 1, int pageSize = 10);
    Task<ExportReceipt> GetByIdAsync(int id);
    Task<ExportReceipt> GetByReceiptNumberAsync(string receiptNumber);
    Task<bool> CreateExportReceiptAsync(ExportReceiptCreateDto dto);
    Task<string> GenerateReceiptNumberAsync();
    Task<List<ExportReceipt>> GetByOrderIdAsync(int orderId);
    Task<bool> CanExportForOrderAsync(int orderId);
}

public class ExportReceiptRepository : IExportReceiptRepository
{
    private readonly AppDbContext _context;

    public ExportReceiptRepository(AppDbContext context)
    {
        _context = context;
    }

    // Lấy danh sách phiếu xuất với filter, search, pagination
    public async Task<PagedResult<ExportReceipt>> GetAllAsync(
        string search = "",
        string fromDate = "",
        string toDate = "",
        string sortBy = "newest",
        int page = 1,
        int pageSize = 10)
    {
        try
        {
            var query = _context.ExportReceipts
                .Include(e => e.Customer)
                .Include(e => e.Order)
                .Include(e => e.CreatedByUser)
                .AsQueryable();

            // Tìm kiếm
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(e =>
                    e.ReceiptNumber.Contains(search) ||
                    e.Customer.FullName.Contains(search) ||
                    (e.OrderId.HasValue && e.OrderId.ToString().Contains(search)));
            }

            // Lọc theo ngày
            if (!string.IsNullOrEmpty(fromDate) && DateTime.TryParse(fromDate, out var from))
            {
                query = query.Where(e => e.ExportDate.Date >= from.Date);
            }

            if (!string.IsNullOrEmpty(toDate) && DateTime.TryParse(toDate, out var to))
            {
                query = query.Where(e => e.ExportDate.Date <= to.Date);
            }

            // Sắp xếp
            query = sortBy switch
            {
                "oldest" => query.OrderBy(e => e.ExportDate),
                "highest" => query.OrderByDescending(e => e.TotalValue),
                _ => query.OrderByDescending(e => e.ExportDate)
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return new PagedResult<ExportReceipt>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi lấy danh sách phiếu xuất: {ex.Message}");
        }
    }

    // Lấy phiếu xuất theo ID
    public async Task<ExportReceipt> GetByIdAsync(int id)
    {
        try
        {
            return await _context.ExportReceipts
                .Include(e => e.Customer)
                .Include(e => e.Order)
                .Include(e => e.CreatedByUser)
                .Include(e => e.ExportReceiptDetails)
                    .ThenInclude(d => d.Product)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ExportReceiptId == id);
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi lấy phiếu xuất: {ex.Message}");
        }
    }

    // Lấy phiếu xuất theo mã phiếu
    public async Task<ExportReceipt> GetByReceiptNumberAsync(string receiptNumber)
    {
        try
        {
            return await _context.ExportReceipts
                .Include(e => e.ExportReceiptDetails)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ReceiptNumber == receiptNumber);
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi lấy phiếu xuất: {ex.Message}");
        }
    }

    // Tạo phiếu xuất bằng stored procedure
    public async Task<bool> CreateExportReceiptAsync(ExportReceiptCreateDto dto)
    {
        try
        {
            // Tạo DataTable cho TVP (Table-Valued Parameter)
            var itemsTable = new DataTable();
            itemsTable.Columns.Add("ProductId", typeof(int));
            itemsTable.Columns.Add("StockCode", typeof(string));
            itemsTable.Columns.Add("SKU", typeof(string));
            itemsTable.Columns.Add("Name", typeof(string));
            itemsTable.Columns.Add("Quantity", typeof(int));
            itemsTable.Columns.Add("UnitPrice", typeof(decimal));

            foreach (var item in dto.Items)
            {
                itemsTable.Rows.Add(
                    item.ProductId.HasValue ? (object)item.ProductId.Value : DBNull.Value,
                    item.StockCode,
                    item.SKU ?? string.Empty,
                    item.Name ?? string.Empty,
                    item.Quantity,
                    item.UnitPrice
                );
            }

            // Gọi stored procedure
            var receiptNumberParam = new SqlParameter("@ReceiptNumber", dto.ReceiptNumber);
            var customerIdParam = new SqlParameter("@CustomerId", dto.CustomerId.HasValue ? (object)dto.CustomerId.Value : DBNull.Value);
            var orderIdParam = new SqlParameter("@OrderId", dto.OrderId.HasValue ? (object)dto.OrderId.Value : DBNull.Value);
            var createdByUserIdParam = new SqlParameter("@CreatedByUserId", dto.CreatedByUserId);
            var notesParam = new SqlParameter("@Notes", string.IsNullOrEmpty(dto.Notes) ? (object)DBNull.Value : dto.Notes);
            var itemsParam = new SqlParameter("@Items", SqlDbType.Structured)
            {
                TypeName = "dbo.ExportItemType",
                Value = itemsTable
            };

            await _context.Database.ExecuteSqlRawAsync(
                "EXEC dbo.usp_CreateExportReceipt @ReceiptNumber, @CustomerId, @OrderId, @CreatedByUserId, @Notes, @Items",
                receiptNumberParam,
                customerIdParam,
                orderIdParam,
                createdByUserIdParam,
                notesParam,
                itemsParam
            );

            return true;
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi tạo phiếu xuất: {ex.Message}", ex);
        }
    }

    // Generate mã phiếu xuất tự động
    public async Task<string> GenerateReceiptNumberAsync()
    {
        try
        {
            var today = DateTime.Now.ToString("yyyyMMdd");
            var prefix = $"PX-{today}-";

            var lastReceipt = await _context.ExportReceipts
                .Where(e => e.ReceiptNumber.StartsWith(prefix))
                .OrderByDescending(e => e.ReceiptNumber)
                .FirstOrDefaultAsync();

            if (lastReceipt == null)
            {
                return $"{prefix}0001";
            }

            var lastNumber = lastReceipt.ReceiptNumber.Substring(prefix.Length);
            if (int.TryParse(lastNumber, out var number))
            {
                return $"{prefix}{(number + 1):D4}";
            }

            return $"{prefix}0001";
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi tạo mã phiếu: {ex.Message}");
        }
    }

    // Lấy danh sách phiếu xuất theo OrderId
    public async Task<List<ExportReceipt>> GetByOrderIdAsync(int orderId)
    {
        try
        {
            return await _context.ExportReceipts
                .Where(e => e.OrderId == orderId)
                .Include(e => e.ExportReceiptDetails)
                .AsNoTracking()
                .ToListAsync();
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi lấy phiếu xuất theo đơn hàng: {ex.Message}");
        }
    }

    // Kiểm tra đơn hàng có thể xuất không
    public async Task<bool> CanExportForOrderAsync(int orderId)
    {
        try
        {
            // Kiểm tra đơn hàng tồn tại
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null) return false;

            // Kiểm tra trạng thái đơn hàng = 3 (Completed)
            if (order.StatusId != 3) return false;

            // Kiểm tra đã xuất hàng chưa
            var hasExported = await _context.ExportReceipts
                .AnyAsync(e => e.OrderId == orderId);

            return !hasExported;
        }
        catch (Exception ex)
        {
            throw new Exception($"Lỗi khi kiểm tra đơn hàng: {ex.Message}");
        }
    }
}
