using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Models;

namespace QuanCafe.Controllers;

public class DonHangController : KhachController
{
    public DonHangController(AppDbContext db) : base(db) { }

    // GET /DonHang/ThanhCong?maDon=261028-007
    public async Task<IActionResult> ThanhCong(string maDon)
    {
        // Chỉ cho xem đơn thuộc phiên của bàn đang ngồi (không xem được đơn bàn khác)
        var don = await _db.Donhangs
            .AsNoTracking()
            .Include(d => d.Chitietdons)
                .ThenInclude(c => c.IdsanphamNavigation)
            .FirstOrDefaultAsync(d => d.Madonhang == maDon && d.Idphien == IdPhien);

        if (don == null) return RedirectToAction("Index", "Menu");

        return View(don);
    }
}
