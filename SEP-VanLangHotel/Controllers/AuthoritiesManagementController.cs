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
    public class AuthoritiesManagementController : Controller
    {
        // GET: AuthoritiesManagement
        SEP25Team09Entities model = new SEP25Team09Entities();
        public ActionResult Home()
        {
            if (Session["user-role"].ToString().Equals("Quản lý"))  //Tài khoản thuộc quyền Admin
            {
                var quyen = model.Quyen.ToList();
                return View(quyen);
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult AddAuthorities(string tenquyenmoi, string motaquyenmoi) //Thêm quyền
        {
            if (Session["user-role"].ToString().Equals("Quản lý")) //Tài khoản thuộc quyền Admin
            {
                Quyen quyen = new Quyen();
                var quyens = model.Quyen.FirstOrDefault(q => q.Ten_Quyen.ToLower().Equals(tenquyenmoi.Trim().ToLower()));
                if (quyens == null)
                {
                    Session["quyen-tontai"] = null;
                    quyen.Ma_Quyen = "1";
                    quyen.Ten_Quyen = tenquyenmoi.Trim();
                    quyen.Mo_Ta = motaquyenmoi.Trim();
                    model.Quyen.Add(quyen);
                    model.SaveChanges();
                    return RedirectToAction("Home");
                }
                Session["quyen-tontai"] = true;
                return RedirectToAction("Home");
            }
            return RedirectToAction("Homepage", "Home");
        }

        public ActionResult UpdateAuthorities(string maquyen, string tenquyen, string motaquyen) //Sửa quyền
        {
            if (Session["user-role"].ToString().Equals("Quản lý")) //Tài khoản thuộc quyền Admin
            {
                var quyen = model.Quyen.FirstOrDefault(q => q.Ma_Quyen.ToLower().Equals(maquyen));
                if (quyen != null)
                {
                    Session["quyen-tontai"] = null;
                    quyen.Ten_Quyen = tenquyen.Trim();
                    quyen.Mo_Ta = motaquyen.Trim();
                    model.SaveChanges();
                    return RedirectToAction("Home");
                }
                return RedirectToAction("Home");
            }
            return RedirectToAction("Homepage", "Home");
        }

        public ActionResult RemoveAuthorities(string id)
        {
            Session["try-taikhoan-dadungquyen"] = null;
            if (Session["user-role"].ToString().Equals("Quản lý")) //Tài khoản thuộc quyền Admin
            {
                var quyen = model.Quyen.FirstOrDefault(q => q.Ma_Quyen.Equals(id));
                if (quyen != null) //quyền tồn tại
                {
                    var taikhoan = model.Tai_Khoan.ToList();
                    foreach (var item in taikhoan)
                    {
                        if (item.Quyen.Ten_Quyen.Equals(quyen.Ten_Quyen)) //Nếu Quyền đã được áp dụng cho tài khoản bất kỳ
                            Session["try-taikhoan-dadungquyen"] = true;
                    }
                    if (Session["try-taikhoan-dadungquyen"] == null) //Nếu quyền chưa được áp dụng cho bất kỳ tài khoản nào
                    {
                        model.Quyen.Remove(quyen);
                        model.SaveChanges();
                        return RedirectToAction("Home");
                    }
                }
                return RedirectToAction("Home");
            }
            return RedirectToAction("Homepage", "Home");
        }
    }
}