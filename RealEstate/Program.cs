// =====================================================================
// Program.cs - Điểm khởi động ứng dụng
// Chứa: đăng ký dịch vụ (DI), cấu hình Identity, Session, Repository,
//       cấu hình HTTP Pipeline và gọi Seed Data.
// =====================================================================
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;
using RealEstate.Data;
using RealEstate.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------
// [Câu 1 - Phần 4] Đọc chuỗi kết nối "DefaultConnection" từ appsettings.json
// và đăng ký ApplicationDbContext dùng SQL Server
// ---------------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Không tìm thấy chuỗi kết nối 'DefaultConnection'.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// ---------------------------------------------------------------------
// [Câu 2 - Phần 1] Cấu hình ASP.NET Core Identity + Identity Options
// AddRoles<IdentityRole>() để dùng RoleManager và [Authorize(Roles = "...")]
// ---------------------------------------------------------------------
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        // Không yêu cầu xác nhận email để tài khoản đăng ký mới có thể đăng nhập ngay
        options.SignIn.RequireConfirmedAccount = false;

        // [Câu 2 - Phần 1] Chính sách mật khẩu theo đề bài
        options.Password.RequiredLength = 6;              // Tối thiểu 6 ký tự
        options.Password.RequireUppercase = true;         // Bắt buộc có chữ hoa
        options.Password.RequireNonAlphanumeric = true;   // Bắt buộc có ký tự đặc biệt

        // Email là duy nhất cho mỗi tài khoản
        options.User.RequireUniqueEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// ---------------------------------------------------------------------
// [Câu 5 - Phần 3] Đăng ký Dependency Injection cho Repository (vòng đời Scoped,
// cùng vòng đời với DbContext - mỗi HTTP request một instance)
// ---------------------------------------------------------------------
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();

// ---------------------------------------------------------------------
// [Câu 4 - Phần 1] Đăng ký dịch vụ Session
// AddDistributedMemoryCache: kho lưu trữ dữ liệu Session trong bộ nhớ server
// AddSession: cấu hình cookie Session
// ---------------------------------------------------------------------
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session hết hạn sau 30 phút không hoạt động
    options.Cookie.HttpOnly = true;                 // JavaScript không đọc được cookie Session
    options.Cookie.IsEssential = true;              // Cookie cần thiết, không phụ thuộc consent GDPR
});

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Phục vụ các trang Login/Register của Identity UI

// Cho phép Razor xuất tiếng Việt có dấu nguyên dạng (thay vì mã hóa thành &#x...;) trong HTML
builder.Services.Configure<WebEncoderOptions>(options =>
    options.TextEncoderSettings = new TextEncoderSettings(UnicodeRanges.All));

var app = builder.Build();

// ---------------------------------------------------------------------
// [Câu 2 - Phần 4] Gọi Seed Data: SAU builder.Build() và TRƯỚC app.Run()
// ---------------------------------------------------------------------
await DbInitializer.SeedAsync(app.Services);

// Cấu hình HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles(); // Phục vụ file tĩnh trong wwwroot (bao gồm ảnh /uploads/properties/...)

app.UseRouting();

// [Câu 4 - Phần 1] Middleware Session: đặt SAU UseRouting() và TRƯỚC UseAuthorization()
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

// [Câu 3 - Phần 1] Route cho Area (Admin) - phải khai báo TRƯỚC route mặc định
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
