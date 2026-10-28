using System.Globalization;
using System.Text.Json;

namespace QuanCafe.Helpers;

/// <summary>Tên các khoá lưu trong Session (dùng chung cho mọi controller).</summary>
public static class SessionKeys
{
    public const string IdPhien = "IdPhien";
    public const string IdBan = "IdBan";
    public const string MaBan = "MaBan";
    public const string GioHang = "GioHang";
}

/// <summary>Lưu / đọc object (dạng JSON) trong Session.</summary>
public static class SessionJsonExtensions
{
    public static void SetObject<T>(this ISession session, string key, T value) =>
        session.SetString(key, JsonSerializer.Serialize(value));

    public static T? GetObject<T>(this ISession session, string key)
    {
        var json = session.GetString(key);
        return json == null ? default : JsonSerializer.Deserialize<T>(json);
    }
}

/// <summary>Hàm hỗ trợ hiển thị trong View.</summary>
public static class HienThiExtensions
{
    /// <summary>25000 -> "25.000đ" (không phụ thuộc cấu hình ngôn ngữ của máy chủ).</summary>
    public static string Tien(this decimal so) =>
        so.ToString("N0", CultureInfo.InvariantCulture).Replace(',', '.') + "đ";

    public static string Tien(this decimal? so) => (so ?? 0m).Tien();

    /// <summary>
    /// Đường dẫn ảnh món để đưa vào Url.Content(...).
    /// Cột hinhanh lưu dạng "images/menu/ca-phe-den.jpg"; để trống thì dùng ảnh mặc định.
    /// </summary>
    public static string DuongDanAnh(this string? hinhanh) =>
        string.IsNullOrWhiteSpace(hinhanh)
            ? "~/images/placeholder.svg"
            : "~/" + hinhanh.Trim().TrimStart('~', '/');
}
