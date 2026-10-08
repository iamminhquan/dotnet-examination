// =====================================================================
// [Câu 2 - Phần 2 & 3] DbInitializer - Seed Data
// Mục đích: Khi ứng dụng khởi động, tự động:
//   - Áp dụng các migration còn thiếu vào CSDL.
//   - Tạo 2 role "Admin" và "Customer" nếu chưa tồn tại.
//   - Tạo tài khoản admin mặc định (admin@caothang.edu.vn / Admin@123) và gán role "Admin",
//     có kiểm tra tồn tại trước để không bị tạo trùng.
//   - Tạo sẵn vài danh mục mẫu để form tạo BĐS có dữ liệu chọn.
// Toàn bộ thao tác đều dùng phương thức bất đồng bộ (async/await).
// =====================================================================
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RealEstate.Models;

namespace RealEstate.Data;

public static class DbInitializer
{
    // [Câu 2 - Phần 2] Hằng số tên Role, dùng chung cho Seed và [Authorize(Roles = ...)]
    public const string RoleAdmin = "Admin";
    public const string RoleCustomer = "Customer";

    // [Câu 2 - Phần 3] Thông tin tài khoản admin mặc định theo đề bài
    private const string AdminEmail = "admin@caothang.edu.vn";
    private const string AdminPassword = "Admin@123";

    // [Câu 2 - Phần 2 & 3] Hàm Seed chính, được gọi trong Program.cs (sau builder.Build(), trước app.Run())
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        // Tạo scope riêng vì DbContext, UserManager, RoleManager được đăng ký với vòng đời Scoped
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

        // [Câu 2 - Phần 2] Đảm bảo CSDL đã được tạo và cập nhật theo migration mới nhất
        await context.Database.MigrateAsync();

        // [Câu 2 - Phần 2] Tạo các Role nếu chưa tồn tại
        await SeedRolesAsync(roleManager);

        // [Câu 2 - Phần 3] Tạo tài khoản admin mặc định nếu chưa tồn tại
        await SeedAdminUserAsync(userManager);

        // Dữ liệu danh mục mẫu (phục vụ Câu 3 - thẻ <select> danh mục)
        await SeedCategoriesAsync(context);
    }

    // [Câu 2 - Phần 2] Tạo 2 role "Admin" và "Customer"; RoleExistsAsync giúp tránh tạo trùng
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = [RoleAdmin, RoleCustomer];

        foreach (var roleName in roles)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    // [Câu 2 - Phần 3] Tạo tài khoản admin mặc định và gán role "Admin"
    private static async Task SeedAdminUserAsync(UserManager<IdentityUser> userManager)
    {
        // Kiểm tra tài khoản đã tồn tại chưa trước khi tạo => tránh lỗi trùng lặp (DuplicateUserName)
        var adminUser = await userManager.FindByEmailAsync(AdminEmail);

        if (adminUser is null)
        {
            adminUser = new IdentityUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true // Xác nhận sẵn email để có thể đăng nhập ngay
            };

            var createResult = await userManager.CreateAsync(adminUser, AdminPassword);
            if (!createResult.Succeeded)
            {
                // Báo lỗi rõ ràng nếu mật khẩu không thỏa Identity Options (Câu 2 - Phần 1)
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Không thể tạo tài khoản admin mặc định: {errors}");
            }
        }

        // Gán role "Admin" nếu tài khoản chưa thuộc role này (an toàn khi chạy Seed nhiều lần)
        if (!await userManager.IsInRoleAsync(adminUser, RoleAdmin))
        {
            await userManager.AddToRoleAsync(adminUser, RoleAdmin);
        }
    }

    // Seed danh mục mẫu nếu bảng Categories đang trống
    private static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.Categories.AnyAsync())
        {
            return;
        }

        var categories = new List<Category>
        {
            new() { Name = "Căn hộ chung cư" },
            new() { Name = "Nhà phố" },
            new() { Name = "Biệt thự" },
            new() { Name = "Đất nền" }
        };

        await context.Categories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }
}
