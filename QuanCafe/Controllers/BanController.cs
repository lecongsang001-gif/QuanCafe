using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Models;

namespace QuanCafe.Controllers;

public class BanController : Controller
{
    private readonly AppDbContext _db;
    public BanController(AppDbContext db) => _db = db;

    // GET: /Ban/Quet/5   <- QR code mỗi bàn trỏ tới URL này
    [HttpGet("Ban/Quet/{idban:int}")]
    public async Task<IActionResult> Quet(int idban)
    {
        var ban = await _db.Bans.FindAsync(idban);
        if (ban == null) return NotFound("Bàn không tồn tại");

        var phien = await _db.Phiensudungbans
            .FirstOrDefaultAsync(p => p.Idban == idban && p.Trangthai == "dangmo");

        if (phien == null)
        {
            phien = new Phiensudungban
            {
                Idban = idban,
                Thoigianbatdau = DateTime.Now,
                Trangthai = "dangmo"
            };
            _db.Phiensudungbans.Add(phien);

            ban.Trangthai = "dangphucvu";

            await _db.SaveChangesAsync();
        }

        HttpContext.Session.SetInt32("IdPhien", phien.Idphien);
        HttpContext.Session.SetInt32("IdBan", idban);

        return RedirectToAction("Menu", "DonHang");
    }
}