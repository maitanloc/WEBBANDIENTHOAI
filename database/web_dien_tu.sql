/**********************************************
 * PhoneShopFull_Init_FullRealistic_Vietnamese.sql
 * Full DB schema + seed data (realistic, ~30 products)
 * Target: Microsoft SQL Server 2016+
 * All in Vietnamese
 **********************************************/
SET NOCOUNT ON;
GO
-- Create database if not exists
IF DB_ID(N'PhoneShopFull') IS NULL
BEGIN
    CREATE DATABASE PhoneShopFull;
    PRINT N'Đã tạo cơ sở dữ liệu PhoneShopFull1.';
END
GO
USE PhoneShopFull;
GO
/***************************************
 * Drop existing objects (safe for re-run)
 ***************************************/
-- Drop trong dependency order
IF OBJECT_ID('dbo.trg_ChiTietDonHang_Insert','TR') IS NOT NULL DROP TRIGGER dbo.trg_ChiTietDonHang_Insert;
IF OBJECT_ID('dbo.sp_TaoDonHangTuGio','P') IS NOT NULL DROP PROC dbo.sp_TaoDonHangTuGio;
IF OBJECT_ID('dbo.fn_TinhTongDonHang','FN') IS NOT NULL DROP FUNCTION dbo.fn_TinhTongDonHang;
IF OBJECT_ID('dbo.sp_XoaSanPham','P') IS NOT NULL DROP PROC dbo.sp_XoaSanPham;
IF OBJECT_ID('dbo.sp_CapNhatSanPham','P') IS NOT NULL DROP PROC dbo.sp_CapNhatSanPham;
IF OBJECT_ID('dbo.sp_ThemSanPham','P') IS NOT NULL DROP PROC dbo.sp_ThemSanPham;
IF OBJECT_ID('dbo.sp_LaySanPhamTheoId','P') IS NOT NULL DROP PROC dbo.sp_LaySanPhamTheoId;
IF OBJECT_ID('dbo.sp_LaySanPhamPhanTrang','P') IS NOT NULL DROP PROC dbo.sp_LaySanPhamPhanTrang;
IF OBJECT_ID('dbo.sp_XacThucKhachHang','P') IS NOT NULL DROP PROC dbo.sp_XacThucKhachHang;
IF OBJECT_ID('dbo.sp_XacThucNguoiDung','P') IS NOT NULL DROP PROC dbo.sp_XacThucNguoiDung;
IF OBJECT_ID('dbo.vw_DoanhSoTheoThang','V') IS NOT NULL DROP VIEW dbo.vw_DoanhSoTheoThang;
IF OBJECT_ID('dbo.vw_DoanhSoTheoSanPham','V') IS NOT NULL DROP VIEW dbo.vw_DoanhSoTheoSanPham;
IF OBJECT_ID('dbo.vw_TonKhoSanPham','V') IS NOT NULL DROP VIEW dbo.vw_TonKhoSanPham;
-- Drop tables if exist
IF OBJECT_ID('dbo.CauHinhDienThoai','U') IS NOT NULL DROP TABLE dbo.CauHinhDienThoai;
IF OBJECT_ID('dbo.CauHinhLaptop','U') IS NOT NULL DROP TABLE dbo.CauHinhLaptop;
IF OBJECT_ID('dbo.ChiTietDonHang','U') IS NOT NULL DROP TABLE dbo.ChiTietDonHang;
IF OBJECT_ID('dbo.DonHang','U') IS NOT NULL DROP TABLE dbo.DonHang;
IF OBJECT_ID('dbo.ChiTietGioHang','U') IS NOT NULL DROP TABLE dbo.ChiTietGioHang;
IF OBJECT_ID('dbo.GioHang','U') IS NOT NULL DROP TABLE dbo.GioHang;
IF OBJECT_ID('dbo.HinhAnhSanPham','U') IS NOT NULL DROP TABLE dbo.HinhAnhSanPham;
IF OBJECT_ID('dbo.SanPham','U') IS NOT NULL DROP TABLE dbo.SanPham;
IF OBJECT_ID('dbo.DanhMuc','U') IS NOT NULL DROP TABLE dbo.DanhMuc;
IF OBJECT_ID('dbo.KhachHang','U') IS NOT NULL DROP TABLE dbo.KhachHang;
IF OBJECT_ID('dbo.NguoiDung','U') IS NOT NULL DROP TABLE dbo.NguoiDung;
IF OBJECT_ID('dbo.VaiTro','U') IS NOT NULL DROP TABLE dbo.VaiTro;
IF OBJECT_ID('dbo.NhatKyHeThong','U') IS NOT NULL DROP TABLE dbo.NhatKyHeThong;
IF OBJECT_ID('dbo.ThuongHieu','U') IS NOT NULL DROP TABLE dbo.ThuongHieu;
GO
/***************************************
 * Tables (create) - ĐÃ CHUYỂN SANG TIẾNG VIỆT
 ***************************************/
-- VaiTro
CREATE TABLE dbo.VaiTro
(
    MaVaiTro INT IDENTITY(1,1) PRIMARY KEY,
    TenVaiTro NVARCHAR(50) NOT NULL UNIQUE,
    MoTa NVARCHAR(250) NULL
);
GO
-- NguoiDung
CREATE TABLE dbo.NguoiDung
(
    MaNguoiDung INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap NVARCHAR(100) NOT NULL UNIQUE,
    MatKhauHash VARBINARY(64) NOT NULL,
    HoTen NVARCHAR(150) NULL,
    Email NVARCHAR(150) NULL,
    MaVaiTro INT NOT NULL,
    DangHoatDong BIT NOT NULL DEFAULT 1,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_NguoiDung_VaiTro FOREIGN KEY(MaVaiTro) REFERENCES dbo.VaiTro(MaVaiTro)
);
GO
-- KhachHang
CREATE TABLE dbo.KhachHang
(
    MaKhachHang INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    MatKhauHash VARBINARY(64) NOT NULL,
    SoDienThoai NVARCHAR(30) NULL,
    DiaChi NVARCHAR(300) NULL,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    DangHoatDong BIT NOT NULL DEFAULT 1
);
GO
-- DanhMuc
CREATE TABLE dbo.DanhMuc
(
    MaDanhMuc INT IDENTITY(1,1) PRIMARY KEY,
    TenDanhMuc NVARCHAR(120) NOT NULL,
    MoTa NVARCHAR(500) NULL,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);
GO
-- ThuongHieu
CREATE TABLE dbo.ThuongHieu
(
    MaThuongHieu INT IDENTITY(1,1) PRIMARY KEY,
    TenThuongHieu NVARCHAR(100) NOT NULL UNIQUE,
    MoTa NVARCHAR(500) NULL
);
GO
-- SanPham
CREATE TABLE dbo.SanPham
(
    MaSanPham INT IDENTITY(1,1) PRIMARY KEY,
    MaDanhMuc INT NOT NULL,
    MaThuongHieu INT NOT NULL,
    SKU NVARCHAR(60) NOT NULL UNIQUE,
    TenSanPham NVARCHAR(250) NOT NULL,
    Gia DECIMAL(18,2) NOT NULL DEFAULT 0,
    GiaCu DECIMAL(18,2) NULL,
    SoLuongTon INT NOT NULL DEFAULT 0,
    HinhAnhMacDinh NVARCHAR(300) NULL,
    MoTaNgan NVARCHAR(1000) NULL,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    DangHoatDong BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_SanPham_DanhMuc FOREIGN KEY(MaDanhMuc) REFERENCES dbo.DanhMuc(MaDanhMuc),
    CONSTRAINT FK_SanPham_ThuongHieu FOREIGN KEY(MaThuongHieu) REFERENCES dbo.ThuongHieu(MaThuongHieu)
);
GO
CREATE INDEX IX_SanPham_MaThuongHieu ON dbo.SanPham(MaThuongHieu);
CREATE INDEX IX_SanPham_Gia ON dbo.SanPham(Gia);
GO
-- HinhAnhSanPham
CREATE TABLE dbo.HinhAnhSanPham
(
    MaHinhAnh INT IDENTITY(1,1) PRIMARY KEY,
    MaSanPham INT NOT NULL,
    DuongDanHinhAnh NVARCHAR(300) NOT NULL,
    LaHinhChinh BIT NOT NULL DEFAULT 0,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_HinhAnhSanPham_SanPham FOREIGN KEY(MaSanPham) REFERENCES dbo.SanPham(MaSanPham) ON DELETE CASCADE
);
GO
-- GioHang
CREATE TABLE dbo.GioHang
(
    MaGioHang INT IDENTITY(1,1) PRIMARY KEY,
    MaKhachHang INT NOT NULL,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    NgayCapNhat DATETIME2 NULL,
    CONSTRAINT FK_GioHang_KhachHang FOREIGN KEY(MaKhachHang) REFERENCES dbo.KhachHang(MaKhachHang)
);
GO
-- ChiTietGioHang
CREATE TABLE dbo.ChiTietGioHang
(
    MaChiTietGioHang INT IDENTITY(1,1) PRIMARY KEY,
    MaGioHang INT NOT NULL,
    MaSanPham INT NOT NULL,
    SoLuong INT NOT NULL DEFAULT 1,
    DonGia DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_ChiTietGioHang_GioHang FOREIGN KEY(MaGioHang) REFERENCES dbo.GioHang(MaGioHang) ON DELETE CASCADE,
    CONSTRAINT FK_ChiTietGioHang_SanPham FOREIGN KEY(MaSanPham) REFERENCES dbo.SanPham(MaSanPham)
);
GO
-- DonHang
CREATE TABLE dbo.DonHang
(
    MaDonHang INT IDENTITY(1,1) PRIMARY KEY,
    MaKhachHang INT NOT NULL,
    NgayDatHang DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    TongTien DECIMAL(18,2) NOT NULL DEFAULT 0,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT N'Đang xử lý',
    DiaChiGiaoHang NVARCHAR(300) NULL,
    MaNguoiTao INT NULL,
    CONSTRAINT FK_DonHang_KhachHang FOREIGN KEY(MaKhachHang) REFERENCES dbo.KhachHang(MaKhachHang),
    CONSTRAINT FK_DonHang_NguoiDung FOREIGN KEY(MaNguoiTao) REFERENCES dbo.NguoiDung(MaNguoiDung)
);
GO
-- ChiTietDonHang
CREATE TABLE dbo.ChiTietDonHang
(
    MaChiTietDonHang INT IDENTITY(1,1) PRIMARY KEY,
    MaDonHang INT NOT NULL,
    MaSanPham INT NOT NULL,
    SoLuong INT NOT NULL,
    DonGia DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_ChiTietDonHang_DonHang FOREIGN KEY(MaDonHang) REFERENCES dbo.DonHang(MaDonHang) ON DELETE CASCADE,
    CONSTRAINT FK_ChiTietDonHang_SanPham FOREIGN KEY(MaSanPham) REFERENCES dbo.SanPham(MaSanPham)
);
GO
-- NhatKyHeThong
CREATE TABLE dbo.NhatKyHeThong
(
    MaNhatKy INT IDENTITY(1,1) PRIMARY KEY,
    ThoiGian DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    TenDangNhap NVARCHAR(150) NULL,
    HanhDong NVARCHAR(250) NULL,
    ChiTiet NVARCHAR(MAX) NULL
);
GO
/***************************************
 * Bảng cấu hình Laptop
 ***************************************/
CREATE TABLE dbo.CauHinhLaptop
(
    ID_CAUHINH INT IDENTITY(1,1) PRIMARY KEY,
    MaSanPham INT NOT NULL, -- FK tới SanPham
    CPU NVARCHAR(100) NULL,
    SoNhan NVARCHAR(50) NULL,
    SoLuongLuongo NVARCHAR(50) NULL,
    RAM NVARCHAR(50) NULL,
    OCung NVARCHAR(100) NULL,
    CardDoHoa NVARCHAR(100) NULL,
    Pin NVARCHAR(50) NULL,
    OS NVARCHAR(50) NULL,
    KichThuocManHinh NVARCHAR(50) NULL,
    CongNgheManHinh NVARCHAR(50) NULL,
    DoPhanGiai NVARCHAR(50) NULL,
    CongGiaoTiep NVARCHAR(200) NULL,
    MauSac NVARCHAR(50) NULL,
    TrongLuong NVARCHAR(50) NULL,
    CONSTRAINT FK_CauHinhLaptop_SanPham FOREIGN KEY(MaSanPham) REFERENCES dbo.SanPham(MaSanPham)
);
GO
/***************************************
 * Bảng cấu hình Điện thoại
 ***************************************/
CREATE TABLE dbo.CauHinhDienThoai
(
    ID_CAUHINH INT IDENTITY(1,1) PRIMARY KEY,
    MaSanPham INT NOT NULL, -- FK tới SanPham
    CPU NVARCHAR(100) NULL,
    SoNhan NVARCHAR(50) NULL,
    SoLuongLuongo NVARCHAR(50) NULL,
    RAM NVARCHAR(50) NULL,
    BoNhoTrong NVARCHAR(50) NULL,
    Pin NVARCHAR(50) NULL,
    HeDieuHanh NVARCHAR(50) NULL,
    ManHinh NVARCHAR(50) NULL,
    CongNgheManHinh NVARCHAR(50) NULL,
    DoPhanGiai NVARCHAR(50) NULL,
    Camera NVARCHAR(200) NULL,
    CongGiaoTiep NVARCHAR(200) NULL,
    MauSac NVARCHAR(50) NULL,
    CONSTRAINT FK_CauHinhDienThoai_SanPham FOREIGN KEY(MaSanPham) REFERENCES dbo.SanPham(MaSanPham)
);
GO
/**********************************************
 * BỔ SUNG HỆ THỐNG QUẢN LÝ KHO
 **********************************************/
-- Thêm các bảng quản lý kho
IF OBJECT_ID('dbo.PhieuXuat','U') IS NOT NULL DROP TABLE dbo.PhieuXuat;
IF OBJECT_ID('dbo.ChiTietPhieuXuat','U') IS NOT NULL DROP TABLE dbo.ChiTietPhieuXuat;
IF OBJECT_ID('dbo.PhieuNhap','U') IS NOT NULL DROP TABLE dbo.PhieuNhap;
IF OBJECT_ID('dbo.ChiTietPhieuNhap','U') IS NOT NULL DROP TABLE dbo.ChiTietPhieuNhap;
IF OBJECT_ID('dbo.Kho','U') IS NOT NULL DROP TABLE dbo.Kho;
IF OBJECT_ID('dbo.NhaCungCap','U') IS NOT NULL DROP TABLE dbo.NhaCungCap;
GO
/***************************************
 * Bảng Nhà cung cấp
 ***************************************/
CREATE TABLE dbo.NhaCungCap
(
    MaNhaCungCap INT IDENTITY(1,1) PRIMARY KEY,
    TenNhaCungCap NVARCHAR(150) NOT NULL,
    SoDienThoai NVARCHAR(20) NULL,
    Email NVARCHAR(150) NULL,
    DiaChi NVARCHAR(300) NULL,
    GhiChu NVARCHAR(500) NULL,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    DangHoatDong BIT NOT NULL DEFAULT 1
);
GO
/***************************************
 * Bảng Kho (chi tiết tồn kho theo sản phẩm)
 ***************************************/
CREATE TABLE dbo.Kho
(
    MaKho INT IDENTITY(1,1) PRIMARY KEY,
    MaSanPham INT NOT NULL,
    SoLuongTon INT NOT NULL DEFAULT 0,
    SoLuongKhaDung INT NOT NULL DEFAULT 0, -- Số lượng có thể bán
    SoLuongChoNhap INT NOT NULL DEFAULT 0, -- Số lượng đang chờ nhập
    SoLuongChoXuat INT NOT NULL DEFAULT 0, -- Số lượng đang chờ xuất
    NgayCapNhat DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Kho_SanPham FOREIGN KEY(MaSanPham) REFERENCES dbo.SanPham(MaSanPham),
    CONSTRAINT UQ_Kho_SanPham UNIQUE(MaSanPham)
);
GO
/***************************************
 * Bảng Phiếu nhập
 ***************************************/
CREATE TABLE dbo.PhieuNhap
(
    MaPhieuNhap INT IDENTITY(1,1) PRIMARY KEY,
    SoPhieuNhap NVARCHAR(50) NOT NULL UNIQUE, -- Số phiếu tự sinh: PN20250001
    MaNhaCungCap INT NULL,
    NgayNhap DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    TongTien DECIMAL(18,2) NOT NULL DEFAULT 0,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT N'Đã nhập', -- Đã nhập, Đang chờ, Đã hủy
    GhiChu NVARCHAR(500) NULL,
    MaNguoiTao INT NOT NULL,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_PhieuNhap_NhaCungCap FOREIGN KEY(MaNhaCungCap) REFERENCES dbo.NhaCungCap(MaNhaCungCap),
    CONSTRAINT FK_PhieuNhap_NguoiDung FOREIGN KEY(MaNguoiTao) REFERENCES dbo.NguoiDung(MaNguoiDung)
);
GO
/***************************************
 * Bảng Chi tiết phiếu nhập
 ***************************************/
CREATE TABLE dbo.ChiTietPhieuNhap
(
    MaChiTietPhieuNhap INT IDENTITY(1,1) PRIMARY KEY,
    MaPhieuNhap INT NOT NULL,
    MaSanPham INT NOT NULL,
    SoLuong INT NOT NULL,
    DonGiaNhap DECIMAL(18,2) NOT NULL, -- Giá nhập từ nhà cung cấp
    ThanhTien DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_ChiTietPhieuNhap_PhieuNhap FOREIGN KEY(MaPhieuNhap) REFERENCES dbo.PhieuNhap(MaPhieuNhap) ON DELETE CASCADE,
    CONSTRAINT FK_ChiTietPhieuNhap_SanPham FOREIGN KEY(MaSanPham) REFERENCES dbo.SanPham(MaSanPham)
);
GO
/***************************************
 * Bảng Phiếu xuất
 ***************************************/
CREATE TABLE dbo.PhieuXuat
(
    MaPhieuXuat INT IDENTITY(1,1) PRIMARY KEY,
    SoPhieuXuat NVARCHAR(50) NOT NULL UNIQUE, -- Số phiếu tự sinh: PX20250001
    LoaiPhieuXuat NVARCHAR(50) NOT NULL DEFAULT N'Bán hàng', -- Bán hàng, Bảo hành, Khác
    MaDonHang INT NULL, -- Liên kết với đơn hàng (nếu có)
    NgayXuat DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    TongTien DECIMAL(18,2) NOT NULL DEFAULT 0,
    TrangThai NVARCHAR(50) NOT NULL DEFAULT N'Đã xuất', -- Đã xuất, Đang chờ, Đã hủy
    GhiChu NVARCHAR(500) NULL,
    MaNguoiTao INT NOT NULL,
    NgayTao DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_PhieuXuat_DonHang FOREIGN KEY(MaDonHang) REFERENCES dbo.DonHang(MaDonHang),
    CONSTRAINT FK_PhieuXuat_NguoiDung FOREIGN KEY(MaNguoiTao) REFERENCES dbo.NguoiDung(MaNguoiDung)
);
GO
/***************************************
 * Bảng Chi tiết phiếu xuất
 ***************************************/
CREATE TABLE dbo.ChiTietPhieuXuat
(
    MaChiTietPhieuXuat INT IDENTITY(1,1) PRIMARY KEY,
    MaPhieuXuat INT NOT NULL,
    MaSanPham INT NOT NULL,
    SoLuong INT NOT NULL,
    DonGiaXuat DECIMAL(18,2) NOT NULL, -- Giá bán tại thời điểm xuất
    ThanhTien DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_ChiTietPhieuXuat_PhieuXuat FOREIGN KEY(MaPhieuXuat) REFERENCES dbo.PhieuXuat(MaPhieuXuat) ON DELETE CASCADE,
    CONSTRAINT FK_ChiTietPhieuXuat_SanPham FOREIGN KEY(MaSanPham) REFERENCES dbo.SanPham(MaSanPham)
);
GO
/***************************************
 * Tạo indexes cho các bảng mới
 ***************************************/
CREATE INDEX IX_PhieuNhap_NgayNhap ON dbo.PhieuNhap(NgayNhap);
CREATE INDEX IX_PhieuXuat_NgayXuat ON dbo.PhieuXuat(NgayXuat);
CREATE INDEX IX_ChiTietPhieuNhap_MaSanPham ON dbo.ChiTietPhieuNhap(MaSanPham);
CREATE INDEX IX_ChiTietPhieuXuat_MaSanPham ON dbo.ChiTietPhieuXuat(MaSanPham);
GO
/***************************************
 * Tạo Table Types cho table-valued parameters
 ***************************************/
IF TYPE_ID('dbo.ChiTietNhapType') IS NOT NULL DROP TYPE dbo.ChiTietNhapType;
IF TYPE_ID('dbo.ChiTietXuatType') IS NOT NULL DROP TYPE dbo.ChiTietXuatType;
GO
CREATE TYPE dbo.ChiTietNhapType AS TABLE
(
    MaSanPham INT,
    SoLuong INT,
    DonGiaNhap DECIMAL(18,2)
);
GO
CREATE TYPE dbo.ChiTietXuatType AS TABLE
(
    MaSanPham INT,
    SoLuong INT,
    DonGiaXuat DECIMAL(18,2)
);
GO
/***************************************
 * Stored Procedures & Functions - ĐÃ CHUYỂN SANG TIẾNG VIỆT
 ***************************************/
-- Xác thực người dùng (tên đăng nhập)
CREATE PROCEDURE dbo.sp_XacThucNguoiDung
    @tenDangNhap NVARCHAR(100),
    @matKhauVanBan NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @h VARBINARY(64) = HASHBYTES('SHA2_256', @matKhauVanBan);
    SELECT u.MaNguoiDung, u.TenDangNhap, u.HoTen, u.Email, u.MaVaiTro, r.TenVaiTro
    FROM dbo.NguoiDung u
    INNER JOIN dbo.VaiTro r ON u.MaVaiTro = r.MaVaiTro
    WHERE u.TenDangNhap = @tenDangNhap AND u.MatKhauHash = @h AND u.DangHoatDong = 1;
END
GO
-- Xác thực khách hàng (email)
CREATE PROCEDURE dbo.sp_XacThucKhachHang
    @email NVARCHAR(150),
    @matKhauVanBan NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @h VARBINARY(64) = HASHBYTES('SHA2_256', @matKhauVanBan);
    SELECT c.MaKhachHang, c.HoTen, c.Email
    FROM dbo.KhachHang c
    WHERE c.Email = @email AND c.MatKhauHash = @h AND c.DangHoatDong = 1;
END
GO
-- Lấy sản phẩm phân trang
CREATE PROCEDURE dbo.sp_LaySanPhamPhanTrang
    @MaDanhMuc INT = NULL,
    @TimKiem NVARCHAR(250) = NULL,
    @MaThuongHieu INT = NULL,
    @GiaThapNhat DECIMAL(18,2) = NULL,
    @GiaCaoNhat DECIMAL(18,2) = NULL,
    @TrangSo INT = 1,
    @KichThuocTrang INT = 12
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @Offset INT = (@TrangSo - 1) * @KichThuocTrang;
    SELECT p.MaSanPham, p.SKU, p.TenSanPham, t.TenThuongHieu, p.Gia, p.GiaCu, p.SoLuongTon, p.HinhAnhMacDinh, p.MoTaNgan,
           d.TenDanhMuc
    FROM dbo.SanPham p
    INNER JOIN dbo.DanhMuc d ON p.MaDanhMuc = d.MaDanhMuc
    INNER JOIN dbo.ThuongHieu t ON p.MaThuongHieu = t.MaThuongHieu
    WHERE (@MaDanhMuc IS NULL OR p.MaDanhMuc = @MaDanhMuc)
      AND (@MaThuongHieu IS NULL OR p.MaThuongHieu = @MaThuongHieu)
      AND (@TimKiem IS NULL OR p.TenSanPham LIKE '%' + @TimKiem + '%')
      AND (@GiaThapNhat IS NULL OR p.Gia >= @GiaThapNhat)
      AND (@GiaCaoNhat IS NULL OR p.Gia <= @GiaCaoNhat)
    ORDER BY p.NgayTao DESC
    OFFSET @Offset ROWS FETCH NEXT @KichThuocTrang ROWS ONLY;
END
GO
-- Lấy sản phẩm theo id (bao gồm danh mục và thương hiệu)
CREATE PROCEDURE dbo.sp_LaySanPhamTheoId
    @MaSanPham INT
AS
BEGIN
    SET NOCOUNT ON;
    SELECT p.*, d.TenDanhMuc, t.TenThuongHieu
    FROM dbo.SanPham p
    INNER JOIN dbo.DanhMuc d ON p.MaDanhMuc = d.MaDanhMuc
    INNER JOIN dbo.ThuongHieu t ON p.MaThuongHieu = t.MaThuongHieu
    WHERE p.MaSanPham = @MaSanPham;
END
GO
-- Thêm sản phẩm
CREATE PROCEDURE dbo.sp_ThemSanPham
    @MaDanhMuc INT,
    @MaThuongHieu INT,
    @SKU NVARCHAR(60),
    @TenSanPham NVARCHAR(250),
    @Gia DECIMAL(18,2),
    @GiaCu DECIMAL(18,2) = NULL,
    @SoLuongTon INT = 0,
    @HinhAnhMacDinh NVARCHAR(300) = NULL,
    @MoTaNgan NVARCHAR(1000) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    -- Kiểm tra SKU trùng lặp
    IF EXISTS (SELECT 1 FROM dbo.SanPham WHERE SKU = @SKU)
    BEGIN
        DECLARE @ErrorMessage NVARCHAR(4000) = N'SKU đã tồn tại: ' + @SKU;
        RAISERROR(@ErrorMessage, 16, 1);
        RETURN;
    END
    INSERT INTO dbo.SanPham (MaDanhMuc, MaThuongHieu, SKU, TenSanPham, Gia, GiaCu, SoLuongTon, HinhAnhMacDinh, MoTaNgan)
    VALUES (@MaDanhMuc, @MaThuongHieu, @SKU, @TenSanPham, @Gia, @GiaCu, @SoLuongTon, @HinhAnhMacDinh, @MoTaNgan);
    SELECT SCOPE_IDENTITY() AS MaSanPhamMoi;
END
GO
-- Cập nhật sản phẩm
CREATE PROCEDURE dbo.sp_CapNhatSanPham
    @MaSanPham INT,
    @MaDanhMuc INT,
    @MaThuongHieu INT,
    @SKU NVARCHAR(60),
    @TenSanPham NVARCHAR(250),
    @Gia DECIMAL(18,2),
    @GiaCu DECIMAL(18,2) = NULL,
    @SoLuongTon INT = 0,
    @HinhAnhMacDinh NVARCHAR(300) = NULL,
    @MoTaNgan NVARCHAR(1000) = NULL,
    @DangHoatDong BIT = 1
AS
BEGIN
    SET NOCOUNT ON;
    UPDATE dbo.SanPham SET
        MaDanhMuc = @MaDanhMuc,
        MaThuongHieu = @MaThuongHieu,
        SKU = @SKU,
        TenSanPham = @TenSanPham,
        Gia = @Gia,
        GiaCu = @GiaCu,
        SoLuongTon = @SoLuongTon,
        HinhAnhMacDinh = @HinhAnhMacDinh,
        MoTaNgan = @MoTaNgan,
        DangHoatDong = @DangHoatDong
    WHERE MaSanPham = @MaSanPham;
END
GO
-- Xóa sản phẩm
CREATE PROCEDURE dbo.sp_XoaSanPham
    @MaSanPham INT
AS
BEGIN
    SET NOCOUNT ON;
    DELETE FROM dbo.HinhAnhSanPham WHERE MaSanPham = @MaSanPham;
    DELETE FROM dbo.SanPham WHERE MaSanPham = @MaSanPham;
END
GO
-- Hàm: tính tổng đơn hàng
CREATE FUNCTION dbo.fn_TinhTongDonHang(@MaDonHang INT)
RETURNS DECIMAL(18,2)
AS
BEGIN
    DECLARE @tong DECIMAL(18,2) = 0;
    SELECT @tong = SUM(DonGia * SoLuong) FROM dbo.ChiTietDonHang WHERE MaDonHang = @MaDonHang;
    RETURN ISNULL(@tong,0);
END
GO
/***************************************
 * Tạo procedures cho quản lý kho (với RAISERROR)
 ***************************************/
-- Tạo phiếu nhập
CREATE PROCEDURE dbo.sp_TaoPhieuNhap
    @MaNhaCungCap INT = NULL,
    @ChiTietNhap dbo.ChiTietNhapType READONLY,
    @GhiChu NVARCHAR(500) = NULL,
    @MaNguoiTao INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000);
    -- Kiểm tra dữ liệu đầu vào
    IF NOT EXISTS (SELECT 1 FROM @ChiTietNhap)
    BEGIN
        SET @ErrorMessage = N'Chi tiết phiếu nhập không được để trống';
        RAISERROR(@ErrorMessage, 16, 1);
        RETURN;
    END
    BEGIN TRY
        BEGIN TRANSACTION;
        -- Tạo số phiếu nhập tự động
        DECLARE @SoPhieuNhap NVARCHAR(50);
        DECLARE @NamHienTai NVARCHAR(4) = CONVERT(NVARCHAR(4), YEAR(GETDATE()));
        DECLARE @MaxSoPhieu INT;
        SELECT @MaxSoPhieu = ISNULL(MAX(CAST(RIGHT(SoPhieuNhap, 4) AS INT)), 0)
        FROM dbo.PhieuNhap
        WHERE SoPhieuNhap LIKE 'PN' + @NamHienTai + '%';
        SET @SoPhieuNhap = 'PN' + @NamHienTai + RIGHT('0000' + CAST(@MaxSoPhieu + 1 AS NVARCHAR(4)), 4);
        -- Tính tổng tiền
        DECLARE @TongTien DECIMAL(18,2) = 0;
        SELECT @TongTien = SUM(SoLuong * DonGiaNhap) FROM @ChiTietNhap;
        -- Tạo phiếu nhập
        INSERT INTO dbo.PhieuNhap (SoPhieuNhap, MaNhaCungCap, TongTien, GhiChu, MaNguoiTao)
        VALUES (@SoPhieuNhap, @MaNhaCungCap, @TongTien, @GhiChu, @MaNguoiTao);
        DECLARE @MaPhieuNhap INT = SCOPE_IDENTITY();
        -- Thêm chi tiết phiếu nhập
        INSERT INTO dbo.ChiTietPhieuNhap (MaPhieuNhap, MaSanPham, SoLuong, DonGiaNhap, ThanhTien)
        SELECT @MaPhieuNhap, MaSanPham, SoLuong, DonGiaNhap, SoLuong * DonGiaNhap
        FROM @ChiTietNhap;
        -- Cập nhật tồn kho
        UPDATE k
        SET k.SoLuongTon = k.SoLuongTon + cn.SoLuong,
            k.SoLuongKhaDung = k.SoLuongKhaDung + cn.SoLuong,
            k.NgayCapNhat = GETDATE()
        FROM dbo.Kho k
        INNER JOIN @ChiTietNhap cn ON k.MaSanPham = cn.MaSanPham;
        -- Cập nhật số lượng tồn trong bảng sản phẩm
        UPDATE sp
        SET sp.SoLuongTon = sp.SoLuongTon + cn.SoLuong
        FROM dbo.SanPham sp
        INNER JOIN @ChiTietNhap cn ON sp.MaSanPham = cn.MaSanPham;
        COMMIT TRANSACTION;
        SELECT @MaPhieuNhap AS MaPhieuNhap, @SoPhieuNhap AS SoPhieuNhap;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @ErrorMessage = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO
-- Tạo phiếu xuất
CREATE PROCEDURE dbo.sp_TaoPhieuXuat
    @LoaiPhieuXuat NVARCHAR(50) = N'Bán hàng',
    @MaDonHang INT = NULL,
    @ChiTietXuat dbo.ChiTietXuatType READONLY,
    @GhiChu NVARCHAR(500) = NULL,
    @MaNguoiTao INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000);
    -- Kiểm tra dữ liệu đầu vào
    IF NOT EXISTS (SELECT 1 FROM @ChiTietXuat)
    BEGIN
        SET @ErrorMessage = N'Chi tiết phiếu xuất không được để trống';
        RAISERROR(@ErrorMessage, 16, 1);
        RETURN;
    END
    -- Kiểm tra tồn kho
    IF EXISTS (
        SELECT 1 FROM @ChiTietXuat cx
        INNER JOIN dbo.Kho k ON cx.MaSanPham = k.MaSanPham
        WHERE cx.SoLuong > k.SoLuongKhaDung
    )
    BEGIN
        SET @ErrorMessage = N'Không đủ tồn kho cho một hoặc nhiều sản phẩm';
        RAISERROR(@ErrorMessage, 16, 1);
        RETURN;
    END
    BEGIN TRY
        BEGIN TRANSACTION;
        -- Tạo số phiếu xuất tự động
        DECLARE @SoPhieuXuat NVARCHAR(50);
        DECLARE @NamHienTai NVARCHAR(4) = CONVERT(NVARCHAR(4), YEAR(GETDATE()));
        DECLARE @MaxSoPhieu INT;
        SELECT @MaxSoPhieu = ISNULL(MAX(CAST(RIGHT(SoPhieuXuat, 4) AS INT)), 0)
        FROM dbo.PhieuXuat
        WHERE SoPhieuXuat LIKE 'PX' + @NamHienTai + '%';
        SET @SoPhieuXuat = 'PX' + @NamHienTai + RIGHT('0000' + CAST(@MaxSoPhieu + 1 AS NVARCHAR(4)), 4);
        -- Tính tổng tiền
        DECLARE @TongTien DECIMAL(18,2) = 0;
        SELECT @TongTien = SUM(SoLuong * DonGiaXuat) FROM @ChiTietXuat;
        -- Tạo phiếu xuất
        INSERT INTO dbo.PhieuXuat (SoPhieuXuat, LoaiPhieuXuat, MaDonHang, TongTien, GhiChu, MaNguoiTao)
        VALUES (@SoPhieuXuat, @LoaiPhieuXuat, @MaDonHang, @TongTien, @GhiChu, @MaNguoiTao);
        DECLARE @MaPhieuXuat INT = SCOPE_IDENTITY();
        -- Thêm chi tiết phiếu xuất
        INSERT INTO dbo.ChiTietPhieuXuat (MaPhieuXuat, MaSanPham, SoLuong, DonGiaXuat, ThanhTien)
        SELECT @MaPhieuXuat, MaSanPham, SoLuong, DonGiaXuat, SoLuong * DonGiaXuat
        FROM @ChiTietXuat;
        -- Cập nhật tồn kho
        UPDATE k
        SET k.SoLuongTon = k.SoLuongTon - cx.SoLuong,
            k.SoLuongKhaDung = k.SoLuongKhaDung - cx.SoLuong,
            k.NgayCapNhat = GETDATE()
        FROM dbo.Kho k
        INNER JOIN @ChiTietXuat cx ON k.MaSanPham = cx.MaSanPham;
        -- Cập nhật số lượng tồn trong bảng sản phẩm
        UPDATE sp
        SET sp.SoLuongTon = sp.SoLuongTon - cx.SoLuong
        FROM dbo.SanPham sp
        INNER JOIN @ChiTietXuat cx ON sp.MaSanPham = cx.MaSanPham;
        COMMIT TRANSACTION;
        SELECT @MaPhieuXuat AS MaPhieuXuat, @SoPhieuXuat AS SoPhieuXuat;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @ErrorMessage = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO
-- Tạo đơn hàng từ giỏ
CREATE PROCEDURE dbo.sp_TaoDonHangTuGio
    @MaGioHang INT,
    @DiaChiGiaoHang NVARCHAR(300),
    @MaNguoiTao INT = NULL
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @ErrorMessage NVARCHAR(4000);
    IF NOT EXISTS (SELECT 1 FROM dbo.GioHang WHERE MaGioHang = @MaGioHang)
    BEGIN
        SET @ErrorMessage = N'Không tìm thấy giỏ hàng';
        RAISERROR(@ErrorMessage, 16, 1);
        RETURN;
    END
    -- Kiểm tra tồn kho qua bảng Kho
    IF EXISTS (
        SELECT 1 FROM dbo.ChiTietGioHang cd
        JOIN dbo.Kho k ON cd.MaSanPham = k.MaSanPham
        WHERE cd.MaGioHang = @MaGioHang AND cd.SoLuong > k.SoLuongKhaDung
    )
    BEGIN
        SET @ErrorMessage = N'Không đủ tồn kho cho một hoặc nhiều sản phẩm';
        RAISERROR(@ErrorMessage, 16, 1);
        RETURN;
    END
    BEGIN TRY
        BEGIN TRANSACTION;
        DECLARE @tong DECIMAL(18,2) = 0;
        SELECT @tong = SUM(DonGia * SoLuong) FROM dbo.ChiTietGioHang WHERE MaGioHang = @MaGioHang;
        -- Tạo đơn hàng
        INSERT INTO dbo.DonHang (MaKhachHang, TongTien, DiaChiGiaoHang, TrangThai, MaNguoiTao)
        SELECT c.MaKhachHang, @tong, @DiaChiGiaoHang, N'Đang xử lý', @MaNguoiTao
        FROM dbo.GioHang c WHERE c.MaGioHang = @MaGioHang;
        DECLARE @maDonHang INT = SCOPE_IDENTITY();
        -- Thêm chi tiết đơn hàng
        INSERT INTO dbo.ChiTietDonHang (MaDonHang, MaSanPham, SoLuong, DonGia)
        SELECT @maDonHang, cd.MaSanPham, cd.SoLuong, cd.DonGia
        FROM dbo.ChiTietGioHang cd WHERE cd.MaGioHang = @MaGioHang;
        -- Tạo phiếu xuất tự động từ đơn hàng
        DECLARE @ChiTietXuat dbo.ChiTietXuatType;
        INSERT INTO @ChiTietXuat (MaSanPham, SoLuong, DonGiaXuat)
        SELECT MaSanPham, SoLuong, DonGia
        FROM dbo.ChiTietDonHang
        WHERE MaDonHang = @maDonHang;
        -- Gọi procedure tạo phiếu xuất
        DECLARE @MaNguoiXuat INT = ISNULL(@MaNguoiTao, 1);
        DECLARE @GhiChuXuat NVARCHAR(500) = N'Xuất kho tự động từ đơn hàng #' + CAST(@maDonHang AS NVARCHAR(10));
        EXEC dbo.sp_TaoPhieuXuat
            @LoaiPhieuXuat = N'Bán hàng',
            @MaDonHang = @maDonHang,
            @ChiTietXuat = @ChiTietXuat,
            @GhiChu = @GhiChuXuat,
            @MaNguoiTao = @MaNguoiXuat;
        -- Xóa giỏ hàng
        DELETE FROM dbo.ChiTietGioHang WHERE MaGioHang = @MaGioHang;
        COMMIT TRANSACTION;
        SELECT @maDonHang AS MaDonHangMoi;
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
        SET @ErrorMessage = ERROR_MESSAGE();
        RAISERROR(@ErrorMessage, 16, 1);
    END CATCH
END
GO
/***************************************
 * Triggers (kiểm tra tồn kho)
 ***************************************/
CREATE TRIGGER dbo.trg_ChiTietDonHang_Insert
ON dbo.ChiTietDonHang
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (
        SELECT 1 FROM inserted i
        JOIN dbo.SanPham p ON i.MaSanPham = p.MaSanPham
        WHERE i.SoLuong > p.SoLuongTon
    )
    BEGIN
        RAISERROR(N'Không đủ tồn kho cho sản phẩm khi chèn đơn hàng',16,1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
    UPDATE p
    SET p.SoLuongTon = p.SoLuongTon - i.SoLuong
    FROM dbo.SanPham p
    JOIN inserted i ON p.MaSanPham = i.MaSanPham;
END
GO
-- Trigger: Đồng bộ khi thêm sản phẩm mới
CREATE TRIGGER dbo.trg_SanPham_Insert
ON dbo.SanPham
AFTER INSERT
AS
BEGIN
    SET NOCOUNT ON;
    INSERT INTO dbo.Kho (MaSanPham, SoLuongTon, SoLuongKhaDung)
    SELECT i.MaSanPham, i.SoLuongTon, i.SoLuongTon
    FROM inserted i;
END
GO
/***************************************
 * Views for reports - ĐÃ CHUYỂN SANG TIẾNG VIỆT
 ***************************************/
CREATE VIEW dbo.vw_TonKhoSanPham
AS
SELECT p.MaSanPham, p.SKU, p.TenSanPham, t.TenThuongHieu, p.SoLuongTon, p.Gia, p.HinhAnhMacDinh, d.TenDanhMuc
FROM dbo.SanPham p
JOIN dbo.DanhMuc d ON p.MaDanhMuc = d.MaDanhMuc
JOIN dbo.ThuongHieu t ON p.MaThuongHieu = t.MaThuongHieu;
GO
CREATE VIEW dbo.vw_DoanhSoTheoSanPham
AS
SELECT p.MaSanPham, p.TenSanPham, t.TenThuongHieu,
       SUM(od.SoLuong) AS TongSoLuong,
       SUM(od.SoLuong * od.DonGia) AS TongDoanhThu
FROM dbo.ChiTietDonHang od
JOIN dbo.SanPham p ON od.MaSanPham = p.MaSanPham
JOIN dbo.ThuongHieu t ON p.MaThuongHieu = t.MaThuongHieu
GROUP BY p.MaSanPham, p.TenSanPham, t.TenThuongHieu;
GO
CREATE VIEW dbo.vw_DoanhSoTheoThang
AS
SELECT
    YEAR(o.NgayDatHang) AS Nam,
    MONTH(o.NgayDatHang) AS Thang,
    COUNT(DISTINCT o.MaDonHang) AS SoDonHang,
    SUM(o.TongTien) AS DoanhThu
FROM dbo.DonHang o
GROUP BY YEAR(o.NgayDatHang), MONTH(o.NgayDatHang);
GO
-- Xóa views cũ nếu tồn tại
IF OBJECT_ID('dbo.vw_TonKhoChiTiet','V') IS NOT NULL DROP VIEW dbo.vw_TonKhoChiTiet;
IF OBJECT_ID('dbo.vw_LichSuNhapXuat','V') IS NOT NULL DROP VIEW dbo.vw_LichSuNhapXuat;
GO
-- View: Tồn kho chi tiết
CREATE VIEW dbo.vw_TonKhoChiTiet
AS
SELECT
    k.MaSanPham,
    p.TenSanPham,
    t.TenThuongHieu,
    d.TenDanhMuc,
    k.SoLuongTon,
    k.SoLuongKhaDung,
    k.SoLuongChoNhap,
    k.SoLuongChoXuat,
    p.Gia,
    k.NgayCapNhat
FROM dbo.Kho k
JOIN dbo.SanPham p ON k.MaSanPham = p.MaSanPham
JOIN dbo.ThuongHieu t ON p.MaThuongHieu = t.MaThuongHieu
JOIN dbo.DanhMuc d ON p.MaDanhMuc = d.MaDanhMuc;
GO
-- View: Lịch sử nhập xuất
CREATE VIEW dbo.vw_LichSuNhapXuat
AS
SELECT
    'Nhập' AS Loai,
    pn.SoPhieuNhap AS SoPhieu,
    pn.NgayNhap AS Ngay,
    ISNULL(ncc.TenNhaCungCap, N'Không xác định') AS TenNhaCungCap,
    p.TenSanPham,
    ctpn.SoLuong,
    ctpn.DonGiaNhap AS DonGia,
    ctpn.ThanhTien,
    u.HoTen AS NguoiTao
FROM dbo.ChiTietPhieuNhap ctpn
JOIN dbo.PhieuNhap pn ON ctpn.MaPhieuNhap = pn.MaPhieuNhap
JOIN dbo.SanPham p ON ctpn.MaSanPham = p.MaSanPham
LEFT JOIN dbo.NhaCungCap ncc ON pn.MaNhaCungCap = ncc.MaNhaCungCap
JOIN dbo.NguoiDung u ON pn.MaNguoiTao = u.MaNguoiDung
UNION ALL
SELECT
    'Xuất' AS Loai,
    px.SoPhieuXuat AS SoPhieu,
    px.NgayXuat AS Ngay,
    N'Khách hàng' AS TenNhaCungCap,
    p.TenSanPham,
    ctpx.SoLuong,
    ctpx.DonGiaXuat AS DonGia,
    ctpx.ThanhTien,
    u.HoTen AS NguoiTao
FROM dbo.ChiTietPhieuXuat ctpx
JOIN dbo.PhieuXuat px ON ctpx.MaPhieuXuat = px.MaPhieuXuat
JOIN dbo.SanPham p ON ctpx.MaSanPham = p.MaSanPham
JOIN dbo.NguoiDung u ON px.MaNguoiTao = u.MaNguoiDung;
GO
/***************************************
 * Seed data (roles/users/customers/categories/brands/products ~30) - ĐÃ CHUYỂN SANG TIẾNG VIỆT
 ***************************************/
 /***************************************
 * Seed data cho bảng mới
 ***************************************/
-- Thêm nhà cung cấp
INSERT INTO dbo.NhaCungCap (TenNhaCungCap, SoDienThoai, Email, DiaChi)
VALUES
(N'Công ty TNHH Apple Việt Nam', '02838227999', 'contact@apple.com.vn', N'Tòa nhà Viettel, 285 Cách Mạng Tháng 8, Q.10, TP.HCM'),
(N'Công ty Samsung Electronics Vina', '02838111999', 'contact@samsung.com.vn', N'Số 2, đường Hải Triều, P.Bến Nghé, Q.1, TP.HCM'),
(N'Công ty TNHH Xiaomi Vietnam', '02839351999', 'contact@xiaomi.com.vn', N'Tòa nhà Vincom Center, 72 Lê Thánh Tôn, Q.1, TP.HCM'),
(N'Công ty TNHH Dell Technologies Vietnam', '02839101999', 'contact@dell.com.vn', N'Tòa nhà Saigon Tower, 29 Lê Duẩn, Q.1, TP.HCM');
GO
-- VaiTro
INSERT INTO dbo.VaiTro(TenVaiTro, MoTa) VALUES
(N'Quản trị viên', N'Quản trị hệ thống'),
(N'Nhân viên', N'Nhân viên cửa hàng / quản lý sản phẩm'),
(N'Khách hàng', N'Người dùng cuối / người mua');
GO
-- NguoiDung (mật khẩu demo đã hash)
INSERT INTO dbo.NguoiDung(TenDangNhap, MatKhauHash, HoTen, Email, MaVaiTro)
VALUES
(N'admin', HASHBYTES('SHA2_256', 'admin123'), N'Quản trị viên', 'admin@phoneshop.local', 1),
(N'nhanvien1', HASHBYTES('SHA2_256', 'nhanvien123'), N'Nhân viên Một', 'nhanvien1@phoneshop.local', 2);
GO
-- KhachHang
INSERT INTO dbo.KhachHang(HoTen, Email, MatKhauHash, SoDienThoai, DiaChi)
VALUES
(N'Nguyễn Văn A','a.nguyen@example.com', HASHBYTES('SHA2_256','pass123'), '0912345678', N'HCM, Quận 1'),
(N'Trần Thị B','b.tran@example.com', HASHBYTES('SHA2_256','pass123'), '0987654321', N'Đà Nẵng'),
(N'Lê Văn C','c.le@example.com', HASHBYTES('SHA2_256','pass123'), '0901122334', N'Hà Nội');
GO
-- DanhMuc
INSERT INTO dbo.DanhMuc(TenDanhMuc, MoTa)
VALUES
(N'Điện thoại thông minh', N'Điện thoại thông minh: Android & iOS'),
(N'Máy tính xách tay', N'Laptop: Ultrabook, Gaming, Workstation'),
(N'TV', N'TV: LED, OLED, QLED, Smart TV'),
(N'Phụ kiện điện thoại', N'Phụ kiện điện thoại: ốp, cáp, sạc, tai nghe'),
(N'Phụ kiện máy tính', N'Phụ kiện máy tính: chuột, bàn phím, sạc, ổ cứng'),
(N'Âm thanh & Thiết bị đeo', N'Tai nghe, loa, smartwatch, earbuds');
GO
-- ThuongHieu
INSERT INTO dbo.ThuongHieu(TenThuongHieu, MoTa)
VALUES
(N'Apple', N'Thương hiệu công nghệ hàng đầu thế giới'),
(N'Samsung', N'Thương hiệu điện tử Hàn Quốc'),
(N'Google', N'Thương hiệu sản phẩm của Google'),
(N'Xiaomi', N'Thương hiệu điện thoại & thiết bị thông minh Trung Quốc'),
(N'OnePlus', N'Thương hiệu smartphone cao cấp'),
(N'Dell', N'Thương hiệu máy tính cá nhân Mỹ'),
(N'Razer', N'Thương hiệu thiết bị chơi game'),
(N'ASUS', N'Thương hiệu máy tính Đài Loan'),
(N'Lenovo', N'Thương hiệu máy tính Trung Quốc'),
(N'LG', N'Thương hiệu điện tử Hàn Quốc'),
(N'AccessoryBrand', N'Thương hiệu chung cho phụ kiện'),
(N'Logitech', N'Thương hiệu thiết bị ngoại vi'),
(N'KeyBrand', N'Thương hiệu bàn phím chung'),
(N'StorageBrand', N'Thương hiệu ổ cứng chung'),
(N'Sony', N'Thương hiệu điện tử Nhật Bản'),
(N'JBL', N'Thương hiệu âm thanh Mỹ'),
(N'AudioBrand', N'Thương hiệu thiết bị âm thanh chung');
GO
-- Lấy MaThuongHieu để sử dụng trong INSERT SanPham
DECLARE @AppleId INT = (SELECT MaThuongHieu FROM dbo.ThuongHieu WHERE TenThuongHieu = N'Apple');
DECLARE @SamsungId INT = (SELECT MaThuongHieu FROM dbo.ThuongHieu WHERE TenThuongHieu = N'Samsung');
DECLARE @GoogleId INT = (SELECT MaThuongHieu FROM dbo.ThuongHieu WHERE TenThuongHieu = N'Google');
DECLARE @XiaomiId INT = (SELECT MaThuongHieu FROM dbo.ThuongHieu WHERE TenThuongHieu = N'Xiaomi');
DECLARE @OnePlusId INT = (SELECT MaThuongHieu FROM dbo.ThuongHieu WHERE TenThuongHieu = N'OnePlus');
DECLARE @DellId INT = (SELECT MaThuongHieu FROM dbo.ThuongHieu WHERE TenThuongHieu = N'Dell');
DECLARE @RazerId INT = (SELECT MaThuongHieu FROM dbo.ThuongHieu WHERE TenThuongHieu = N'Razer');
DECLARE @ASUSId INT = (SELECT MaThuongHieu FROM dbo.ThuongHieu WHERE TenThuongHieu = N'ASUS');
DECLARE @LenovoId INT = (SELECT MaThuongHieu FROM dbo.ThuongHieu WHERE TenThuongHieu = N'Lenovo');
DECLARE @LGId INT = (SELECT MaThuongHieu FROM dbo.ThuongHieu WHERE TenThuongHieu = N'LG');
DECLARE @AccessoryBrandId INT = (SELECT MaThuongHieu FROM dbo.ThuongHieu WHERE TenThuongHieu = N'AccessoryBrand');
DECLARE @LogitechId INT = (SELECT MaThuongHieu FROM dbo.ThuongHieu WHERE TenThuongHieu = N'Logitech');
DECLARE @KeyBrandId INT = (SELECT MaThuongHieu FROM dbo.ThuongHieu WHERE TenThuongHieu = N'KeyBrand');
DECLARE @StorageBrandId INT = (SELECT MaThuongHieu FROM dbo.ThuongHieu WHERE TenThuongHieu = N'StorageBrand');
DECLARE @SonyId INT = (SELECT MaThuongHieu FROM dbo.ThuongHieu WHERE TenThuongHieu = N'Sony');
DECLARE @JBLId INT = (SELECT MaThuongHieu FROM dbo.ThuongHieu WHERE TenThuongHieu = N'JBL');
DECLARE @AudioBrandId INT = (SELECT MaThuongHieu FROM dbo.ThuongHieu WHERE TenThuongHieu = N'AudioBrand');
-- SanPham (~30 sản phẩm thực tế) - ĐÃ CHUYỂN SANG TIẾNG VIỆT
INSERT INTO dbo.SanPham (MaDanhMuc, MaThuongHieu, SKU, TenSanPham, Gia, GiaCu, SoLuongTon, HinhAnhMacDinh, MoTaNgan)
VALUES
-- Điện thoại thông minh (MaDanhMuc = 1)
(1,@AppleId,'IP16PM-001','iPhone 16 Pro Max',34990000,37990000,15,'/images/iphone16promax.jpg',N'Apple A18 Pro, camera 48MP, OLED'),
(1,@AppleId,'IP16-001','iPhone 16',25990000,28990000,20,'/images/iphone16.jpg',N'Apple A17, camera kép, MagSafe'),
(1,@SamsungId,'S24U-001','Samsung Galaxy S24 Ultra',32990000,35990000,12,'/images/s24ultra.jpg',N'Snapdragon/Exynos, camera 200MP'),
(1,@GoogleId,'PX9P-001','Google Pixel 9 Pro',25990000,27990000,9,'/images/pixel9pro.jpg',N'Tensor G4, camera AI'),
(1,@XiaomiId,'XIAO-14-U','Xiaomi 14 Ultra',22990000,24990000,10,'/images/xiaomi14ultra.jpg',N'Camera Leica, màn hình 6.73\" AMOLED'),
(1,@OnePlusId,'OP12-001','OnePlus 12',21990000,23990000,14,'/images/oneplus12.jpg',N'Snapdragon top, hệ điều hành mượt mà'),
(1,@SamsungId,'S24-001','Samsung Galaxy S24',19990000,21990000,18,'/images/s24.jpg',N'Flagship nhỏ gọn'),
-- Máy tính xách tay (MaDanhMuc = 2)
(2,@AppleId,'MBP-16-2025','MacBook Pro 16 (M4)',64990000,69990000,8,'/images/macbookpro16_m4.jpg',N'Apple M4, CPU lên đến 24 nhân'),
(2,@AppleId,'MB-13-2025','MacBook Air 13 (M4)',32990000,34990000,10,'/images/macbookair13_m4.jpg',N'M4, mỏng nhẹ'),
(2,@DellId,'XPS-15-2025','Dell XPS 15',42990000,46990000,10,'/images/dell_xps15.jpg',N'Intel i9, RAM lên đến 32GB'),
(2,@RazerId,'RZ-17G-2025','Razer Blade 17',54990000,57990000,5,'/images/razerblade17.jpg',N'Intel i9, NVIDIA RTX 40-series, chơi game'),
(2,@ASUSId,'AS-GF-15','Asus ROG Flow',38990000,41990000,7,'/images/asus_rog.jpg',N'Máy chơi game, tùy chọn RTX'),
(2,@LenovoId,'LENO-IDEA7','Lenovo IdeaPad 7',17990000,19990000,18,'/images/lenovo_ideapad7.jpg',N'Hiệu suất hiệu quả, Ryzen'),
-- TV (MaDanhMuc = 3)
(3,@LGId,'OLED55-2025','LG OLED 55\" C-Series',24990000,27990000,6,'/images/lg_oled55.jpg',N'OLED 4K, Smart TV'),
(3,@SamsungId,'QLED65-2025','Samsung QLED 65\"',31990000,34990000,4,'/images/samsung_qled65.jpg',N'QLED 4K'),
-- Phụ kiện điện thoại (MaDanhMuc = 4)
(4,@AccessoryBrandId,'CASE-001','Ốp lưng chống sốc (Universal)',299000,399000,120,'/images/case001.jpg',N'Ốp lưng bảo vệ cho nhiều dòng'),
(4,@AccessoryBrandId,'CHG-65W','Adapter sạc nhanh 65W',399000,499000,200,'/images/charger65w.jpg',N'Sạc PD 65W'),
(4,@AccessoryBrandId,'CABLE-USBC-1M','Cáp USB-C 1m',99000,129000,300,'/images/usb_cable.jpg',N'Cáp sạc & dữ liệu'),
(4,@AccessoryBrandId,'PROT-GLASS','Kính cường lực',99000,129000,200,'/images/screen_protector.jpg',N'Kính cường lực bảo vệ màn hình'),
-- Phụ kiện máy tính (MaDanhMuc = 5)
(5,@LogitechId,'MOUSE-G502','Logitech G502 HERO',1290000,1490000,60,'/images/logitech_g502.jpg',N'Chuột chơi game, DPI cao'),
(5,@KeyBrandId,'KB-MECH-01','Bàn phím cơ RGB',990000,1190000,50,'/images/keyboard_mech.jpg',N'Hot-swap, RGB'),
(5,@StorageBrandId,'SSD-1TB','SSD NVMe 1TB',2399000,2799000,40,'/images/ssd_1tb.jpg',N'Ổ cứng NVMe tốc độ cao'),
(5,@StorageBrandId,'EXT-HDD-2TB','HDD External 2TB',1999000,2299000,35,'/images/hdd_2tb.jpg',N'Ổ cứng di động'),
-- Âm thanh & Thiết bị đeo (MaDanhMuc = 6)
(6,@SonyId,'BH-ANC1','Sony WH-1000XM5',6790000,7290000,20,'/images/sony_wh1000xm5.jpg',N'Chống ồn chủ động, pin lâu'),
(6,@AppleId,'WATCH-5','Apple Watch Series 9',11990000,12990000,12,'/images/apple_watch9.jpg',N'Sức khỏe & thể dục'),
(6,@JBLId,'SPEAKER-1','JBL Flip 6',1999000,2299000,30,'/images/jbl_flip6.jpg',N'Loa Bluetooth chống nước'),
(6,@AudioBrandId,'EAR-SPORTS','Tai nghe thể thao',499000,699000,60,'/images/earbuds_sport.jpg',N'Chống mồ hôi');
GO
-- HinhAnhSanPham (đặt hình chính)
INSERT INTO dbo.HinhAnhSanPham (MaSanPham, DuongDanHinhAnh, LaHinhChinh)
SELECT MaSanPham, HinhAnhMacDinh, 1 FROM dbo.SanPham WHERE HinhAnhMacDinh IS NOT NULL;
GO
-- Tạo giỏ hàng cho hai khách hàng đầu tiên
INSERT INTO dbo.GioHang(MaKhachHang) VALUES (1), (2);
GO
-- Mẫu chi tiết giỏ hàng
INSERT INTO dbo.ChiTietGioHang (MaGioHang, MaSanPham, SoLuong, DonGia)
VALUES
(1, (SELECT TOP 1 MaSanPham FROM dbo.SanPham WHERE SKU='IP16PM-001'), 1, (SELECT Gia FROM dbo.SanPham WHERE SKU='IP16PM-001')),
(1, (SELECT TOP 1 MaSanPham FROM dbo.SanPham WHERE SKU='CABLE-USBC-1M'), 2, (SELECT Gia FROM dbo.SanPham WHERE SKU='CABLE-USBC-1M')),
(2, (SELECT TOP 1 MaSanPham FROM dbo.SanPham WHERE SKU='MBP-16-2025'), 1, (SELECT Gia FROM dbo.SanPham WHERE SKU='MBP-16-2025'));
GO
/***************************************
 * Seed configurations (dùng SKU -> MaSanPham)
 ***************************************/
-- Dữ liệu cấu hình giữ nguyên, tham chiếu đến MaSanPham
-- iPhone 16 Pro Max
INSERT INTO dbo.CauHinhDienThoai
(MaSanPham, CPU, SoNhan, SoLuongLuongo, RAM, BoNhoTrong, Pin, HeDieuHanh, ManHinh, CongNgheManHinh, DoPhanGiai, Camera, CongGiaoTiep, MauSac)
VALUES
((SELECT TOP 1 MaSanPham FROM dbo.SanPham WHERE SKU='IP16PM-001'),
 N'Apple A18 Pro', NULL, NULL, N'8GB',N'256GB',N'4500mAh',N'iOS 18',N'6.7 inch',N'XDR OLED',N'2796x1290',N'48MP + 12MP + 12MP',N'Lightning, 5G, WiFi6E',N'Titanium');
-- iPhone 16
INSERT INTO dbo.CauHinhDienThoai
(MaSanPham, CPU, RAM, BoNhoTrong, Pin, HeDieuHanh, ManHinh, CongNgheManHinh, DoPhanGiai, Camera, CongGiaoTiep, MauSac)
VALUES
((SELECT TOP 1 MaSanPham FROM dbo.SanPham WHERE SKU='IP16-001'),
 N'Apple A17', N'6GB',N'128GB',N'3800mAh',N'iOS 17',N'6.1 inch',N'OLED',N'2556x1179',N'48MP kép',N'Lightning, 5G',N'Đen');
-- Samsung Galaxy S24 Ultra
INSERT INTO dbo.CauHinhDienThoai
(MaSanPham, CPU, RAM, BoNhoTrong, Pin, HeDieuHanh, ManHinh, CongNgheManHinh, DoPhanGiai, Camera, CongGiaoTiep, MauSac)
VALUES
((SELECT TOP 1 MaSanPham FROM dbo.SanPham WHERE SKU='S24U-001'),
 N'Snapdragon 8 Gen 3',N'12GB',N'256GB',N'5000mAh',N'Android 14',N'6.8 inch',N'Dynamic AMOLED',N'3088x1440',N'200MP + 12MP + 10MP',N'USB-C, 5G, WiFi6E',N'Đen Phantom');
-- Google Pixel 9 Pro
INSERT INTO dbo.CauHinhDienThoai
(MaSanPham, CPU, RAM, BoNhoTrong, Pin, HeDieuHanh, ManHinh, CongNgheManHinh, DoPhanGiai, Camera, CongGiaoTiep, MauSac)
VALUES
((SELECT TOP 1 MaSanPham FROM dbo.SanPham WHERE SKU='PX9P-001'),
 N'Google Tensor G4',N'12GB',N'256GB',N'4900mAh',N'Android 14',N'6.7 inch',N'LTPO OLED',N'3120x1440',N'50MP + 48MP + 12MP',N'USB-C, 5G',N'Xanh biển');
-- Xiaomi 14 Ultra
INSERT INTO dbo.CauHinhDienThoai
(MaSanPham, CPU, RAM, BoNhoTrong, Pin, HeDieuHanh, ManHinh, CongNgheManHinh, DoPhanGiai, Camera, CongGiaoTiep, MauSac)
VALUES
((SELECT TOP 1 MaSanPham FROM dbo.SanPham WHERE SKU='XIAO-14-U'),
 N'Snapdragon 8 Gen 3',N'12GB',N'512GB',N'4900mAh',N'Android 14',N'6.73 inch',N'AMOLED',N'3200x1440',N'50MP (Leica)',N'USB-C, 5G',N'Trắng');
-- OnePlus 12
INSERT INTO dbo.CauHinhDienThoai
(MaSanPham, CPU, RAM, BoNhoTrong, Pin, HeDieuHanh, ManHinh, CongNgheManHinh, DoPhanGiai, Camera, CongGiaoTiep, MauSac)
VALUES
((SELECT TOP 1 MaSanPham FROM dbo.SanPham WHERE SKU='OP12-001'),
 N'Snapdragon 8 Gen 3',N'12GB',N'256GB',N'5000mAh',N'Android 14',N'6.82 inch',N'AMOLED',N'3168x1440',N'50MP + 48MP',N'USB-C, 5G',N'Đen');
-- Samsung S24 (compact)
INSERT INTO dbo.CauHinhDienThoai
(MaSanPham, CPU, RAM, BoNhoTrong, Pin, HeDieuHanh, ManHinh, CongNgheManHinh, DoPhanGiai, Camera, CongGiaoTiep, MauSac)
VALUES
((SELECT TOP 1 MaSanPham FROM dbo.SanPham WHERE SKU='S24-001'),
 N'Snapdragon 8 Gen 3',N'8GB',N'128GB',N'3900mAh',N'Android 14',N'6.2 inch',N'Dynamic AMOLED',N'2340x1080',N'50MP',N'USB-C, 5G',N'Kem');
-- MacBook Pro 16 (M4)
INSERT INTO dbo.CauHinhLaptop
(MaSanPham, CPU, SoNhan, SoLuongLuongo, RAM, OCung, CardDoHoa, Pin, OS, KichThuocManHinh, CongNgheManHinh, DoPhanGiai, CongGiaoTiep, MauSac, TrongLuong)
VALUES
((SELECT TOP 1 MaSanPham FROM dbo.SanPham WHERE SKU='MBP-16-2025'),
 N'Apple M4', NULL, NULL, N'16GB',N'1TB SSD',N'Integrated Apple GPU',N'99Wh',N'macOS',N'16 inch',N'Liquid Retina XDR',N'3456x2234',N'Thunderbolt 4, HDMI',N'Xám không gian',N'2.1 kg');
-- MacBook Air 13 (M4)
INSERT INTO dbo.CauHinhLaptop
(MaSanPham, CPU, RAM, OCung, CardDoHoa, Pin, OS, KichThuocManHinh, CongNgheManHinh, DoPhanGiai, CongGiaoTiep, MauSac, TrongLuong)
VALUES
((SELECT TOP 1 MaSanPham FROM dbo.SanPham WHERE SKU='MB-13-2025'),
 N'Apple M4',N'16GB',N'512GB SSD',N'Integrated Apple GPU',N'52Wh',N'macOS',N'13.6 inch',N'Liquid Retina',N'2560x1664',N'Thunderbolt 4',N'Bạc',N'1.24 kg');
-- Dell XPS 15
INSERT INTO dbo.CauHinhLaptop
(MaSanPham, CPU, SoNhan, SoLuongLuongo, RAM, OCung, CardDoHoa, Pin, OS, KichThuocManHinh, CongNgheManHinh, DoPhanGiai, CongGiaoTiep, MauSac, TrongLuong)
VALUES
((SELECT TOP 1 MaSanPham FROM dbo.SanPham WHERE SKU='XPS-15-2025'),
 N'Intel Core i9-13900HK',N'8',N'16',N'32GB',N'1TB SSD',N'NVIDIA RTX 4060 (mobile)',N'86Wh',N'Windows 11',N'15.6 inch',N'OLED / FHD options',N'2880x1800',N'USB-C, HDMI',N'Bạc',N'1.8 kg');
-- Razer Blade 17 (gaming)
INSERT INTO dbo.CauHinhLaptop
(MaSanPham, CPU, SoNhan, SoLuongLuongo, RAM, OCung, CardDoHoa, Pin, OS, KichThuocManHinh, CongNgheManHinh, DoPhanGiai, CongGiaoTiep, MauSac, TrongLuong)
VALUES
((SELECT TOP 1 MaSanPham FROM dbo.SanPham WHERE SKU='RZ-17G-2025'),
 N'Intel Core i9-13950HX',N'16',N'32',N'32GB',N'1TB SSD',N'NVIDIA RTX 4080 (mobile)',N'95Wh',N'Windows 11',N'17.3 inch',N'IPS 240Hz',N'2560x1600',N'USB-C, HDMI, Ethernet',N'Đen',N'2.75 kg');
-- Asus ROG Flow
INSERT INTO dbo.CauHinhLaptop
(MaSanPham, CPU, SoNhan, RAM, OCung, CardDoHoa, Pin, OS, KichThuocManHinh, CongNgheManHinh, DoPhanGiai, CongGiaoTiep, MauSac, TrongLuong)
VALUES
((SELECT TOP 1 MaSanPham FROM dbo.SanPham WHERE SKU='AS-GF-15'),
 N'AMD Ryzen 9 7945HS',N'16',N'32GB',N'1TB SSD',N'NVIDIA RTX 4070 (mobile)',N'90Wh',N'Windows 11',N'15.6 inch',N'IPS',N'2560x1440',N'USB-C, HDMI',N'Đen',N'1.9 kg');
-- Lenovo IdeaPad 7
INSERT INTO dbo.CauHinhLaptop
(MaSanPham, CPU, SoNhan, RAM, OCung, CardDoHoa, Pin, OS, KichThuocManHinh, CongNgheManHinh, DoPhanGiai, CongGiaoTiep, MauSac, TrongLuong)
VALUES
((SELECT TOP 1 MaSanPham FROM dbo.SanPham WHERE SKU='LENO-IDEA7'),
 N'AMD Ryzen 7 7840U',N'8',N'16GB',N'512GB SSD',N'Integrated',N'70Wh',N'Windows 11',N'15.6 inch',N'IPS',N'1920x1080',N'USB-C',N'Xám',N'1.7 kg');
-- Phụ kiện & khác: không cần cấu hình nhưng để bảng sẵn sàng
GO
PRINT N'ĐÃ SỬA LỖI THÀNH CÔNG! Tất cả THROW đã được thay bằng RAISERROR.';
PRINT N'Đã sửa các stored procedures:';
PRINT N'- sp_TaoPhieuNhap';
PRINT N'- sp_TaoPhieuXuat';
PRINT N'- sp_TaoDonHangTuGio';
PRINT N'- sp_ThemSanPham';
GO
/***************************************
 * Final checks
 ***************************************/
PRINT N'Thiết lập hoàn tất. Tóm tắt:';
SELECT
    (SELECT COUNT(*) FROM dbo.VaiTro) AS SoVaiTro,
    (SELECT COUNT(*) FROM dbo.NguoiDung) AS SoNguoiDung,
    (SELECT COUNT(*) FROM dbo.KhachHang) AS SoKhachHang,
    (SELECT COUNT(*) FROM dbo.DanhMuc) AS SoDanhMuc,
    (SELECT COUNT(*) FROM dbo.ThuongHieu) AS SoThuongHieu,
    (SELECT COUNT(*) FROM dbo.SanPham) AS SoSanPham,
    (SELECT COUNT(*) FROM dbo.GioHang) AS SoGioHang,
    (SELECT COUNT(*) FROM dbo.DonHang) AS SoDonHang;
GO
-- Ví dụ sử dụng:
-- EXEC dbo.sp_XacThucNguoiDung @tenDangNhap=N'admin', @matKhauVanBan=N'admin123';
-- EXEC dbo.sp_LaySanPhamPhanTrang @TrangSo=1, @KichThuocTrang=12, @MaThuongHieu=@AppleId;
-- EXEC dbo.sp_TaoDonHangTuGio @MaGioHang=1, @DiaChiGiaoHang=N'HCM, Quận 1';
GO


USE PhoneShopFull;
GO
SELECT ROUTINE_NAME AS TenThuTuc
FROM INFORMATION_SCHEMA.ROUTINES
WHERE ROUTINE_TYPE = 'PROCEDURE'
ORDER BY ROUTINE_NAME;
