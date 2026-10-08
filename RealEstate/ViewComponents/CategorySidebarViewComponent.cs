// =====================================================================
// [Câu 4 - Phần 3] CategorySidebarViewComponent
// Mục đích: Thành phần giao diện tái sử dụng, hiển thị danh sách danh mục
//           kèm SỐ LƯỢNG BĐS của từng danh mục ở thanh bên (sidebar) trong _Layout.
// Truy vấn dữ liệu nằm ở lớp C# này (không nằm trong View .cshtml).
// =====================================================================
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstate.Data;
using RealEstate.Models.ViewModels;

namespace RealEstate.ViewComponents;

// Tên lớp "CategorySidebarViewComponent" => tên component là "CategorySidebar"
// => View mặc định: Views/Shared/Components/CategorySidebar/Default.cshtml
public class CategorySidebarViewComponent : ViewComponent
{
    private readonly ApplicationDbContext _context;

    // [Câu 4 - Phần 3] Inject DbContext qua Constructor
    public CategorySidebarViewComponent(ApplicationDbContext context)
    {
        _context = context;
    }

    // [Câu 4 - Phần 3] Được gọi bởi @await Component.InvokeAsync("CategorySidebar")
    public async Task<IViewComponentResult> InvokeAsync()
    {
        // Truy vấn LINQ đếm số BĐS theo từng Category.
        // Tương đương new { Category = c, PropertyCount = c.Properties.Count() }
        // nhưng dùng ViewModel strongly-typed để View khai báo được @model.
        // EF Core dịch c.Properties.Count() thành sub-query COUNT(*) trong SQL.
        var items = await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new CategorySidebarItemViewModel
            {
                Category = c,
                PropertyCount = c.Properties.Count()
            })
            .ToListAsync();

        // Trả về View "Default" của component
        return View(items);
    }
}
