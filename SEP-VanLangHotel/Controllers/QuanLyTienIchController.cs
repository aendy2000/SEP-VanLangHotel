using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SEP_VanLangHotel.Models;
using SEP_VanLangHotel.Middleware;
using System.Data.Entity.Validation;
using System.Data.Entity;

namespace SEP_VanLangHotel.Controllers
{
    [LoginVerification]
    public class QuanLyTienIchController : Controller
    {
        SEP25Team09Entities model = new SEP25Team09Entities();
        //trang chu ql tien ich
        public ActionResult Home()
        {
            if (Session["user-role"].Equals("Quản lý"))
            {
                var ltstienIch = model.Tien_Ich.ToList();
                return View(ltstienIch);
            }
            return RedirectToAction("Homepage", "Home");
        }
    }
}