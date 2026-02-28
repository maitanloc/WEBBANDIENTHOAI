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

        // Tạo mã phiếu xuất tự động
        public async Task<string> GenerateReceiptNumberAsync()
        {
            try
            {
                var today = DateTime.UtcNow.ToString("yyyyMMdd");
                var lastReceipt = await _context.ExportReceipts
                    .Where(e => e.ReceiptNumber.StartsWith($"PX{today}"))
                    .OrderByDescending(e => e.ReceiptNumber)
                    .FirstOrDefaultAsync();

                if (lastReceipt == null)
                {
                    return $"PX{today}-001";
                }

                var lastNumberStr = lastReceipt.ReceiptNumber.Substring(lastReceipt.ReceiptNumber.LastIndexOf('-') + 1);
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    return $"PX{today}-{(lastNumber + 1):D3}";
                }

                return $"PX{today}-001";
            }
            catch (Exception ex)
            {
                // Trong trường hợp lỗi, fallback về timestamp
                return $"PX{DateTime.UtcNow.ToString("yyyyMMddHHmmss")}";
            }
        }

        // Tạo phiếu xuất bằng EF Core (thay cho stored procedure)
        public async Task<bool> CreateExportReceiptAsync(ExportReceiptCreateDto dto)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                decimal totalValue = dto.Items.Sum(i => i.Quantity * i.UnitPrice);
                if (dto.OrderId.HasValue)
                {
                    var order = await _context.Orders.FindAsync(dto.OrderId.Value);
                    if (order != null)
                    {
                        totalValue = order.Total;
                    }
                }

                var exportReceipt = new ExportReceipt
                {
                    ReceiptNumber = dto.ReceiptNumber,
                    ExportDate = DateTime.UtcNow,
                    CustomerId = dto.CustomerId,
                    OrderId = dto.OrderId,
                    CreatedByUserId = dto.CreatedByUserId,
                    Notes = dto.Notes,
                    TotalQuantity = dto.Items.Sum(i => i.Quantity),
                    TotalValue = totalValue
                };

                _context.ExportReceipts.Add(exportReceipt);
                await _context.SaveChangesAsync();

                foreach (var item in dto.Items)
                {
                    var detail = new ExportReceiptDetail
                    {
                        ExportReceiptId = exportReceipt.ExportReceiptId,
                        ProductId = item.ProductId,
                        StockCode = item.StockCode,
                        SnapshotSKU = item.SKU,
                        SnapshotName = item.Name,
                        SnapshotBrand = item.Brand,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.Quantity * item.UnitPrice
                    };
                    _context.ExportReceiptDetails.Add(detail);

                    // Cập nhật tồn kho
                    var inventory = await _context.Inventory
                        .FirstOrDefaultAsync(i => i.ProductId == item.ProductId && i.StockCode == item.StockCode);
                    
                    if (inventory != null)
                    {
                        inventory.CurrentQuantity -= item.Quantity;
                        if (inventory.CurrentQuantity < 0) inventory.CurrentQuantity = 0; // Tránh tồn kho âm
                        inventory.LastUpdated = DateTime.UtcNow;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi khi tạo phiếu xuất: {ex.Message}", ex);
            }
        }
    }
}
