using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Models;

namespace QuanCafe.Controllers;

public class NhanVienController : Controller
{
    private readonly AppDbContext _db;
    public NhanVienController(AppDbContext db) => _db = db;

    // GET: /NhanVien/DanhSachDon
    public async Task<IActionResult> DanhSachDon()
    {
        var donHangs = await _db.Donhangs
            .Include(d => d.Chitietdons)
                .ThenInclude(c => c.IdsanphamNavigation)
            .Include(d => d.IdbanNavigation)
            .Where(d => d.Trangthai != "dagiao" && d.Trangthai != "huy")
            .OrderBy(d => d.Thoigiandat)
            .ToListAsync();

        return View(donHangs);
    }

    // POST: /NhanVien/CapNhatMon
    [HttpPost]
    public async Task<IActionResult> CapNhatMon(int idChiTiet, string trangThaiMoi)
    {
        var chiTiet = await _db.Chitietdons.FindAsync(idChiTiet);
        if (chiTiet == null) return NotFound();

        chiTiet.Trangthai = trangThaiMoi; // danglam / xong

        var donHang = await _db.Donhangs
            .Include(d => d.Chitietdons)
            .FirstOrDefaultAsync(d => d.Idonhang == chiTiet.Idonhang);

        if (donHang != null && donHang.Chitietdons.All(c => c.Trangthai == "xong"))
        {
            donHang.Trangthai = "sansanggiao";
        }

        await _db.SaveChangesAsync();
        return RedirectToAction("DanhSachDon");
    }

    // POST: /NhanVien/XacNhanGiao
    [HttpPost]
    public async Task<IActionResult> XacNhanGiao(int idDonHang)
    {
        var don = await _db.Donhangs
            .Include(d => d.Chitietdons)
            .FirstOrDefaultAsync(d => d.Idonhang == idDonHang);
        if (don == null) return NotFound();

        don.Trangthai = "dagiao";
        foreach (var ct in don.Chitietdons)
            ct.Trangthai = "dagiao";

        await _db.SaveChangesAsync();
        return RedirectToAction("DanhSachDon");
    }
}