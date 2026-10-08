// =====================================================================
// [Câu 4 - Phần 3] CategorySidebarItemViewModel
// Mục đích: Kiểu dữ liệu strongly-typed thay cho anonymous type
//           new { Category = c, PropertyCount = c.Properties.Count() }
//           để View Default.cshtml của CategorySidebar dùng được @model và IntelliSense.
// =====================================================================
namespace RealEstate.Models.ViewModels;

public class CategorySidebarItemViewModel
{
    // Danh mục
    public Category Category { get; set; } = null!;

    // Số lượng BĐS thuộc danh mục đó
    public int PropertyCount { get; set; }
}
