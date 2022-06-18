using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SEP_VanLangHotel.Models;

namespace SEP_VanLangHotel.Controllers
{
    public class AuthController : Controller
    {
        // GET: Admin/LoginLogout
        VanLangHotelEntities model = new VanLangHotelEntities();
        public ActionResult Login()
        {
            Session["username-incorrect"] = null;
            Session["password-incorrect"] = null;
            return View();
        }
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            var user = model.Tai_Khoan.FirstOrDefault(u => u.Ten_Dang_Nhap.Equals(username));
            if (user != null)
            {
                if (user.Quyen.Ten_Quyen.Equals("Admin"))
                {
                    Session["username-incorrect"] = null;
                    if (user.Mat_Khau.Equals(password))
                    {
                        Session["user-fullname"] = user.Ho_Va_Ten;
                        Session["user-id"] = user.Ten_Dang_Nhap;
                        Session["user-role"] = user.Quyen.Ten_Quyen;
                        return RedirectToAction("Index", "DashboardAdmin");
                    }
                    else
                    {
                        Session["password-incorrect"] = true;
                        return View();
                    }
                }
            }

            Session["username-incorrect"] = true;
            Session["password-incorrect"] = true;
            return View();
        }
        public ActionResult Logout()
        {
            Session["user-fullname"] = null;
            Session["user-id"] = null;
            Session["user-role"] = null;
            return RedirectToAction("Login");
        }

    }
}