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
        VanLangHotelEntities model = new VanLangHotelEntities();
        public ActionResult Home()
        {
            if (Session["user-role"].ToString().Equals("Admin"))  //Tài khoản thuộc quyền Admin
            {
                var quyen = model.Quyen.ToList();
                return View(quyen);
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult AddAuthorities() //Thêm quyền
        {
            if (Session["user-role"].ToString().Equals("Admin")) //Tài khoản thuộc quyền Admin
            {
                Quyen quyen = new Quyen();
                return View(quyen);
            }
            return RedirectToAction("Homepage", "Home");
        }
        [HttpPost]
        public ActionResult AddAuthorities(Quyen quyen)
        {
            if (Session["user-role"].ToString().Equals("Admin"))//Tài khoản thuộc quyền Admin
            {
                if(ModelState.IsValid)
                {
                    try
                    {
                        quyen.Ma_Quyen = "1";
                        model.Quyen.Add(quyen);
                        model.SaveChanges();
                    }
                    catch (Exception)
                    {
                        throw;
                    }
                    return View("Home");
                }
                return View(quyen);

            }
            return RedirectToAction("Homepage", "Home");
        }


        public ActionResult UpdateAuthorities(string id) //Chỉnh sửa quyền
        {
            if (Session["user-role"].ToString().Equals("Admin")) //Tài khoản thuộc quyền Admin
            {
                var quyen = model.Quyen.Find(id);
                if (quyen != null)
                {
                    return View(quyen);
                }
                return View("Home");
            }
            return RedirectToAction("Homepage", "Home");
        }
        [HttpPost]
        public ActionResult UpdateAuthorities(Quyen quyen)
        {
            if (Session["user-role"].ToString().Equals("Admin")) //Tài khoản thuộc quyền Admin
            {
                if (ModelState.IsValid)
                {
                    model.Entry(quyen).State = EntityState.Modified;
                    model.SaveChanges();
                    return View("Home");
                }
                return View(quyen);
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult RemoveAuthorities(string id)
        {
            Session["try-taikhoan-dadungquyen"] = null;
            if (Session["user-role"].ToString().Equals("Admin")) //Tài khoản thuộc quyền Admin
            {
                var quyen = model.Quyen.Find(id);
                if (quyen != null)
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
                        return View("Home");
                    }
                }
                return View("Home");
            }
            return RedirectToAction("Homepage", "Home");
        }
    }
}