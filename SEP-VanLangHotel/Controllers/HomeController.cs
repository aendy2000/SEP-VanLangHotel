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
                DateTime NGAYHN = Convert.ToDateTime(DateTime.Now.ToString("dd/MM/yyyy"));
                var checkDatCoc = model.Coc_Phong.Where(c => c.Ngay_Bat_Dau.CompareTo(NGAYHN) <= 0 && c.Trang_Thai == 0 && c.Phong.Ma_Trang_Thai.Equals("TT202207050001")).ToList();
                if(checkDatCoc.Count > 0)
                {
                    for (int i = 0; i < checkDatCoc.Count; i++)
                    {
                        checkDatCoc[i].Trang_Thai = 1;
                        var phong = checkDatCoc[i].Phong;
                        phong.Ma_Trang_Thai = "TT202207050002";
                        model.Entry(checkDatCoc[i]).State = EntityState.Modified;
                        model.Entry(phong).State = EntityState.Modified;
                        model.SaveChanges();
                    }
                }

                List<Tien_Ich> dstienIch = new List<Tien_Ich>();
                foreach (var item in model.Tien_Ich.ToList())
                {
                    if(!item.Ma_Tien_Ich.Equals("TI202207070001"))
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
                //decimal thangmot = 0;
                //var thangmot = model.TT_Dat_Phong.Where(t => t.);

                for (int i = 1; i <= 12; i++)
                {
                    int days = DateTime.DaysInMonth(DateTime.Now.Year, i);
                    string ssDate = DateTime.Now.Year + "-" + i.ToString().PadLeft(2, '0') + "-01 00:00:00";
                    string seDate = DateTime.Now.Year + "-" + i.ToString().PadLeft(2, '0') + "-" + days.ToString().PadLeft(2, '0') + " 00:00:00";
                    DateTime stD = DateTime.ParseExact(ssDate, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    DateTime enD = DateTime.ParseExact(seDate, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    var checks = model.TT_Dat_Phong.ToList();
                    foreach (var item in checks)
                    {
                      
                    }
                }

                ViewBag.ThangMot = 100;
                ViewBag.ThangHai = 100;
                ViewBag.ThangBa = 100;
                ViewBag.ThangBon = 100;
                ViewBag.ThangNam = 100;
                ViewBag.ThangSau = 100;
                ViewBag.ThangBay = 100;
                ViewBag.ThangTam = 100;
                ViewBag.ThangChin = 100;
                ViewBag.ThangMuoi = 100;
                ViewBag.ThangMMot = 100;
                ViewBag.ThangMHai = 100;

                return View("Dashboard", Convert.ToDecimal(1000000));
            }
            return RedirectToAction("Homepage");

        }
    }
}