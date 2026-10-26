using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuanCafe.Models;

namespace QuanCafe.Controllers;

public class DonHangController : Controller
{
    private readonly AppDbContext _db;
    public DonHangController(AppDbContext db) => _db = db;

    // GET: /DonHang/Menu
    public async Task<IActionResult> Menu()
    {
        if (HttpContext.Session.GetInt32("IdPhien") == null)
            return RedirectToAction("Index", "Home");

        var sanPhams = await _db.Sanphams
            .Where(s => s.Conhang == true)
            .ToListAsync();

        return View(sanPhams);
    }

    // POST: /DonHang/DatMon
    [HttpPost]
    public async Task<IActionResult> DatMon(List<int> idSanPhams, List<int> soLuongs, string? ghiChu)
    {
        int? idPhien = HttpContext.Session.GetInt32("IdPhien");
        int? idBan = HttpContext.Session.GetInt32("IdBan");
        if (idPhien == null || idBan == null)
            return RedirectToAction("Index", "Home");

        string maDonHang = "DH" + DateTime.Now.ToString("yyMMddHHmmss");

        var donHang = new Donhang
        {
            Madonhang = maDonHang,
            Idphien = idPhien.Value,
            Idban = idBan.Value,
            Trangthai = "choxacnhan",
            Trangthaithanhtoan = "chuathanhtoan",
            Thoigiandat = DateTime.Now,
            Ghichu = ghiChu,
            Tongtien = 0
        };

        decimal tongTien = 0;

        for (int i = 0; i < idSanPhams.Count; i++)
        {
            if (soLuongs[i] <= 0) continue;
            var sp = await _db.Sanphams.FindAsync(idSanPhams[i]);
            if (sp == null) continue;

            donHang.Chitietdons.Add(new Chitietdon
            {
                Idsanpham = sp.Idsanpham,
                Soluong = soLuongs[i],
                Dongia = sp.Gia,
                Trangthai = "cholam"
            });

            tongTien += sp.Gia * soLuongs[i];
        }

        donHang.Tongtien = tongTien;

        _db.Donhangs.Add(donHang);
        await _db.SaveChangesAsync(); // lưu để có Idchitiet, Idonhang

        // Trừ kho theo công thức từng món vừa đặt
        foreach (var ct in donHang.Chitietdons)
        {
            var congThucs = await _db.Congthucsanphams
                .Where(c => c.Idsanpham == ct.Idsanpham)
                .ToListAsync();

            foreach (var ct2 in congThucs)
            {
                var nguyenLieu = await _db.Nguyenlieus.FindAsync(ct2.Idnguyenlieu);
                if (nguyenLieu == null) continue;

                decimal soLuongTru = ct2.Soluongcanthiet * ct.Soluong;
                nguyenLieu.Soluongtonkho = (nguyenLieu.Soluongtonkho ?? 0) - soLuongTru;

                _db.Lichsukhos.Add(new Lichsukho
                {
                    Idnguyenlieu = nguyenLieu.Idnguyenlieu,
                    Loai = "xuat",
                    Soluong = soLuongTru,
                    IdonhangLienQuan = donHang.Idonhang,
                    Thoigiantao = DateTime.Now
                });

                if (nguyenLieu.Soluongtonkho <= 0)
                {
                    var spHetHang = await _db.Sanphams.FindAsync(ct.Idsanpham);
                    if (spHetHang != null) spHetHang.Conhang = false;
                }
            }
        }

        await _db.SaveChangesAsync();

        return RedirectToAction("ChonThanhToan", new { maDon = maDonHang });
    }

    // GET: /DonHang/ChonThanhToan?maDon=DH...
    public IActionResult ChonThanhToan(string maDon)
    {
        ViewBag.MaDon = maDon;
        return View();
    }

    // GET: /DonHang/TrangThai?maDon=DH...
    public async Task<IActionResult> TrangThai(string maDon)
    {
        var don = await _db.Donhangs
            .Include(d => d.Chitietdons)
                .ThenInclude(c => c.IdsanphamNavigation)
            .FirstOrDefaultAsync(d => d.Madonhang == maDon);

        if (don == null) return NotFound();
        return View(don);
    }
}