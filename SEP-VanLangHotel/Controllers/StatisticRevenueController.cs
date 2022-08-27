using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SEP_VanLangHotel.Models;
using SEP_VanLangHotel.Middleware;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.IO;
using Firebase.Auth;
using System.Threading;
using Firebase.Storage;
using System.Threading.Tasks;
using System.Text;

namespace SEP_VanLangHotel.Controllers
{
    [LoginVerification]
    public class StatisticRevenueController : Controller
    {
        SEP25Team09Entities model = new SEP25Team09Entities();
        // GET: StatisticRevenue
        public ActionResult Home()
        {
            if (Session["user-role"].Equals("Quản lý"))
            {
                //Sao kê theo năm
                for (int i = 1; i <= 12; i++)
                {
                    int days = DateTime.DaysInMonth(DateTime.Now.Year, i);
                    string ssDate = DateTime.Now.Year + "-" + i.ToString().PadLeft(2, '0') + "-01 00:00:00";
                    string seDate = DateTime.Now.Year + "-" + i.ToString().PadLeft(2, '0') + "-" + days.ToString().PadLeft(2, '0') + " 00:00:00";
                    DateTime stD = DateTime.ParseExact(ssDate, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    DateTime enD = DateTime.ParseExact(seDate, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture).AddDays(1);

                    var saoke = model.Sao_Ke.Where(s => s.Ngay_Giao_Dich >= stD && s.Ngay_Giao_Dich < enD).ToList();
                    decimal tong = 0;
                    decimal coc = 0;
                    decimal thanhtoan = 0;

                    if (saoke.Count > 0)
                    {
                        foreach (var item in saoke.Where(s => s.Coc_or_ThanhToan == 0).ToList())
                            coc += item.So_Tien;
                        foreach (var item in saoke.Where(s => s.Coc_or_ThanhToan == 1).ToList())
                            thanhtoan += item.So_Tien;
                        foreach (var item in saoke)
                            tong += item.So_Tien;
                    }

                    if (i == 1) { ViewBag.CocThangMot = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangMot = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangMot = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                    else if (i == 2) { ViewBag.CocThangHai = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangHai = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangHai = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                    else if (i == 3) { ViewBag.CocThangBa = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangBa = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangBa = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                    else if (i == 4) { ViewBag.CocThangBon = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangBon = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangBon = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                    else if (i == 5) { ViewBag.CocThangNam = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangNam = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangNam = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                    else if (i == 6) { ViewBag.CocThangSau = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangSau = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangSau = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                    else if (i == 7) { ViewBag.CocThangBay = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangBay = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangBay = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                    else if (i == 8) { ViewBag.CocThangTam = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangTam = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangTam = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                    else if (i == 9) { ViewBag.CocThangChin = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangChin = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangChin = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                    else if (i == 10) { ViewBag.CocThangMuoi = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangMuoi = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangMuoi = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                    else if (i == 11) { ViewBag.CocThangMMot = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangMot = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangMMot = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                    else if (i == 12) { ViewBag.CocThangMHai = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangMHai = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangMHai = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                }

                for (int i = 1; i <= DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month); i++)
                {
                    DateTime startDate = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM") + "-" + i.ToString().PadLeft(2, '0'));
                    DateTime endDate = startDate.AddDays(1);

                    var saoketheothang = model.Sao_Ke.Where(s => s.Ngay_Giao_Dich >= startDate && s.Ngay_Giao_Dich < endDate).ToList();

                    decimal coc = 0;
                    decimal thanhtoan = 0;
                    decimal tong = 0;
                    int soluong = 0;

                    if (saoketheothang.Count > 0)
                    {
                        foreach (var item in saoketheothang.Where(s => s.Coc_or_ThanhToan == 0).ToList())
                            coc += item.So_Tien;
                        foreach (var item in saoketheothang.Where(s => s.Coc_or_ThanhToan == 1).ToList())
                            thanhtoan += item.So_Tien;
                        foreach (var item in saoketheothang)
                            tong += item.So_Tien;
                        soluong += saoketheothang.Count;
                    }

                    if (i == 1) { ViewBag.CocN1 = coc; ViewBag.ThanhToanN1 = thanhtoan; ViewBag.TongN1 = tong; ViewBag.SLN1 = soluong; }
                    else if (i == 2) { ViewBag.CocN2 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN2 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN2 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN2 = soluong; }
                    else if (i == 3) { ViewBag.CocN3 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN3 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN3 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN3 = soluong; }
                    else if (i == 4) { ViewBag.CocN4 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN4 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN4 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN4 = soluong; }
                    else if (i == 5) { ViewBag.CocN5 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN5 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN5 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN5 = soluong; }
                    else if (i == 6) { ViewBag.CocN6 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN6 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN6 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN6 = soluong; }
                    else if (i == 7) { ViewBag.CocN7 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN7 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN7 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN7 = soluong; }
                    else if (i == 8) { ViewBag.CocN8 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN8 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN8 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN8 = soluong; }
                    else if (i == 9) { ViewBag.CocN9 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN9 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN9 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN9 = soluong; }
                    else if (i == 10) { ViewBag.CocN10 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN10 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN10 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN10 = soluong; }
                    else if (i == 11) { ViewBag.CocN11 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN11 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN11 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN11 = soluong; }
                    else if (i == 12) { ViewBag.CocN12 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN12 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN12 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN12 = soluong; }
                    else if (i == 13) { ViewBag.CocN13 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN13 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN13 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN13 = soluong; }
                    else if (i == 14) { ViewBag.CocN14 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN14 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN14 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN14 = soluong; }
                    else if (i == 15) { ViewBag.CocN15 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN15 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN15 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN15 = soluong; }
                    else if (i == 16) { ViewBag.CocN16 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN16 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN16 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN16 = soluong; }
                    else if (i == 17) { ViewBag.CocN17 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN17 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN17 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN17 = soluong; }
                    else if (i == 18) { ViewBag.CocN18 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN18 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN18 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN18 = soluong; }
                    else if (i == 19) { ViewBag.CocN19 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN19 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN19 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN19 = soluong; }
                    else if (i == 20) { ViewBag.CocN20 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN20 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN20 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN20 = soluong; }
                    else if (i == 21) { ViewBag.CocN21 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN21 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN21 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN21 = soluong; }
                    else if (i == 22) { ViewBag.CocN22 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN22 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN22 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN22 = soluong; }
                    else if (i == 23) { ViewBag.CocN23 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN23 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN23 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN23 = soluong; }
                    else if (i == 24) { ViewBag.CocN24 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN24 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN24 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN24 = soluong; }
                    else if (i == 25) { ViewBag.CocN25 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN25 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN25 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN25 = soluong; }
                    else if (i == 26) { ViewBag.CocN26 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN26 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN26 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN26 = soluong; }
                    else if (i == 27) { ViewBag.CocN27 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN27 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN27 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN27 = soluong; }
                    else if (i == 28) { ViewBag.CocN28 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN28 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN28 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN28 = soluong; }
                    else if (i == 29) { ViewBag.CocN29 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN29 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN29 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN29 = soluong; }
                    else if (i == 30) { ViewBag.CocN30 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN30 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN30 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN30 = soluong; }
                    else if (i == 31) { ViewBag.CocN31 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN31 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN31 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN31 = soluong; }
                }
                DateTime strDate = Convert.ToDateTime(DateTime.Now.Year + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "-" + 1.ToString().PadLeft(2, '0'));
                DateTime enDate = Convert.ToDateTime(DateTime.Now.Year + "-" + DateTime.Now.Month.ToString().PadLeft(2, '0') + "-" + DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month).ToString().PadLeft(2, '0')).AddDays(1);
                var dbSaoKe = model.Sao_Ke.Where(s => s.Ngay_Giao_Dich >= strDate && s.Ngay_Giao_Dich < enDate);
                return View(dbSaoKe);
            }
            return RedirectToAction("Homepage");
        }

        [HttpPost]
        public ActionResult SearchExportRevenue(string thanglist, string namlist, string btnSearch)
        {
            if (Session["user-role"].Equals("Quản lý"))
            {
                if (!string.IsNullOrEmpty(thanglist) && !string.IsNullOrEmpty(namlist)
                    && !string.IsNullOrEmpty(btnSearch))
                {
                    //Sao kê theo năm
                    for (int i = 1; i <= 12; i++)
                    {
                        int days = DateTime.DaysInMonth(Convert.ToInt32(namlist), i);
                        string ssDate = Convert.ToInt32(namlist) + "-" + i.ToString().PadLeft(2, '0') + "-01 00:00:00";
                        string seDate = Convert.ToInt32(namlist) + "-" + i.ToString().PadLeft(2, '0') + "-" + days.ToString().PadLeft(2, '0') + " 00:00:00";
                        DateTime stD = DateTime.ParseExact(ssDate, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                        DateTime enD = DateTime.ParseExact(seDate, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture).AddDays(1);

                        var saoke = model.Sao_Ke.Where(s => s.Ngay_Giao_Dich >= stD && s.Ngay_Giao_Dich < enD).ToList();
                        decimal tong = 0;
                        decimal coc = 0;
                        decimal thanhtoan = 0;

                        if (saoke.Count > 0)
                        {
                            foreach (var item in saoke.Where(s => s.Coc_or_ThanhToan == 0).ToList())
                                coc += item.So_Tien;
                            foreach (var item in saoke.Where(s => s.Coc_or_ThanhToan == 1).ToList())
                                thanhtoan += item.So_Tien;
                            foreach (var item in saoke)
                                tong += item.So_Tien;
                        }

                        if (i == 1) { ViewBag.CocThangMot = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangMot = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangMot = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                        else if (i == 2) { ViewBag.CocThangHai = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangHai = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangHai = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                        else if (i == 3) { ViewBag.CocThangBa = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangBa = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangBa = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                        else if (i == 4) { ViewBag.CocThangBon = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangBon = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangBon = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                        else if (i == 5) { ViewBag.CocThangNam = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangNam = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangNam = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                        else if (i == 6) { ViewBag.CocThangSau = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangSau = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangSau = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                        else if (i == 7) { ViewBag.CocThangBay = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangBay = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangBay = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                        else if (i == 8) { ViewBag.CocThangTam = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangTam = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangTam = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                        else if (i == 9) { ViewBag.CocThangChin = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangChin = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangChin = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                        else if (i == 10) { ViewBag.CocThangMuoi = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangMuoi = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangMuoi = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                        else if (i == 11) { ViewBag.CocThangMMot = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangMot = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangMMot = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                        else if (i == 12) { ViewBag.CocThangMHai = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanThangMHai = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongThangMHai = tong.ToString("0,0").Replace(".", "").Replace(",", ""); }
                    }
                    //sao kê theo tháng
                    for (int i = 1; i <= DateTime.DaysInMonth(Convert.ToInt32(namlist), Convert.ToInt32(thanglist)); i++)
                    {
                        DateTime startDate = Convert.ToDateTime(namlist + "-" + thanglist.PadLeft(2, '0') + "-" + i.ToString().PadLeft(2, '0'));
                        DateTime endDate = startDate.AddDays(1);

                        var saoketheothang = model.Sao_Ke.Where(s => s.Ngay_Giao_Dich >= startDate && s.Ngay_Giao_Dich < endDate).ToList();

                        decimal coc = 0;
                        decimal thanhtoan = 0;
                        decimal tong = 0;
                        int soluong = 0;

                        if (saoketheothang.Count > 0)
                        {
                            foreach (var item in saoketheothang.Where(s => s.Coc_or_ThanhToan == 0).ToList())
                                coc += item.So_Tien;
                            foreach (var item in saoketheothang.Where(s => s.Coc_or_ThanhToan == 1).ToList())
                                thanhtoan += item.So_Tien;
                            foreach (var item in saoketheothang)
                                tong += item.So_Tien;
                            soluong += saoketheothang.Count;
                        }

                        if (i == 1) { ViewBag.CocN1 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN1 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN1 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN1 = soluong; }
                        else if (i == 2) { ViewBag.CocN2 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN2 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN2 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN2 = soluong; }
                        else if (i == 3) { ViewBag.CocN3 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN3 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN3 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN3 = soluong; }
                        else if (i == 4) { ViewBag.CocN4 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN4 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN4 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN4 = soluong; }
                        else if (i == 5) { ViewBag.CocN5 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN5 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN5 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN5 = soluong; }
                        else if (i == 6) { ViewBag.CocN6 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN6 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN6 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN6 = soluong; }
                        else if (i == 7) { ViewBag.CocN7 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN7 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN7 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN7 = soluong; }
                        else if (i == 8) { ViewBag.CocN8 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN8 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN8 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN8 = soluong; }
                        else if (i == 9) { ViewBag.CocN9 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN9 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN9 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN9 = soluong; }
                        else if (i == 10) { ViewBag.CocN10 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN10 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN10 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN10 = soluong; }
                        else if (i == 11) { ViewBag.CocN11 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN11 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN11 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN11 = soluong; }
                        else if (i == 12) { ViewBag.CocN12 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN12 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN12 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN12 = soluong; }
                        else if (i == 13) { ViewBag.CocN13 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN13 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN13 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN13 = soluong; }
                        else if (i == 14) { ViewBag.CocN14 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN14 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN14 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN14 = soluong; }
                        else if (i == 15) { ViewBag.CocN15 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN15 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN15 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN15 = soluong; }
                        else if (i == 16) { ViewBag.CocN16 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN16 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN16 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN16 = soluong; }
                        else if (i == 17) { ViewBag.CocN17 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN17 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN17 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN17 = soluong; }
                        else if (i == 18) { ViewBag.CocN18 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN18 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN18 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN18 = soluong; }
                        else if (i == 19) { ViewBag.CocN19 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN19 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN19 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN19 = soluong; }
                        else if (i == 20) { ViewBag.CocN20 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN20 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN20 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN20 = soluong; }
                        else if (i == 21) { ViewBag.CocN21 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN21 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN21 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN21 = soluong; }
                        else if (i == 22) { ViewBag.CocN22 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN22 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN22 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN22 = soluong; }
                        else if (i == 23) { ViewBag.CocN23 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN23 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN23 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN23 = soluong; }
                        else if (i == 24) { ViewBag.CocN24 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN24 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN24 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN24 = soluong; }
                        else if (i == 25) { ViewBag.CocN25 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN25 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN25 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN25 = soluong; }
                        else if (i == 26) { ViewBag.CocN26 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN26 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN26 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN26 = soluong; }
                        else if (i == 27) { ViewBag.CocN27 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN27 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN27 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN27 = soluong; }
                        else if (i == 28) { ViewBag.CocN28 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN28 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN28 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN28 = soluong; }
                        else if (i == 29) { ViewBag.CocN29 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN29 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN29 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN29 = soluong; }
                        else if (i == 30) { ViewBag.CocN30 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN30 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN30 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN30 = soluong; }
                        else if (i == 31) { ViewBag.CocN31 = coc.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.ThanhToanN31 = thanhtoan.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.TongN31 = tong.ToString("0,0").Replace(".", "").Replace(",", ""); ViewBag.SLN31 = soluong; }

                    }
                    ViewBag.Thang = thanglist.PadLeft(2, '0');
                    ViewBag.Nam = namlist;
                    DateTime strDate = Convert.ToDateTime(namlist + "-" + thanglist.PadLeft(2, '0') + "-" + 1.ToString().PadLeft(2, '0'));
                    DateTime enDate = Convert.ToDateTime(namlist + "-" + thanglist.PadLeft(2, '0') + "-" + DateTime.DaysInMonth(Convert.ToInt32(namlist), Convert.ToInt32(thanglist)).ToString().PadLeft(2, '0')).AddDays(1);

                    var dbSaoKe = model.Sao_Ke.Where(s => s.Ngay_Giao_Dich >= strDate && s.Ngay_Giao_Dich < enDate);
                    return View("Home", dbSaoKe);
                }
                return RedirectToAction("Home");
            }
            return RedirectToAction("Homepage", "Home");
        }
    }
}