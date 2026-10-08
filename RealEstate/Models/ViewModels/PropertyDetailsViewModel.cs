// =====================================================================
// [Câu 4 - Phần 2] PropertyDetailsViewModel
// Mục đích: Gom dữ liệu cho trang chi tiết BĐS phía Customer gồm
//           BĐS đang xem + danh sách "Các BĐS vừa xem" đọc từ Session.
// =====================================================================
namespace RealEstate.Models.ViewModels;

public class PropertyDetailsViewModel
{
    // BĐS đang xem chi tiết
    public Property Property { get; set; } = null!;

    // Các BĐS đã xem trước đó (không bao gồm BĐS hiện tại), sắp theo thứ tự xem gần nhất
    public IEnumerable<Property> RecentlyViewed { get; set; } = [];
}
