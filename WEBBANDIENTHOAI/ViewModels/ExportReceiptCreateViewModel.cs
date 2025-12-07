using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WEBBANDIENTHOAI.ViewModels
{
    // ViewModel cho form tạo phiếu xuất
    public class ExportReceiptCreateViewModel
    {
        public int? OrderId { get; set; }

        public int? CustomerId { get; set; }

        public string CustomerName { get; set; }

        public string CustomerPhone { get; set; }

        public string ShippingAddress { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        public List<ExportItemViewModel> Items { get; set; } = new List<ExportItemViewModel>();
    }

    // ViewModel cho từng item trong form tạo phiếu
    public class ExportItemViewModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string StockCode { get; set; }

        public string SKU { get; set; }

        public string Brand { get; set; }

        [Required(ErrorMessage = "Số lượng là bắt buộc")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice => Quantity * UnitPrice;

        public int CurrentStock { get; set; }  // Tồn kho hiện tại

        public bool IsOutOfStock => CurrentStock < Quantity;
    }

    // ViewModel cho danh sách phiếu xuất
    public class ExportReceiptListViewModel
    {
        public int ExportReceiptId { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTime ExportDate { get; set; }
        public string CustomerName { get; set; }
        public int? OrderId { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
        public string CreatedByUserName { get; set; }
        public string Notes { get; set; }
    }

    // ViewModel cho chi tiết phiếu xuất
    public class ExportReceiptDetailsViewModel
    {
        public int ExportReceiptId { get; set; }
        public string ReceiptNumber { get; set; }
        public DateTime ExportDate { get; set; }
        public string CustomerName { get; set; }
        public string CustomerPhone { get; set; }
        public string ShippingAddress { get; set; }
        public int? OrderId { get; set; }
        public int TotalQuantity { get; set; }
        public decimal TotalValue { get; set; }
        public string CreatedByUserName { get; set; }
        public string Notes { get; set; }
        public List<ExportDetailItemViewModel> Items { get; set; } = new List<ExportDetailItemViewModel>();
    }

    // ViewModel cho từng item trong chi tiết phiếu
    public class ExportDetailItemViewModel
    {
        public int ExportDetailId { get; set; }
        public string ProductName { get; set; }
        public string SKU { get; set; }
        public string Brand { get; set; }
        public string StockCode { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}