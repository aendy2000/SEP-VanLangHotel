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
    public class RoomManagementController : Controller
    {
        SEP25Team09Entities model = new SEP25Team09Entities();
        // GET: RoomManagement
        public ActionResult Home()
        {
            return View();
        }
        public ActionResult ListOfEmptyRooms()
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                var phongTrong = model.Phong.Where(p => p.Ma_Trang_Thai.Equals("TT202207050001") || p.Ma_Trang_Thai.Equals("TT202207050004")).ToList();
                return View(phongTrong);
            }
            return RedirectToAction("Homepage", "Home");

        }
        public ActionResult DetailtEmptyRooms(string id)
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if (!string.IsNullOrEmpty(id))
                {
                    ViewBag.Ma_Trang_Thai = new SelectList(model.Trang_Thai.Where(t => !t.Ma_Trang_Thai.Equals("TT202207110001") && !t.Ma_Trang_Thai.Equals("TT202207050003")), "Ma_Trang_Thai", "Ten_Trang_Thai");
                    var phong = model.Phong.Where(p => p.Ma_Phong.Equals(id) && (p.Ma_Trang_Thai.Equals("TT202207050001") || p.Ma_Trang_Thai.Equals("TT202207050004"))).First();
                    return View(phong);
                }
                return RedirectToAction("ListOfEmptyRooms");
            }
            return RedirectToAction("Homepage", "Home");
        }
        [HttpPost]
        public ActionResult DetailtEmptyRooms(Phong phong)
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if (phong != null)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            model.Entry(phong).State = EntityState.Modified;
                            model.SaveChanges();
                            ViewBag.Ma_Trang_Thai = new SelectList(model.Trang_Thai.Where(t => !t.Ma_Trang_Thai.Equals("TT202207110001") && !t.Ma_Trang_Thai.Equals("TT202207050003")), "Ma_Trang_Thai", "Ten_Trang_Thai");
                            return RedirectToAction("DetailtEmptyRooms", new {id = phong.Ma_Phong});
                        }
                        catch (Exception e)
                        {
                            Session["error-import-file"] = "Lỗi " + e.Message;
                            ViewBag.Ma_Trang_Thai = new SelectList(model.Trang_Thai.Where(t => !t.Ma_Trang_Thai.Equals("TT202207110001") && !t.Ma_Trang_Thai.Equals("TT202207050003")), "Ma_Trang_Thai", "Ten_Trang_Thai");
                            return View(phong);
                        }
                    }
                    ViewBag.Ma_Trang_Thai = new SelectList(model.Trang_Thai.Where(t => !t.Ma_Trang_Thai.Equals("TT202207110001") && !t.Ma_Trang_Thai.Equals("TT202207050003")), "Ma_Trang_Thai", "Ten_Trang_Thai");
                    return View(phong);
                }
                return RedirectToAction("ListOfEmptyRooms");
            }
            return View("Homepage", "Home");
        }
        public ActionResult ListOfRentingRooms()
        {
            var phongDangChoThue = model.TT_Dat_Phong.Where(p => p.Phong.Ma_Trang_Thai.Equals("TT202207110001")).ToList();
            return View(phongDangChoThue);
        }
        public ActionResult ListRoomDueToPay()
        {
            DateTime ngayhientai = DateTime.Now;
            var phongDangChoThue = model.TT_Dat_Phong.Where(p => DateTime.Compare(p.Thoi_Gian_Doi_Tra, ngayhientai) < 1 && p.Phong.Ma_Trang_Thai.Equals("TT202207110001")).ToList();
            return View(phongDangChoThue);
        }
    }
}