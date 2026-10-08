// =====================================================================
// [Câu 5 - Phần 1] Interface IPropertyRepository (Repository Pattern)
// Mục đích: Tách lớp truy cập dữ liệu khỏi Controller. Controller chỉ làm việc
//           với interface này, không phụ thuộc trực tiếp vào DbContext.
//           Mọi phương thức đều trả về Task => bất đồng bộ 100%.
// =====================================================================
using RealEstate.Models;

namespace RealEstate.Repositories;

public interface IPropertyRepository
{
    // [Câu 5 - Phần 1] Lấy toàn bộ BĐS kèm thông tin Category (Eager Loading)
    Task<IEnumerable<Property>> GetAllWithCategoryAsync();

    // [Câu 5 - Phần 1] Lấy 1 BĐS theo Id (kèm Category), trả về null nếu không tìm thấy
    Task<Property?> GetByIdAsync(int id);

    // [Câu 5 - Phần 1] Thêm mới 1 BĐS và lưu vào CSDL
    Task AddAsync(Property property);

    // [Câu 5 - Phần 1] Lấy danh sách danh mục (phục vụ SelectList ở form Create)
    Task<IEnumerable<Category>> GetCategoriesAsync();

    // [Câu 4 - Phần 2] (Bổ sung) Lấy các BĐS theo danh sách Id - dùng hiển thị "Các BĐS vừa xem" từ Session
    Task<IEnumerable<Property>> GetByIdsAsync(IEnumerable<int> ids);

    // [Câu 4 - Phần 3] (Bổ sung) Lấy các BĐS thuộc 1 danh mục - dùng khi bấm vào link ở Category Sidebar
    Task<IEnumerable<Property>> GetByCategoryAsync(int categoryId);
}
