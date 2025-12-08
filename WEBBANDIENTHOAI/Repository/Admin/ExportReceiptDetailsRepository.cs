using Microsoft.EntityFrameworkCore;
using WEBBANDIENTHOAI.Data;
using WEBBANDIENTHOAI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WEBBANDIENTHOAI.Repository.Admin
{
    public interface IExportReceiptDetailsRepository
    {
        Task<List<ExportReceiptDetail>> GetByExportReceiptIdAsync(int exportReceiptId);
        Task<ExportReceiptDetail> GetByIdAsync(int exportDetailId);
        Task<bool> AddAsync(ExportReceiptDetail detail);
        Task<bool> UpdateAsync(ExportReceiptDetail detail);
        Task<bool> DeleteAsync(int exportDetailId);
        Task<List<ExportReceiptDetail>> GetByProductIdAsync(int productId);
        Task<List<ExportReceiptDetail>> GetByStockCodeAsync(string stockCode);
        Task<decimal> GetTotalExportedByProductIdAsync(int productId, DateTime? fromDate = null, DateTime? toDate = null);
    }

    public class ExportReceiptDetailsRepository : IExportReceiptDetailsRepository
    {
        private readonly AppDbContext _context;

        public ExportReceiptDetailsRepository(AppDbContext context)
        {
            _context = context;
        }

        // Lấy tất cả chi tiết theo ExportReceiptId
        public async Task<List<ExportReceiptDetail>> GetByExportReceiptIdAsync(int exportReceiptId)
        {
            try
            {
                return await _context.ExportReceiptDetails
                    .Include(d => d.Product)
                        .ThenInclude(p => p.Category)
                    .Include(d => d.ExportReceipt)
                    .Where(d => d.ExportReceiptId == exportReceiptId)
                    .OrderBy(d => d.ExportDetailId)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy chi tiết phiếu xuất: {ex.Message}", ex);
            }
        }

        // Lấy chi tiết theo ID
        public async Task<ExportReceiptDetail> GetByIdAsync(int exportDetailId)
        {
            try
            {
                return await _context.ExportReceiptDetails
                    .Include(d => d.Product)
                    .Include(d => d.ExportReceipt)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.ExportDetailId == exportDetailId);
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy chi tiết: {ex.Message}", ex);
            }
        }

        // Thêm chi tiết mới
        public async Task<bool> AddAsync(ExportReceiptDetail detail)
        {
            try
            {
                if (detail == null)
                    throw new ArgumentNullException(nameof(detail));

                // Validate
                if (detail.Quantity <= 0)
                    throw new Exception("Số lượng phải lớn hơn 0");

                if (detail.UnitPrice < 0)
                    throw new Exception("Đơn giá không được âm");

                // Tính tổng tiền
                detail.TotalPrice = detail.Quantity * detail.UnitPrice;
                detail.CreatedAt = DateTime.UtcNow;

                await _context.ExportReceiptDetails.AddAsync(detail);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi thêm chi tiết: {ex.Message}", ex);
            }
        }

        // Cập nhật chi tiết
        public async Task<bool> UpdateAsync(ExportReceiptDetail detail)
        {
            try
            {
                if (detail == null)
                    throw new ArgumentNullException(nameof(detail));

                var existing = await _context.ExportReceiptDetails
                    .FirstOrDefaultAsync(d => d.ExportDetailId == detail.ExportDetailId);

                if (existing == null)
                    throw new Exception("Không tìm thấy chi tiết");

                // Validate
                if (detail.Quantity <= 0)
                    throw new Exception("Số lượng phải lớn hơn 0");

                if (detail.UnitPrice < 0)
                    throw new Exception("Đơn giá không được âm");

                // Cập nhật thông tin
                existing.ProductId = detail.ProductId;
                existing.StockCode = detail.StockCode;
                existing.SnapshotSKU = detail.SnapshotSKU;
                existing.SnapshotName = detail.SnapshotName;
                existing.SnapshotBrand = detail.SnapshotBrand;
                existing.Quantity = detail.Quantity;
                existing.UnitPrice = detail.UnitPrice;
                existing.TotalPrice = detail.Quantity * detail.UnitPrice;

                _context.ExportReceiptDetails.Update(existing);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi cập nhật chi tiết: {ex.Message}", ex);
            }
        }

        // Xóa chi tiết
        public async Task<bool> DeleteAsync(int exportDetailId)
        {
            try
            {
                var detail = await _context.ExportReceiptDetails
                    .FirstOrDefaultAsync(d => d.ExportDetailId == exportDetailId);

                if (detail == null)
                    throw new Exception("Không tìm thấy chi tiết");

                _context.ExportReceiptDetails.Remove(detail);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi xóa chi tiết: {ex.Message}", ex);
            }
        }

        // Lấy chi tiết theo ProductId
        public async Task<List<ExportReceiptDetail>> GetByProductIdAsync(int productId)
        {
            try
            {
                return await _context.ExportReceiptDetails
                    .Include(d => d.ExportReceipt)
                        .ThenInclude(e => e.Customer)
                    .Include(d => d.Product)
                    .Where(d => d.ProductId == productId)
                    .OrderByDescending(d => d.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy lịch sử xuất của sản phẩm: {ex.Message}", ex);
            }
        }

        // Lấy chi tiết theo StockCode
        public async Task<List<ExportReceiptDetail>> GetByStockCodeAsync(string stockCode)
        {
            try
            {
                return await _context.ExportReceiptDetails
                    .Include(d => d.ExportReceipt)
                    .Include(d => d.Product)
                    .Where(d => d.StockCode == stockCode)
                    .OrderByDescending(d => d.CreatedAt)
                    .AsNoTracking()
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi lấy lịch sử xuất theo mã kho: {ex.Message}", ex);
            }
        }

        // Tính tổng số lượng đã xuất của sản phẩm trong khoảng thời gian
        public async Task<decimal> GetTotalExportedByProductIdAsync(int productId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            try
            {
                var query = _context.ExportReceiptDetails
                    .Include(d => d.ExportReceipt)
                    .Where(d => d.ProductId == productId);

                if (fromDate.HasValue)
                {
                    query = query.Where(d => d.ExportReceipt.ExportDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(d => d.ExportReceipt.ExportDate <= toDate.Value);
                }

                var total = await query.SumAsync(d => (decimal?)d.Quantity) ?? 0;
                return total;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi khi tính tổng xuất: {ex.Message}", ex);
            }
        }
    }
}