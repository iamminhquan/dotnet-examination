// =====================================================================
// [Câu 5 - Phần 2] PropertyRepository : IPropertyRepository
// Mục đích: Hiện thực các thao tác dữ liệu của Property bằng ApplicationDbContext,
//           sử dụng 100% phương thức bất đồng bộ của EF Core
//           (ToListAsync, FirstOrDefaultAsync, AddAsync, SaveChangesAsync).
// =====================================================================
using Microsoft.EntityFrameworkCore;
using RealEstate.Data;
using RealEstate.Models;

namespace RealEstate.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly ApplicationDbContext _context;

    // [Câu 5 - Phần 2] Inject ApplicationDbContext qua Constructor (DI đăng ký trong Program.cs)
    public PropertyRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // [Câu 5 - Phần 2] + [Câu 3 - Phần 2] Eager Loading bằng Include để lấy kèm Category,
    // tránh lỗi null khi View hiển thị tên danh mục. AsNoTracking vì chỉ đọc dữ liệu.
    public async Task<IEnumerable<Property>> GetAllWithCategoryAsync()
    {
        return await _context.Properties
            .AsNoTracking()
            .Include(p => p.Category)
            .OrderByDescending(p => p.Id)
            .ToListAsync();
    }

    // [Câu 5 - Phần 2] Tìm 1 BĐS theo Id, kèm Category; trả null nếu không có
    public async Task<Property?> GetByIdAsync(int id)
    {
        return await _context.Properties
            .AsNoTracking()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    // [Câu 5 - Phần 2] Thêm mới BĐS và lưu thay đổi xuống CSDL
    public async Task AddAsync(Property property)
    {
        await _context.Properties.AddAsync(property);
        await _context.SaveChangesAsync();
    }

    // [Câu 5 - Phần 2] Lấy danh sách danh mục, sắp xếp theo tên để hiển thị trong <select>
    public async Task<IEnumerable<Category>> GetCategoriesAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    // [Câu 4 - Phần 2] Lấy các BĐS có Id nằm trong danh sách (dịch sang câu SQL "WHERE Id IN (...)")
    public async Task<IEnumerable<Property>> GetByIdsAsync(IEnumerable<int> ids)
    {
        var idList = ids.ToList();
        if (idList.Count == 0)
        {
            return [];
        }

        return await _context.Properties
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => idList.Contains(p.Id))
            .ToListAsync();
    }

    // [Câu 4 - Phần 3] Lấy các BĐS thuộc 1 danh mục cụ thể
    public async Task<IEnumerable<Property>> GetByCategoryAsync(int categoryId)
    {
        return await _context.Properties
            .AsNoTracking()
            .Include(p => p.Category)
            .Where(p => p.CategoryId == categoryId)
            .OrderByDescending(p => p.Id)
            .ToListAsync();
    }
}
