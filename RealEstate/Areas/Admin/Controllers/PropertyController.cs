// =====================================================================
// [Câu 3] PropertyController - Khu vực quản trị (Admin Area)
// Mục đích: Quản lý bất động sản dành cho Admin:
//   - Index : xem danh sách BĐS (kèm Category, ảnh thumbnail).
//   - Create: thêm mới BĐS, có upload ảnh và kiểm tra file chặt chẽ.
// Controller KHÔNG dùng DbContext trực tiếp mà thông qua IPropertyRepository (Câu 5).
// =====================================================================
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RealEstate.Data;
using RealEstate.Models;
using RealEstate.Repositories;

namespace RealEstate.Areas.Admin.Controllers;

// [Câu 3 - Phần 1] Đặt controller trong vùng "Admin" và chỉ cho phép role "Admin" truy cập
[Area("Admin")]
[Authorize(Roles = DbInitializer.RoleAdmin)]
public class PropertyController : Controller
{
    // [Câu 3 - Phần 4] Các ràng buộc cho file upload
    private const long MaxImageSize = 2 * 1024 * 1024; // 2MB
    private static readonly string[] AllowedExtensions = [".jpg", ".png"];

    // [Câu 3 - Phần 5] Thư mục lưu ảnh, tính từ wwwroot (không hardcode đường dẫn tuyệt đối)
    private const string UploadFolder = "uploads/properties";

    private readonly IPropertyRepository _propertyRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;

    // [Câu 3 - Phần 1] Inject IPropertyRepository và IWebHostEnvironment qua Constructor
    public PropertyController(IPropertyRepository propertyRepository, IWebHostEnvironment webHostEnvironment)
    {
        _propertyRepository = propertyRepository;
        _webHostEnvironment = webHostEnvironment;
    }

    // [Câu 3 - Phần 2] GET: /Admin/Property
    // Lấy danh sách BĐS kèm Category (Eager Loading .Include trong Repository) và hiển thị dạng bảng
    public async Task<IActionResult> Index()
    {
        var properties = await _propertyRepository.GetAllWithCategoryAsync();
        return View(properties);
    }

    // [Câu 3 - Phần 3] GET: /Admin/Property/Create
    // Chuẩn bị SelectList danh mục cho thẻ <select> rồi trả về form trống
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        await PopulateCategoriesAsync();
        return View(new Property());
    }

    // [Câu 3 - Phần 4, 5, 6] POST: /Admin/Property/Create
    // Nhận Model Property từ form + file ảnh IFormFile imageFile (form có enctype="multipart/form-data")
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        [Bind(nameof(Property.Title), nameof(Property.Price), nameof(Property.Description), nameof(Property.CategoryId))]
        Property property,
        IFormFile? imageFile)
    {
        // ---------------------------------------------------------------
        // [Câu 3 - Phần 4] VALIDATE FILE UPLOAD
        // ---------------------------------------------------------------
        // (1) Bắt buộc phải chọn file và file không rỗng
        if (imageFile is null || imageFile.Length == 0)
        {
            ModelState.AddModelError("imageFile", "Vui lòng chọn hình ảnh cho bất động sản.");
        }
        else
        {
            // (2) Kiểm tra kích thước: tối đa 2MB
            if (imageFile.Length > MaxImageSize)
            {
                ModelState.AddModelError("imageFile", "Kích thước ảnh không được vượt quá 2MB.");
            }

            // (3) Kiểm tra định dạng: chỉ chấp nhận .jpg hoặc .png (không phân biệt hoa/thường)
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("imageFile", "Chỉ chấp nhận file ảnh định dạng .jpg hoặc .png.");
            }
        }

        // Kiểm tra CategoryId gửi lên có thực sự tồn tại (chống sửa giá trị <select> bằng DevTools)
        var categories = await _propertyRepository.GetCategoriesAsync();
        if (!categories.Any(c => c.Id == property.CategoryId))
        {
            ModelState.AddModelError(nameof(Property.CategoryId), "Danh mục không hợp lệ.");
        }

        // ---------------------------------------------------------------
        // [Câu 3 - Phần 6] KIỂM TRA ModelState TRƯỚC KHI LƯU
        // Nếu có lỗi (Data Annotations hoặc lỗi file ở trên) => trả lại form kèm thông báo lỗi
        // ---------------------------------------------------------------
        if (!ModelState.IsValid)
        {
            await PopulateCategoriesAsync(property.CategoryId);
            return View(property);
        }

        // ---------------------------------------------------------------
        // [Câu 3 - Phần 5] LƯU FILE VẬT LÝ VÀO wwwroot/uploads/properties
        // Chỉ lưu file sau khi dữ liệu đã hợp lệ để không sinh file rác.
        // ---------------------------------------------------------------
        property.ImageUrl = await SaveImageAsync(imageFile!);

        // [Câu 3 - Phần 6] Gọi Repository lưu BĐS vào CSDL (bất đồng bộ)
        await _propertyRepository.AddAsync(property);

        TempData["SuccessMessage"] = $"Đã thêm bất động sản \"{property.Title}\" thành công.";
        return RedirectToAction(nameof(Index));
    }

    // [Câu 3 - Phần 5] Lưu file ảnh với tên duy nhất (Guid) và trả về đường dẫn tương đối để gán vào ImageUrl
    private async Task<string> SaveImageAsync(IFormFile imageFile)
    {
        // Lấy thư mục wwwroot qua IWebHostEnvironment.WebRootPath rồi ghép thư mục con bằng Path.Combine
        var uploadPath = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "properties");

        // Tạo thư mục nếu chưa tồn tại (lần upload đầu tiên)
        Directory.CreateDirectory(uploadPath);

        // Tạo tên file duy nhất bằng Guid.NewGuid(), giữ nguyên phần mở rộng .jpg/.png
        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadPath, fileName);

        // Ghi nội dung file xuống đĩa theo cách bất đồng bộ
        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(stream);
        }

        // Đường dẫn tương đối dùng cho thẻ <img src="..."> : /uploads/properties/{fileName}
        return $"/{UploadFolder}/{fileName}";
    }

    // [Câu 3 - Phần 3] Đổ danh sách danh mục vào ViewBag dưới dạng SelectList cho Tag Helper asp-items
    private async Task PopulateCategoriesAsync(int? selectedCategoryId = null)
    {
        var categories = await _propertyRepository.GetCategoriesAsync();
        ViewBag.Categories = new SelectList(categories, nameof(Category.Id), nameof(Category.Name), selectedCategoryId);
    }
}
