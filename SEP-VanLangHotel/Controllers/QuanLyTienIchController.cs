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
        [HttpPost]
        public ActionResult AddNewUtilities(string tentienich)
        {
            if (Session["user-role"].Equals("Quản lý"))
            {
                if (!string.IsNullOrEmpty(tentienich))
                {
                    try
                    {
                        Tien_Ich newTienIch = new Tien_Ich();
                        newTienIch.Ma_Tien_Ich = "1";
                        newTienIch.Ten_Tien_Ich = tentienich;
                        model.Tien_Ich.Add(newTienIch);
                        model.SaveChanges();
                        Session["thongbaoSuccess"] = "Thêm thành công tiện ích: " + newTienIch.Ten_Tien_Ich;
                        return RedirectToAction("Home");
                    }
                    catch (Exception e)
                    {
                        Session["thongbao-loi"] = e.Message;
                        return RedirectToAction("Home");
                    }
                }
            }
            return RedirectToAction("Homepage", "Home");
        }
    }

}