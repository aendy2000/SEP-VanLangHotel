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

                    if (i == 1) { ViewBag.CocThangMot = coc; ViewBag.ThanhToanThangMot = thanhtoan; ViewBag.TongThangMot = tong; }
                    else if (i == 2) { ViewBag.CocThangHai = coc; ViewBag.ThanhToanThangHai = thanhtoan; ViewBag.TongThangHai = tong; }
                    else if (i == 3) { ViewBag.CocThangBa = coc; ViewBag.ThanhToanThangBa = thanhtoan; ViewBag.TongThangBa = tong; }
                    else if (i == 4) { ViewBag.CocThangBon = coc; ViewBag.ThanhToanThangBon = thanhtoan; ViewBag.TongThangBon = tong; }
                    else if (i == 5) { ViewBag.CocThangNam = coc; ViewBag.ThanhToanThangNam = thanhtoan; ViewBag.TongThangNam = tong; }
                    else if (i == 6) { ViewBag.CocThangSau = coc; ViewBag.ThanhToanThangSau = thanhtoan; ViewBag.TongThangSau = tong; }
                    else if (i == 7) { ViewBag.CocThangBay = coc; ViewBag.ThanhToanThangBay = thanhtoan; ViewBag.TongThangBay = tong; }
                    else if (i == 8) { ViewBag.CocThangTam = coc; ViewBag.ThanhToanThangTam = thanhtoan; ViewBag.TongThangTam = tong; }
                    else if (i == 9) { ViewBag.CocThangChin = coc; ViewBag.ThanhToanThangChin = thanhtoan; ViewBag.TongThangChin = tong; }
                    else if (i == 10) { ViewBag.CocThangMuoi = coc; ViewBag.ThanhToanThangMuoi = thanhtoan; ViewBag.TongThangMuoi = tong; }
                    else if (i == 11) { ViewBag.CocThangMMot = coc; ViewBag.ThanhToanThangMot = thanhtoan; ViewBag.TongThangMMot = tong; }
                    else if (i == 12) { ViewBag.CocThangMHai = coc; ViewBag.ThanhToanThangMHai = thanhtoan; ViewBag.TongThangMHai = tong; }
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
                    else if (i == 2) { ViewBag.CocN2 = coc; ViewBag.ThanhToanN2 = thanhtoan; ViewBag.TongN2 = tong; ViewBag.SLN2 = soluong; }
                    else if (i == 3) { ViewBag.CocN3 = coc; ViewBag.ThanhToanN3 = thanhtoan; ViewBag.TongN3 = tong; ViewBag.SLN3 = soluong; }
                    else if (i == 4) { ViewBag.CocN4 = coc; ViewBag.ThanhToanN4 = thanhtoan; ViewBag.TongN4 = tong; ViewBag.SLN4 = soluong; }
                    else if (i == 5) { ViewBag.CocN5 = coc; ViewBag.ThanhToanN5 = thanhtoan; ViewBag.TongN5 = tong; ViewBag.SLN5 = soluong; }
                    else if (i == 6) { ViewBag.CocN6 = coc; ViewBag.ThanhToanN6 = thanhtoan; ViewBag.TongN6 = tong; ViewBag.SLN6 = soluong; }
                    else if (i == 7) { ViewBag.CocN7 = coc; ViewBag.ThanhToanN7 = thanhtoan; ViewBag.TongN7 = tong; ViewBag.SLN7 = soluong; }
                    else if (i == 8) { ViewBag.CocN8 = coc; ViewBag.ThanhToanN8 = thanhtoan; ViewBag.TongN8 = tong; ViewBag.SLN8 = soluong; }
                    else if (i == 9) { ViewBag.CocN9 = coc; ViewBag.ThanhToanN9 = thanhtoan; ViewBag.TongN9 = tong; ViewBag.SLN9 = soluong; }
                    else if (i == 10) { ViewBag.CocN10 = coc; ViewBag.ThanhToanN10 = thanhtoan; ViewBag.TongN10 = tong; ViewBag.SLN10 = soluong; }
                    else if (i == 11) { ViewBag.CocN11 = coc; ViewBag.ThanhToanN11 = thanhtoan; ViewBag.TongN11 = tong; ViewBag.SLN11 = soluong; }
                    else if (i == 12) { ViewBag.CocN12 = coc; ViewBag.ThanhToanN12 = thanhtoan; ViewBag.TongN12 = tong; ViewBag.SLN12 = soluong; }
                    else if (i == 13) { ViewBag.CocN13 = coc; ViewBag.ThanhToanN13 = thanhtoan; ViewBag.TongN13 = tong; ViewBag.SLN13 = soluong; }
                    else if (i == 14) { ViewBag.CocN14 = coc; ViewBag.ThanhToanN14 = thanhtoan; ViewBag.TongN14 = tong; ViewBag.SLN14 = soluong; }
                    else if (i == 15) { ViewBag.CocN15 = coc; ViewBag.ThanhToanN15 = thanhtoan; ViewBag.TongN15 = tong; ViewBag.SLN15 = soluong; }
                    else if (i == 16) { ViewBag.CocN16 = coc; ViewBag.ThanhToanN16 = thanhtoan; ViewBag.TongN16 = tong; ViewBag.SLN16 = soluong; }
                    else if (i == 17) { ViewBag.CocN17 = coc; ViewBag.ThanhToanN17 = thanhtoan; ViewBag.TongN17 = tong; ViewBag.SLN17 = soluong; }
                    else if (i == 18) { ViewBag.CocN18 = coc; ViewBag.ThanhToanN18 = thanhtoan; ViewBag.TongN18 = tong; ViewBag.SLN18 = soluong; }
                    else if (i == 19) { ViewBag.CocN19 = coc; ViewBag.ThanhToanN19 = thanhtoan; ViewBag.TongN19 = tong; ViewBag.SLN19 = soluong; }
                    else if (i == 20) { ViewBag.CocN20 = coc; ViewBag.ThanhToanN20 = thanhtoan; ViewBag.TongN20 = tong; ViewBag.SLN20 = soluong; }
                    else if (i == 21) { ViewBag.CocN21 = coc; ViewBag.ThanhToanN21 = thanhtoan; ViewBag.TongN21 = tong; ViewBag.SLN21 = soluong; }
                    else if (i == 22) { ViewBag.CocN22 = coc; ViewBag.ThanhToanN22 = thanhtoan; ViewBag.TongN22 = tong; ViewBag.SLN22 = soluong; }
                    else if (i == 23) { ViewBag.CocN23 = coc; ViewBag.ThanhToanN23 = thanhtoan; ViewBag.TongN23 = tong; ViewBag.SLN23 = soluong; }
                    else if (i == 24) { ViewBag.CocN24 = coc; ViewBag.ThanhToanN24 = thanhtoan; ViewBag.TongN24 = tong; ViewBag.SLN24 = soluong; }
                    else if (i == 25) { ViewBag.CocN25 = coc; ViewBag.ThanhToanN25 = thanhtoan; ViewBag.TongN25 = tong; ViewBag.SLN25 = soluong; }
                    else if (i == 26) { ViewBag.CocN26 = coc; ViewBag.ThanhToanN26 = thanhtoan; ViewBag.TongN26 = tong; ViewBag.SLN26 = soluong; }
                    else if (i == 27) { ViewBag.CocN27 = coc; ViewBag.ThanhToanN27 = thanhtoan; ViewBag.TongN27 = tong; ViewBag.SLN27 = soluong; }
                    else if (i == 28) { ViewBag.CocN28 = coc; ViewBag.ThanhToanN28 = thanhtoan; ViewBag.TongN28 = tong; ViewBag.SLN28 = soluong; }
                    else if (i == 29) { ViewBag.CocN29 = coc; ViewBag.ThanhToanN29 = thanhtoan; ViewBag.TongN29 = tong; ViewBag.SLN29 = soluong; }
                    else if (i == 30) { ViewBag.CocN30 = coc; ViewBag.ThanhToanN30 = thanhtoan; ViewBag.TongN30 = tong; ViewBag.SLN30 = soluong; }
                    else if (i == 31) { ViewBag.CocN31 = coc; ViewBag.ThanhToanN31 = thanhtoan; ViewBag.TongN31 = tong; ViewBag.SLN31 = soluong; }
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

                        if (i == 1) { ViewBag.CocThangMot = coc; ViewBag.ThanhToanThangMot = thanhtoan; ViewBag.TongThangMot = tong; }
                        else if (i == 2) { ViewBag.CocThangHai = coc; ViewBag.ThanhToanThangHai = thanhtoan; ViewBag.TongThangHai = tong; }
                        else if (i == 3) { ViewBag.CocThangBa = coc; ViewBag.ThanhToanThangBa = thanhtoan; ViewBag.TongThangBa = tong; }
                        else if (i == 4) { ViewBag.CocThangBon = coc; ViewBag.ThanhToanThangBon = thanhtoan; ViewBag.TongThangBon = tong; }
                        else if (i == 5) { ViewBag.CocThangNam = coc; ViewBag.ThanhToanThangNam = thanhtoan; ViewBag.TongThangNam = tong; }
                        else if (i == 6) { ViewBag.CocThangSau = coc; ViewBag.ThanhToanThangSau = thanhtoan; ViewBag.TongThangSau = tong; }
                        else if (i == 7) { ViewBag.CocThangBay = coc; ViewBag.ThanhToanThangBay = thanhtoan; ViewBag.TongThangBay = tong; }
                        else if (i == 8) { ViewBag.CocThangTam = coc; ViewBag.ThanhToanThangTam = thanhtoan; ViewBag.TongThangTam = tong; }
                        else if (i == 9) { ViewBag.CocThangChin = coc; ViewBag.ThanhToanThangChin = thanhtoan; ViewBag.TongThangChin = tong; }
                        else if (i == 10) { ViewBag.CocThangMuoi = coc; ViewBag.ThanhToanThangMuoi = thanhtoan; ViewBag.TongThangMuoi = tong; }
                        else if (i == 11) { ViewBag.CocThangMMot = coc; ViewBag.ThanhToanThangMot = thanhtoan; ViewBag.TongThangMMot = tong; }
                        else if (i == 12) { ViewBag.CocThangMHai = coc; ViewBag.ThanhToanThangMHai = thanhtoan; ViewBag.TongThangMHai = tong; }
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

                        if (i == 1) { ViewBag.CocN1 = coc; ViewBag.ThanhToanN1 = thanhtoan; ViewBag.TongN1 = tong; ViewBag.SLN1 = soluong; }
                        else if (i == 2) { ViewBag.CocN2 = coc; ViewBag.ThanhToanN2 = thanhtoan; ViewBag.TongN2 = tong; ViewBag.SLN2 = soluong; }
                        else if (i == 3) { ViewBag.CocN3 = coc; ViewBag.ThanhToanN3 = thanhtoan; ViewBag.TongN3 = tong; ViewBag.SLN3 = soluong; }
                        else if (i == 4) { ViewBag.CocN4 = coc; ViewBag.ThanhToanN4 = thanhtoan; ViewBag.TongN4 = tong; ViewBag.SLN4 = soluong; }
                        else if (i == 5) { ViewBag.CocN5 = coc; ViewBag.ThanhToanN5 = thanhtoan; ViewBag.TongN5 = tong; ViewBag.SLN5 = soluong; }
                        else if (i == 6) { ViewBag.CocN6 = coc; ViewBag.ThanhToanN6 = thanhtoan; ViewBag.TongN6 = tong; ViewBag.SLN6 = soluong; }
                        else if (i == 7) { ViewBag.CocN7 = coc; ViewBag.ThanhToanN7 = thanhtoan; ViewBag.TongN7 = tong; ViewBag.SLN7 = soluong; }
                        else if (i == 8) { ViewBag.CocN8 = coc; ViewBag.ThanhToanN8 = thanhtoan; ViewBag.TongN8 = tong; ViewBag.SLN8 = soluong; }
                        else if (i == 9) { ViewBag.CocN9 = coc; ViewBag.ThanhToanN9 = thanhtoan; ViewBag.TongN9 = tong; ViewBag.SLN9 = soluong; }
                        else if (i == 10) { ViewBag.CocN10 = coc; ViewBag.ThanhToanN10 = thanhtoan; ViewBag.TongN10 = tong; ViewBag.SLN10 = soluong; }
                        else if (i == 11) { ViewBag.CocN11 = coc; ViewBag.ThanhToanN11 = thanhtoan; ViewBag.TongN11 = tong; ViewBag.SLN11 = soluong; }
                        else if (i == 12) { ViewBag.CocN12 = coc; ViewBag.ThanhToanN12 = thanhtoan; ViewBag.TongN12 = tong; ViewBag.SLN12 = soluong; }
                        else if (i == 13) { ViewBag.CocN13 = coc; ViewBag.ThanhToanN13 = thanhtoan; ViewBag.TongN13 = tong; ViewBag.SLN13 = soluong; }
                        else if (i == 14) { ViewBag.CocN14 = coc; ViewBag.ThanhToanN14 = thanhtoan; ViewBag.TongN14 = tong; ViewBag.SLN14 = soluong; }
                        else if (i == 15) { ViewBag.CocN15 = coc; ViewBag.ThanhToanN15 = thanhtoan; ViewBag.TongN15 = tong; ViewBag.SLN15 = soluong; }
                        else if (i == 16) { ViewBag.CocN16 = coc; ViewBag.ThanhToanN16 = thanhtoan; ViewBag.TongN16 = tong; ViewBag.SLN16 = soluong; }
                        else if (i == 17) { ViewBag.CocN17 = coc; ViewBag.ThanhToanN17 = thanhtoan; ViewBag.TongN17 = tong; ViewBag.SLN17 = soluong; }
                        else if (i == 18) { ViewBag.CocN18 = coc; ViewBag.ThanhToanN18 = thanhtoan; ViewBag.TongN18 = tong; ViewBag.SLN18 = soluong; }
                        else if (i == 19) { ViewBag.CocN19 = coc; ViewBag.ThanhToanN19 = thanhtoan; ViewBag.TongN19 = tong; ViewBag.SLN19 = soluong; }
                        else if (i == 20) { ViewBag.CocN20 = coc; ViewBag.ThanhToanN20 = thanhtoan; ViewBag.TongN20 = tong; ViewBag.SLN20 = soluong; }
                        else if (i == 21) { ViewBag.CocN21 = coc; ViewBag.ThanhToanN21 = thanhtoan; ViewBag.TongN21 = tong; ViewBag.SLN21 = soluong; }
                        else if (i == 22) { ViewBag.CocN22 = coc; ViewBag.ThanhToanN22 = thanhtoan; ViewBag.TongN22 = tong; ViewBag.SLN22 = soluong; }
                        else if (i == 23) { ViewBag.CocN23 = coc; ViewBag.ThanhToanN23 = thanhtoan; ViewBag.TongN23 = tong; ViewBag.SLN23 = soluong; }
                        else if (i == 24) { ViewBag.CocN24 = coc; ViewBag.ThanhToanN24 = thanhtoan; ViewBag.TongN24 = tong; ViewBag.SLN24 = soluong; }
                        else if (i == 25) { ViewBag.CocN25 = coc; ViewBag.ThanhToanN25 = thanhtoan; ViewBag.TongN25 = tong; ViewBag.SLN25 = soluong; }
                        else if (i == 26) { ViewBag.CocN26 = coc; ViewBag.ThanhToanN26 = thanhtoan; ViewBag.TongN26 = tong; ViewBag.SLN26 = soluong; }
                        else if (i == 27) { ViewBag.CocN27 = coc; ViewBag.ThanhToanN27 = thanhtoan; ViewBag.TongN27 = tong; ViewBag.SLN27 = soluong; }
                        else if (i == 28) { ViewBag.CocN28 = coc; ViewBag.ThanhToanN28 = thanhtoan; ViewBag.TongN28 = tong; ViewBag.SLN28 = soluong; }
                        else if (i == 29) { ViewBag.CocN29 = coc; ViewBag.ThanhToanN29 = thanhtoan; ViewBag.TongN29 = tong; ViewBag.SLN29 = soluong; }
                        else if (i == 30) { ViewBag.CocN30 = coc; ViewBag.ThanhToanN30 = thanhtoan; ViewBag.TongN30 = tong; ViewBag.SLN30 = soluong; }
                        else if (i == 31) { ViewBag.CocN31 = coc; ViewBag.ThanhToanN31 = thanhtoan; ViewBag.TongN31 = tong; ViewBag.SLN31 = soluong; }

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