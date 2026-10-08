// =====================================================================
// [Câu 1 - Phần 1] Model POCO "Category" (Danh mục bất động sản)
// Mục đích: Định nghĩa bảng Categories trong CSDL bằng Data Annotations
//           và thiết lập quan hệ 1-N (một Category có nhiều Property).
// =====================================================================
using System.ComponentModel.DataAnnotations;

namespace RealEstate.Models;

public class Category
{
    // [Câu 1 - Phần 1] Khóa chính (EF Core tự nhận diện qua [Key] + quy ước tên "Id")
    [Key]
    public int Id { get; set; }

    // [Câu 1 - Phần 1] Tên danh mục: bắt buộc nhập, tối đa 50 ký tự
    [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
    [StringLength(50, ErrorMessage = "Tên danh mục tối đa 50 ký tự")]
    [Display(Name = "Tên danh mục")]
    public string Name { get; set; } = string.Empty;

    // [Câu 1 - Phần 1] Thuộc tính điều hướng phía "1" của quan hệ 1-N:
    // một Category chứa nhiều Property
    public ICollection<Property> Properties { get; set; } = new List<Property>();
}
