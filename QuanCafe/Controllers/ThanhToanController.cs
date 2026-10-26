using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Models;

namespace QuanCafe.Controllers;

public class ThanhToanController : Controller
{
    private readonly AppDbContext _db;
    public ThanhToanController(AppDbContext db) => _db = db;

    // POST: /ThanhToan/ThanhToanNgay  (trả ngay sau khi chốt đơn)
    [HttpPost]
    public async Task<IActionResult> ThanhToanNgay(string maDon, string hinhThuc)
    {
        var don = await _db.Donhangs
            .Include(d => d.Idthanhtoans)
            .FirstOrDefaultAsync(d => d.Madonhang == maDon);
        if (don == null) return NotFound();

        var thanhToan = new Thanhtoan
        {
            Idphien = don.Idphien,
            Sotien = don.Tongtien ?? 0,
            Hinhthuc = hinhThuc,
            Trangthai = "hoantat",
            Thoigianthanhtoan = DateTime.Now
        };

        don.Idthanhtoans.Add(thanhToan); // EF tự tạo bản ghi bảng nối ẩn
        don.Trangthaithanhtoan = "dathanhtoan";

        await _db.SaveChangesAsync();

        return RedirectToAction("TrangThai", "DonHang", new { maDon });
    }

    // GET: /ThanhToan/ThanhToanCuoi (khách quét QR thanh toán khi dùng xong)
    public async Task<IActionResult> ThanhToanCuoi()
    {
        int? idPhien = HttpContext.Session.GetInt32("IdPhien");
        if (idPhien == null) return RedirectToAction("Index", "Home");

        var donChuaTra = await _db.Donhangs
            .Include(d => d.Chitietdons)
                .ThenInclude(c => c.IdsanphamNavigation)
            .Where(d => d.Idphien == idPhien && d.Trangthaithanhtoan == "chuathanhtoan")
            .ToListAsync();

        ViewBag.TongTien = donChuaTra.Sum(d => d.Tongtien ?? 0);
        return View(donChuaTra);
    }

    // POST: /ThanhToan/XacNhanThanhToanCuoi
    [HttpPost]
    public async Task<IActionResult> XacNhanThanhToanCuoi(string hinhThuc)
    {
        int? idPhien = HttpContext.Session.GetInt32("IdPhien");
        int? idBan = HttpContext.Session.GetInt32("IdBan");
        if (idPhien == null || idBan == null) return RedirectToAction("Index", "Home");

        var donChuaTra = await _db.Donhangs
            .Include(d => d.Idthanhtoans)
            .Where(d => d.Idphien == idPhien && d.Trangthaithanhtoan == "chuathanhtoan")
            .ToListAsync();

        decimal tongTien = donChuaTra.Sum(d => d.Tongtien ?? 0);

        var thanhToan = new Thanhtoan
        {
            Idphien = idPhien.Value,
            Sotien = tongTien,
            Hinhthuc = hinhThuc,
            Trangthai = "hoantat",
            Thoigianthanhtoan = DateTime.Now
        };

        foreach (var don in donChuaTra)
        {
            don.Idthanhtoans.Add(thanhToan);
            don.Trangthaithanhtoan = "dathanhtoan";
        }

        var phien = await _db.Phiensudungbans.FindAsync(idPhien.Value);
        if (phien != null)
        {
            phien.Trangthai = "dadong";
            phien.Thoigianketthuc = DateTime.Now;
        }

        var ban = await _db.Bans.FindAsync(idBan.Value);
        if (ban != null) ban.Trangthai = "chodon";

        await _db.SaveChangesAsync();

        HttpContext.Session.Clear();

        return View("CamOn");
    }
}