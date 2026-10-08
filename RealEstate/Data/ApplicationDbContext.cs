// =====================================================================
// [Câu 1 - Phần 3] ApplicationDbContext
// Mục đích: Lớp ngữ cảnh EF Core kế thừa IdentityDbContext để có sẵn các bảng
//           của ASP.NET Core Identity (AspNetUsers, AspNetRoles, ...) và
//           khai báo thêm 2 bảng nghiệp vụ: Categories, Properties.
// =====================================================================
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealEstate.Models;

namespace RealEstate.Data;

public class ApplicationDbContext : IdentityDbContext
{
    // [Câu 1 - Phần 3] Constructor nhận DbContextOptions (chuỗi kết nối, provider SQL Server)
    // được cấu hình trong Program.cs qua AddDbContext
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // [Câu 1 - Phần 3] DbSet ánh xạ tới bảng Categories
    public DbSet<Category> Categories { get; set; }

    // [Câu 1 - Phần 3] DbSet ánh xạ tới bảng Properties
    public DbSet<Property> Properties { get; set; }

    // [Câu 1 - Phần 3] Cấu hình bổ sung bằng Fluent API cho quan hệ 1-N Category - Property
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Bắt buộc gọi base để IdentityDbContext tạo cấu hình cho các bảng Identity
        base.OnModelCreating(builder);

        // Một Category có nhiều Property; mỗi Property thuộc đúng một Category.
        // Restrict: không cho xóa Category khi vẫn còn Property tham chiếu tới (tránh mất dữ liệu).
        builder.Entity<Property>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Properties)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
