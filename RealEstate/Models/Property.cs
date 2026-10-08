// =====================================================================
// [Câu 1 - Phần 2] Model POCO "Property" (Bất động sản)
// Mục đích: Định nghĩa bảng Properties trong CSDL bằng Data Annotations,
//           có khóa ngoại CategoryId trỏ về bảng Categories (phía "N" của quan hệ 1-N).
// =====================================================================
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RealEstate.Models;

public class Property
{
    // [Câu 1 - Phần 2] Khóa chính
    [Key]
    public int Id { get; set; }

    // [Câu 1 - Phần 2] Tiêu đề: bắt buộc, tối đa 100 ký tự
    [Required(ErrorMessage = "Tiêu đề là bắt buộc")]
    [StringLength(100, ErrorMessage = "Tiêu đề tối đa 100 ký tự")]
    [Display(Name = "Tiêu đề")]
    public string Title { get; set; } = string.Empty;

    // [Câu 1 - Phần 2] Giá: bắt buộc và phải lớn hơn 0.
    // Cột SQL được khai báo decimal(18,2) để tránh cảnh báo mất độ chính xác của EF Core.
    [Required(ErrorMessage = "Giá là bắt buộc")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
    [Column(TypeName = "decimal(18,2)")]
    [Display(Name = "Giá (VNĐ)")]
    public decimal Price { get; set; }

    // [Câu 1 - Phần 2] Mô tả chi tiết (không bắt buộc)
    [Display(Name = "Mô tả")]
    public string? Description { get; set; }

    // [Câu 1 - Phần 2] Đường dẫn tương đối của ảnh, ví dụ: /uploads/properties/{guid}.jpg
    // Để kiểu nullable vì giá trị này do Controller gán SAU khi lưu file (Câu 3),
    // không phải do người dùng nhập trên form => tránh ModelState báo lỗi "required" ngầm định.
    [Display(Name = "Hình ảnh")]
    public string? ImageUrl { get; set; }

    // [Câu 1 - Phần 2] Khóa ngoại trỏ tới Category
    [Required(ErrorMessage = "Vui lòng chọn danh mục")]
    [Display(Name = "Danh mục")]
    public int CategoryId { get; set; }

    // [Câu 1 - Phần 2] Thuộc tính điều hướng phía "N" của quan hệ 1-N.
    // Nullable để model binding/ModelState không yêu cầu người dùng gửi lên cả object Category.
    [ForeignKey(nameof(CategoryId))]
    public Category? Category { get; set; }
}
