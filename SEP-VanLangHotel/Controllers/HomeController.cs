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
                        string maTI = item.Ma_Tien_Ich;
                        var loaiPhong = model.Loai_Phong.Where(l => l.DS_Tien_Ich.FirstOrDefault(ds => ds.Ma_Tien_Ich.Equals(maTI)).Ma_Tien_Ich.Equals(maTI)).ToList();

                        if (loaiPhong.Count > 0)
                        {
                            var matienIch = item.Ma_Tien_Ich;
                            var tentienIch = item.Ten_Tien_Ich;
                            dstienIch.Add(new Tien_Ich() { Ma_Tien_Ich = matienIch, Ten_Tien_Ich = tentienIch, IsChecks = false });
                        }
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
                ViewBag.Trong = model.Phong.Where(p => p.Ma_Trang_Thai.Equals("TT202207050001")).ToList().Count;
                var datcoc = model.Coc_Phong.Where(c => c.Trang_Thai == 0 && c.Phong.Ma_Trang_Thai.Equals("TT202207050001")).ToList();
                ViewBag.DatTruoc = datcoc.Count;
                ViewBag.DangThue = model.Phong.Where(p => p.Ma_Trang_Thai.Equals("TT202207050002")).ToList().Count;

                var phongDangChoThue = model.TT_Dat_Phong.Where(p => p.Trang_Thai == 0).ToList();
                var ttdoiphong = model.TT_Doi_Phong.ToList();
                int soluongSapTra = 0;
                int soluongmoiden = 0;
                int soluongsapden = 0;
                foreach (var items in datcoc)
                {
                    if (items.Ngay_Bat_Dau.CompareTo(Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"))) > 0 && items.Ngay_Bat_Dau.CompareTo(Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"))) <= 1)
                        soluongsapden++;
                }
                foreach (var item in phongDangChoThue)
                {
                    var mattdatphong = item.Ma_TT_Dat_Phong;
                    var landoi = item.Doi_Tra;
                    if(Convert.ToDateTime(item.Thoi_Gian_Dat.ToString("yyyy-MM-dd")) == Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd")))
                    {
                        soluongmoiden++;
                    }
                    if (item.Doi_Tra == 0 && Convert.ToDateTime(item.Thoi_Gian_Doi_Tra.ToString("yyyy-MM-dd")).CompareTo(Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"))) <= 0)
                    {
                        soluongSapTra++;
                    }
                    else if (item.Doi_Tra > 0 && Convert.ToDateTime(model.TT_Doi_Phong.FirstOrDefault(t => t.Ma_TT_Dat_Phong.Equals(mattdatphong) && t.Lan_Doi == landoi).TG_Doi_Tra.ToString("yyyy-MM-dd")).CompareTo(Convert.ToDateTime(DateTime.Now.ToString("yyyy-MM-dd"))) <= 0)
                    {
                        soluongSapTra++;
                    }
                }
                ViewBag.SapTra = soluongSapTra;
                ViewBag.MoiDen = soluongmoiden;
                ViewBag.SapDen = soluongsapden;
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

                    if (i == 1) { ViewBag.CocThangMot = coc.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.ThanhToanThangMot = thanhtoan.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.TongThangMot = tong.ToString("0,0").Replace(",", "").Replace(".", ""); }
                    else if (i == 2) { ViewBag.CocThangHai = coc.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.ThanhToanThangHai = thanhtoan.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.TongThangHai = tong.ToString("0,0").Replace(",", "").Replace(".", ""); }
                    else if (i == 3) { ViewBag.CocThangBa = coc.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.ThanhToanThangBa = thanhtoan.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.TongThangBa = tong.ToString("0,0").Replace(",", "").Replace(".", ""); }
                    else if (i == 4) { ViewBag.CocThangBon = coc.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.ThanhToanThangBon = thanhtoan.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.TongThangBon = tong.ToString("0,0").Replace(",", "").Replace(".", ""); }
                    else if (i == 5) { ViewBag.CocThangNam = coc.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.ThanhToanThangNam = thanhtoan.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.TongThangNam = tong.ToString("0,0").Replace(",", "").Replace(".", ""); }
                    else if (i == 6) { ViewBag.CocThangSau = coc.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.ThanhToanThangSau = thanhtoan.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.TongThangSau = tong.ToString("0,0").Replace(",", "").Replace(".", ""); }
                    else if (i == 7) { ViewBag.CocThangBay = coc.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.ThanhToanThangBay = thanhtoan.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.TongThangBay = tong.ToString("0,0").Replace(",", "").Replace(".", ""); }
                    else if (i == 8) { ViewBag.CocThangTam = coc.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.ThanhToanThangTam = thanhtoan.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.TongThangTam = tong.ToString("0,0").Replace(",", "").Replace(".", ""); }
                    else if (i == 9) { ViewBag.CocThangChin = coc.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.ThanhToanThangChin = thanhtoan.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.TongThangChin = tong.ToString("0,0").Replace(",", "").Replace(".", ""); }
                    else if (i == 10) { ViewBag.CocThangMuoi = coc.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.ThanhToanThangMuoi = thanhtoan.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.TongThangMuoi = tong.ToString("0,0").Replace(",", "").Replace(".", ""); }
                    else if (i == 11) { ViewBag.CocThangMMot = coc.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.ThanhToanThangMot = thanhtoan.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.TongThangMMot = tong.ToString("0,0").Replace(",", "").Replace(".", ""); }
                    else if (i == 12) { ViewBag.CocThangMHai = coc.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.ThanhToanThangMHai = thanhtoan.ToString("0,0").Replace(",", "").Replace(".", ""); ViewBag.TongThangMHai = tong.ToString("0,0").Replace(",", "").Replace(".", ""); }
                }
                return View();
            }
            return RedirectToAction("Homepage");

        }
    }
}