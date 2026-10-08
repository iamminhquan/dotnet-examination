// =====================================================================
// [Câu 4 - Phần 2] SessionExtensions - Helper chuyển đổi object <-> JSON cho Session
// Mục đích: ISession chỉ lưu được byte[]/string/int. Helper này cho phép lưu
//           các kiểu phức tạp như List<int> bằng cách tuần tự hóa sang chuỗi JSON
//           và đọc ngược lại, có bẫy lỗi khi Session trống hoặc dữ liệu hỏng.
// =====================================================================
using System.Text.Json;

namespace RealEstate.Helpers;

public static class SessionExtensions
{
    // [Câu 4 - Phần 2] Ghi object vào Session dưới dạng chuỗi JSON
    public static void SetObject<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    // [Câu 4 - Phần 2] Đọc chuỗi JSON từ Session và chuyển ngược về object.
    // Trả về default (null với List<int>) nếu key chưa tồn tại hoặc JSON không hợp lệ.
    public static T? GetObject<T>(this ISession session, string key)
    {
        var json = session.GetString(key);
        if (string.IsNullOrWhiteSpace(json))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        catch (JsonException)
        {
            // Dữ liệu trong Session bị hỏng => bỏ qua, coi như chưa có dữ liệu
            return default;
        }
    }
}
