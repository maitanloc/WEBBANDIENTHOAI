
## Thanh Toán VNPAY
##  Cấu trúc & Vai trò các file

###  Nhóm Cấu hình & Khởi tạo

* **appsettings.json**

  * Lưu thông tin cấu hình VNPay: `TmnCode`, `HashSecret`, `BaseUrl`, `PaymentBackReturnUrl`.
  * Cho phép thay đổi cấu hình mà không cần sửa mã nguồn.

* **Program.cs**

  * Đăng ký dịch vụ `IVnPayService` / `VnPayService`.
  * Hỗ trợ Dependency Injection để Controller sử dụng dịch vụ VNPay.

---

###  Nhóm Dữ liệu (Models)

* **PaymentInformationModel.cs**

  * Đóng gói dữ liệu gửi sang VNPay.
  * Gồm: Số tiền, Mã đơn hàng, Tên khách hàng, Nội dung thanh toán.

* **PaymentResponseModel.cs**

  * Chuẩn hóa dữ liệu VNPay trả về.
  * Gồm: Trạng thái giao dịch, Mã phản hồi, Mã giao dịch VNPay.

---

### Nhóm Xử lý Logic

* **VnPayLibrary.cs**

  * Xây dựng URL thanh toán theo chuẩn VNPay.
  * Sắp xếp và mã hóa tham số.
  * Tạo và kiểm tra chữ ký bảo mật (HMACSHA512).

* **VnPayService.cs**

  * Lấy cấu hình từ `appsettings.json`.
  * Sử dụng `VnPayLibrary` để tạo link thanh toán.
  * Xác thực dữ liệu callback và ánh xạ sang `PaymentResponseModel`.

---

###  Nhóm Điều phối (Controller)

* **ThanhToanController.cs**

  * Tạo đơn hàng (trạng thái chờ thanh toán).
  * Chuyển hướng người dùng sang cổng VNPay.
  * Nhận kết quả trả về và cập nhật trạng thái đơn hàng trong Database.

* **PaymentController.cs**

  * File dùng cho test hoặc mã cũ.
  * Không sử dụng trong luồng thanh toán chính.

---

##  Luồng hoạt động thanh toán

1. Người dùng chọn **Thanh toán**.
2. `ThanhToanController` tạo đơn hàng.
3. Gọi `VnPayService` để tạo URL thanh toán.
4. `VnPayLibrary` ký bảo mật và hoàn tất URL.
5. Trình duyệt chuyển sang trang VNPay.
6. VNPay xử lý và trả kết quả về hệ thống.
7. Controller cập nhật trạng thái đơn hàng trong Database.

---
ádasdasdasdasdasdasdasdasdasdasdasdasdasdasd
