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
    public class TourManagementController : Controller
    {
        SEP25Team09Entities model = new SEP25Team09Entities();
        // GET: TourManagement
        public ActionResult Home()
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                var tour = model.TOUR.ToList();
                return View(tour);
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult DetailtTour(string id)
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var tour = model.TOUR.First(t => t.Ma_Tour.Equals(id));
                    if (tour != null)
                    {
                        List<string> gioitinhNam = new List<string>();
                        gioitinhNam.Add("Nam");
                        gioitinhNam.Add("Nữ");
                        gioitinhNam.Add("Khác");
                        ViewBag.Gioi_TinhNam = new SelectList(gioitinhNam);

                        List<string> gioitinhNu = new List<string>();
                        gioitinhNu.Add("Nữ");
                        gioitinhNu.Add("Nam");
                        gioitinhNu.Add("Khác");
                        ViewBag.Gioi_TinhNu = new SelectList(gioitinhNu);

                        List<string> gioitinhKhac = new List<string>();
                        gioitinhKhac.Add("Khác");
                        gioitinhKhac.Add("Nam");
                        gioitinhKhac.Add("Nữ");
                        ViewBag.Gioi_TinhKhac = new SelectList(gioitinhKhac);
                        @Session["SoNgay"] = tour.Thoi_Gian_TraPhong.CompareTo(tour.Thoi_Gian_NhanPhong) + 1;
                        Session["tt-tour"] = model.TT_Dat_Phong.Where(t => t.Ma_Tour.Equals(id)).ToList();
                        return View(tour);
                    }
                }
                return RedirectToAction("Home");
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult PaymentTour(string id)
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if (!string.IsNullOrEmpty(id))
                {
                    try
                    {
                        var tour = model.TOUR.FirstOrDefault(t => t.Ma_Tour.Equals(id));
                        if (tour != null)
                        {
                            var ttdatphong = model.TT_Dat_Phong.Where(t => t.Ma_Tour.Equals(id)).ToList();
                            if (ttdatphong.Count >= 1)
                            {
                                foreach (var item in ttdatphong)
                                {
                                    item.Trang_Thai = 1;
                                    model.Entry(item).State = EntityState.Modified;
                                    if (item.Doi_Tra > 0)
                                    {
                                        int landoi = item.Doi_Tra;
                                        var ttdoi = model.TT_Doi_Phong.FirstOrDefault(t => t.Lan_Doi == landoi);
                                        var phong = ttdoi.Phong;
                                        phong.Ma_Trang_Thai = "TT202207050001";
                                        model.Entry(phong).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        var phong = item.Phong;
                                        phong.Ma_Trang_Thai = "TT202207050001";
                                        model.Entry(phong).State = EntityState.Modified;
                                    }
                                }
                            }
                            tour.Trang_Thai = 1; //Hoàn thành
                            tour.Thoi_Gian_ThanhToan_Huy = DateTime.Now;
                            tour.Ma_Tai_Khoan = Session["user-ma"].ToString();
                            model.Entry(tour).State = EntityState.Modified;
                            model.SaveChanges();
                            Session["thongbaoSuccess"] = "Đã thanh toán Tour!";
                            return RedirectToAction("DetailtTour", new { id = id });
                        }
                    }
                    catch (Exception e)
                    {
                        Session["error-import-file"] = e.Message;
                        return RedirectToAction("DetailtTour", new { id = id });
                    }
                }
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult UnOrderTour(string id)
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if (!string.IsNullOrEmpty(id))
                {
                    try
                    {
                        var tour = model.TOUR.FirstOrDefault(t => t.Ma_Tour.Equals(id));
                        if (tour != null)
                        {
                            var ttdatphong = model.TT_Dat_Phong.Where(t => t.Ma_Tour.Equals(id)).ToList();
                            DateTime tgianhuy = DateTime.Now;
                            if (ttdatphong.Count >= 1)
                            {

                                foreach (var item in ttdatphong)
                                {
                                    item.Trang_Thai = 2;
                                    item.Thoi_Gian_Doi_Tra = tgianhuy;
                                    model.Entry(item).State = EntityState.Modified;
                                    if (item.Doi_Tra > 0)
                                    {
                                        int landoi = item.Doi_Tra;
                                        var ttdoi = model.TT_Doi_Phong.FirstOrDefault(t => t.Lan_Doi == landoi);
                                        var phong = ttdoi.Phong;
                                        phong.Ma_Trang_Thai = "TT202207050001";
                                        model.Entry(phong).State = EntityState.Modified;
                                    }
                                    else
                                    {
                                        var phong = item.Phong;
                                        phong.Ma_Trang_Thai = "TT202207050001";
                                        model.Entry(phong).State = EntityState.Modified;
                                    }
                                }
                            }
                            tour.Trang_Thai = 2; //Hủy
                            tour.Thoi_Gian_ThanhToan_Huy = tgianhuy;
                            tour.Ma_Tai_Khoan = Session["user-ma"].ToString();
                            model.Entry(tour).State = EntityState.Modified;
                            model.SaveChanges();
                            Session["thongbaoSuccess"] = "Đã hủy Tour!";
                            return RedirectToAction("DetailtTour", new { id = id });
                        }
                    }
                    catch (Exception e)
                    {
                        Session["error-import-file"] = e.Message;
                        return RedirectToAction("DetailtTour", new { id = id });
                    }
                }
            }
            return RedirectToAction("Homepage", "Home");
        }
    }
}