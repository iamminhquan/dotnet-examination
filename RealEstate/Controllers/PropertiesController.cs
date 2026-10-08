// =====================================================================
// [Câu 4 - Phần 2] PropertiesController - Giao diện phía Customer
// Mục đích: Cho khách xem danh sách & chi tiết BĐS. Tại action Details,
//           lưu danh sách Id "Các BĐS vừa xem" vào Session (List<int> <-> JSON).
// =====================================================================
using Microsoft.AspNetCore.Mvc;
using RealEstate.Helpers;
using RealEstate.Models.ViewModels;
using RealEstate.Repositories;

namespace RealEstate.Controllers;

public class PropertiesController : Controller
{
    // [Câu 4 - Phần 2] Key lưu danh sách Id BĐS vừa xem trong Session
    private const string RecentlyViewedSessionKey = "RecentlyViewedPropertyIds";

    // [Câu 4 - Phần 2] Giới hạn số BĐS được ghi nhớ để Session không phình to
    private const int MaxRecentlyViewed = 5;

    private readonly IPropertyRepository _propertyRepository;

    // [Câu 5 - Phần 3] Inject Repository qua Constructor (đăng ký DI trong Program.cs)
    public PropertiesController(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    // GET: /Properties hoặc /Properties?categoryId=2
    // Hiển thị danh sách BĐS; nếu có categoryId (bấm từ Category Sidebar) thì lọc theo danh mục
    public async Task<IActionResult> Index(int? categoryId)
    {
        var properties = categoryId.HasValue
            ? await _propertyRepository.GetByCategoryAsync(categoryId.Value)
            : await _propertyRepository.GetAllWithCategoryAsync();

        if (categoryId.HasValue)
        {
            var categories = await _propertyRepository.GetCategoriesAsync();
            var category = categories.FirstOrDefault(c => c.Id == categoryId.Value);

            // Bẫy lỗi: categoryId không tồn tại
            if (category is null)
            {
                return NotFound();
            }

            ViewData["CategoryName"] = category.Name;
        }

        return View(properties);
    }

    // [Câu 4 - Phần 2] GET: /Properties/Details/5
    // Xem chi tiết BĐS + ghi nhận Id vào danh sách "Các BĐS vừa xem" trong Session
    public async Task<IActionResult> Details(int? id)
    {
        // Bẫy lỗi: thiếu id trên URL
        if (id is null)
        {
            return NotFound();
        }

        var property = await _propertyRepository.GetByIdAsync(id.Value);

        // Bẫy lỗi: id không tồn tại trong CSDL
        if (property is null)
        {
            return NotFound();
        }

        // -----------------------------------------------------------------
        // [Câu 4 - Phần 2] XỬ LÝ SESSION "CÁC BĐS VỪA XEM"
        // -----------------------------------------------------------------
        // (1) Đọc List<int> từ Session (JSON -> List<int>).
        //     Bẫy lỗi null: lần đầu truy cập Session chưa có key => khởi tạo danh sách rỗng.
        var recentlyViewedIds = HttpContext.Session.GetObject<List<int>>(RecentlyViewedSessionKey) ?? new List<int>();

        // (2) Lấy danh sách các BĐS đã xem TRƯỚC đó (không tính BĐS hiện tại) để hiển thị
        var previousIds = recentlyViewedIds.Where(x => x != property.Id).ToList();

        // (3) Kiểm tra trùng lặp: nếu Id đã có thì xóa vị trí cũ, sau đó chèn lên đầu
        //     => danh sách không bao giờ chứa Id trùng và luôn sắp theo thứ tự xem gần nhất.
        if (recentlyViewedIds.Contains(property.Id))
        {
            recentlyViewedIds.Remove(property.Id);
        }
        recentlyViewedIds.Insert(0, property.Id);

        // (4) Chỉ giữ lại tối đa MaxRecentlyViewed phần tử
        if (recentlyViewedIds.Count > MaxRecentlyViewed)
        {
            recentlyViewedIds = recentlyViewedIds.Take(MaxRecentlyViewed).ToList();
        }

        // (5) Lưu lại vào Session (List<int> -> JSON)
        HttpContext.Session.SetObject(RecentlyViewedSessionKey, recentlyViewedIds);

        // (6) Truy vấn thông tin các BĐS vừa xem và giữ đúng thứ tự trong Session
        var recentProperties = await _propertyRepository.GetByIdsAsync(previousIds);
        var orderedRecent = previousIds
            .Select(recentId => recentProperties.FirstOrDefault(p => p.Id == recentId))
            .Where(p => p is not null)
            .Select(p => p!)
            .ToList();

        var viewModel = new PropertyDetailsViewModel
        {
            Property = property,
            RecentlyViewed = orderedRecent
        };

        return View(viewModel);
    }
}
