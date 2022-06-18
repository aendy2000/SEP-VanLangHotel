using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SEP_VanLangHotel.Models;
using SEP_VanLangHotel.Middleware;
namespace SEP_VanLangHotel.Controllers
{
    [LoginVerification]
    public class HomeController : Controller
    {
        VanLangHotelEntities model = new VanLangHotelEntities();
        public ActionResult Homepage()
        {
            //Nếu Là tài khoản của nhân viên
            if (Session["user-role"].Equals("Staff"))
                return View(model.Tien_Ich.ToList());
            //Nếu Là tài khoản của admin sẽ đi đến hàm Dashboard
            else
                return RedirectToAction("Dashboard");

        }
        //Tài khoản admin sẽ được đi đến trang dashboard
        public ActionResult Dashboard()
        {
            //Return với tổng doanh thu của khách sạn
            return View("Dashboard", model.TT_Dat_Phong.Sum(s => s.Phong.Loai_Phong.Gia));
        }
    }
}