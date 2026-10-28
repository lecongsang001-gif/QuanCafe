using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Models;

namespace QuanCafe.Controllers;

public class MenuController : KhachController
{
    public MenuController(AppDbContext db) : base(db) { }

    // GET /  hoặc  /Menu   -> trang chào mừng + danh sách món
    public async Task<IActionResult> Index()
    {
        var sanPhams = await _db.Sanphams
            .AsNoTracking()
            .Where(s => s.Conhang == true)   // món hết nguyên liệu (conhang = 0) thì ẩn
            .OrderBy(s => s.Idsanpham)
            .ToListAsync();

        return View(sanPhams);
    }

    // GET /Menu/ChiTiet/5   -> trang riêng của một món
    public async Task<IActionResult> ChiTiet(int id)
    {
        var sanPham = await _db.Sanphams
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Idsanpham == id && s.Conhang == true);

        if (sanPham == null) return RedirectToAction(nameof(Index));

        return View(sanPham);
    }
}
