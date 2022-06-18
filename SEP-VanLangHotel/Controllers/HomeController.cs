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

        public ActionResult IndexAdmin()
        {
            //Tổng danh thu
            var doanhthu = model.TT_Dat_Phong.Sum(s => s.Phong.Loai_Phong.Gia);
            return View(doanhthu);
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}