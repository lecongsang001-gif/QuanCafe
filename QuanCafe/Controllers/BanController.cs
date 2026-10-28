using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Helpers;
using QuanCafe.Models;

namespace QuanCafe.Controllers;

public class BanController : Controller
{
    private readonly AppDbContext _db;

    public BanController(AppDbContext db) => _db = db;

    // Mã QR dán ở mỗi bàn trỏ tới:  https://<tên-miền>/Ban/Quet/{idban}
    [HttpGet("Ban/Quet/{idban:int}")]
    public async Task<IActionResult> Quet(int idban)
    {
        var ban = await _db.Bans.FindAsync(idban);
        if (ban == null) return NotFound("Bàn không tồn tại");

        // Mỗi bàn chỉ có 1 phiên đang mở; khách cùng bàn dùng chung phiên
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

        var session = HttpContext.Session;

        // Quét sang bàn/phiên khác thì bỏ giỏ hàng cũ
        if (session.GetInt32(SessionKeys.IdPhien) != phien.Idphien)
            session.Remove(SessionKeys.GioHang);

        session.SetInt32(SessionKeys.IdPhien, phien.Idphien);
        session.SetInt32(SessionKeys.IdBan, ban.Idban);
        session.SetString(SessionKeys.MaBan, ban.Maban);

        return RedirectToAction("Index", "Menu");
    }
}
