# dotnet-examination — Quản lý Bất động sản (ASP.NET Core MVC .NET 8)

Bài kiểm tra thực hành môn Lập trình ASP.NET Core: EF Core + Identity + Repository Pattern + File Upload + Session + View Component.

## Cấu trúc thư mục

```
RealEstate/
├── Program.cs                                  # [Câu 1-4, 2-1, 2-4, 4-1, 5-3] DI, Identity Options, Session, Seed, Pipeline
├── appsettings.json                            # [Câu 1-4] ConnectionStrings:DefaultConnection (LocalDB)
├── RealEstate.csproj                           # net8.0 + EF Core / Identity 8.0.11
├── Models/
│   ├── Category.cs                             # [Câu 1-1] POCO + Data Annotations, quan hệ 1-N
│   ├── Property.cs                             # [Câu 1-2] POCO + Data Annotations, FK CategoryId
│   └── ViewModels/
│       ├── CategorySidebarItemViewModel.cs     # [Câu 4-3] { Category, PropertyCount }
│       └── PropertyDetailsViewModel.cs         # [Câu 4-2] BĐS + "Các BĐS vừa xem"
├── Data/
│   ├── ApplicationDbContext.cs                 # [Câu 1-3] IdentityDbContext + DbSet
│   ├── DbInitializer.cs                        # [Câu 2-2, 2-3] Seed Roles + Admin + danh mục mẫu
│   └── Migrations/                             # [Câu 1-4] Migration InitialCreate
├── Repositories/
│   ├── IPropertyRepository.cs                  # [Câu 5-1] Interface async
│   └── PropertyRepository.cs                   # [Câu 5-2] Hiện thực bằng ApplicationDbContext
├── Helpers/
│   └── SessionExtensions.cs                    # [Câu 4-2] SetObject/GetObject (List<int> <-> JSON)
├── Areas/Admin/
│   ├── Controllers/PropertyController.cs       # [Câu 3] [Area("Admin")] [Authorize(Roles="Admin")] Index, Create + upload
│   └── Views/
│       ├── _ViewImports.cshtml
│       ├── _ViewStart.cshtml
│       └── Property/
│           ├── Index.cshtml                    # [Câu 3-2] Bảng Bootstrap + thumbnail + tên Category
│           └── Create.cshtml                   # [Câu 3-3] Form multipart + asp-for/asp-items/asp-validation-for
├── Controllers/
│   ├── HomeController.cs
│   └── PropertiesController.cs                 # [Câu 4-2] Phía Customer: Index, Details (Session "vừa xem")
├── ViewComponents/
│   └── CategorySidebarViewComponent.cs         # [Câu 4-3] LINQ đếm số BĐS theo danh mục
├── Views/
│   ├── Properties/Index.cshtml, Details.cshtml
│   └── Shared/
│       ├── _Layout.cshtml                      # [Câu 4-4] @await Component.InvokeAsync("CategorySidebar")
│       └── Components/CategorySidebar/Default.cshtml
└── wwwroot/uploads/properties/                 # [Câu 3-5] Nơi lưu ảnh upload (tên Guid)
```

## Tạo Migration & Update Database (Câu 1 - Phần 4)

Repo đã có sẵn migration `InitialCreate`. Nếu muốn tự tạo lại từ đầu, xóa thư mục `RealEstate/Data/Migrations` rồi chạy:

**.NET CLI** (trong thư mục `RealEstate/`):

```bash
dotnet tool install --global dotnet-ef        # chỉ cần cài 1 lần
dotnet ef migrations add InitialCreate --output-dir Data/Migrations
dotnet ef database update
```

**Package Manager Console** (Visual Studio):

```powershell
Add-Migration InitialCreate -OutputDir Data/Migrations
Update-Database
```

> Ghi chú: `DbInitializer.SeedAsync` cũng gọi `Database.MigrateAsync()` khi ứng dụng khởi động, nên chỉ cần `dotnet run` là CSDL được tạo và seed tự động.

## Chạy ứng dụng

```bash
cd RealEstate
dotnet run
```

| Tài khoản | Mật khẩu | Role |
|---|---|---|
| `admin@caothang.edu.vn` | `Admin@123` | Admin |

- Trang quản trị: `/Admin/Property` (chưa đăng nhập sẽ bị chuyển tới trang Login; không phải Admin bị chặn).
- Trang khách hàng: `/Properties`, chi tiết `/Properties/Details/{id}` (ghi nhận "Các BĐS vừa xem" vào Session).

### Chạy trên Linux/macOS (không có LocalDB)

LocalDB chỉ có trên Windows. Chạy SQL Server bằng container và ghi đè chuỗi kết nối bằng user-secrets
(không sửa `appsettings.json`, không đưa mật khẩu vào repo):

```bash
podman run -d --name realestate-sql -e ACCEPT_EULA=Y -e 'MSSQL_SA_PASSWORD=<mật khẩu mạnh>' \
  -p 1433:1433 -v realestate-sqldata:/var/opt/mssql mcr.microsoft.com/mssql/server:2022-latest

cd RealEstate
dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
  "Server=localhost,1433;Database=RealEstateDb;User Id=sa;Password=<mật khẩu mạnh>;TrustServerCertificate=True"
dotnet run
```

Lần sau chỉ cần `podman start realestate-sql` rồi `dotnet run`.
Máy chỉ có runtime .NET 9/10 vẫn chạy được nhờ `<RollForward>Major</RollForward>` trong `RealEstate.csproj`.
