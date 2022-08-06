using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SEP_VanLangHotel.Models;
using SEP_VanLangHotel.Middleware;
using System.Web;
using System.IO;
using System.Configuration;
using System.Data.OleDb;
using System.Data;
using System.Globalization;
using System.Data.Entity;

namespace SEP_VanLangHotel.Controllers
{
    [LoginVerification]
    public class HomeController : Controller
    {
        SEP25Team09Entities model = new SEP25Team09Entities();
        public ActionResult Homepage()
        {
            //Nếu Là tài khoản của nhân viên
            if (Session["user-role"].Equals("Nhân viên"))
            {
                DateTime NGAYHN = Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"));
                var checkDatCoc = model.Coc_Phong.Where(c => c.Ngay_Bat_Dau.CompareTo(NGAYHN) <= 0 && c.Trang_Thai == 0 && c.Phong.Ma_Trang_Thai.Equals("TT202207050001")).ToList();
                if (checkDatCoc.Count > 0)
                {
                    for (int i = 0; i < checkDatCoc.Count; i++)
                    {
                        var phong = checkDatCoc[i].Phong;
                        phong.Ma_Trang_Thai = "TT202207050002";
                        model.Entry(phong).State = EntityState.Modified;
                        model.SaveChanges();
                    }
                }

                List<Tien_Ich> dstienIch = new List<Tien_Ich>();
                foreach (var item in model.Tien_Ich.ToList())
                {
                    if (!item.Ma_Tien_Ich.Equals("TI202207070001"))
                    {
                        var matienIch = item.Ma_Tien_Ich;
                        var tentienIch = item.Ten_Tien_Ich;
                        dstienIch.Add(new Tien_Ich() { Ma_Tien_Ich = matienIch, Ten_Tien_Ich = tentienIch, IsChecks = false });
                    }
                }
                ListTienIch tienichList = new ListTienIch();
                tienichList.tienIch = dstienIch;

                return View(tienichList);
            }
            return RedirectToAction("Dashboard"); //Nếu Là tài khoản của admin sẽ đi đến hàm Dashboard
        }

        //Tài khoản admin sẽ được đi đến trang dashboard
        public ActionResult Dashboard()
        {
            //Return với tổng doanh thu của khách sạn

            if (Session["user-role"].Equals("Quản lý"))
            {
                for (int i = 1; i <= 12; i++)
                {
                    int days = DateTime.DaysInMonth(DateTime.Now.Year, i);
                    string ssDate = DateTime.Now.Year + "-" + i.ToString().PadLeft(2, '0') + "-01 00:00:00";
                    string seDate = DateTime.Now.Year + "-" + i.ToString().PadLeft(2, '0') + "-" + days.ToString().PadLeft(2, '0') + " 00:00:00";
                    DateTime stD = DateTime.ParseExact(ssDate, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    DateTime enD = DateTime.ParseExact(seDate, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);

                    var saoke = model.Sao_Ke.Where(s => s.Ngay_Giao_Dich >= stD && s.Ngay_Giao_Dich <= enD).ToList();
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
                return View();
            }
            return RedirectToAction("Homepage");

        }
    }
}