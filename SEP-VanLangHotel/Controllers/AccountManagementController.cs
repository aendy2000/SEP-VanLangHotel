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
    public class AccountManagementController : Controller
    {
        // GET: AcountManagement
        VanLangHotelEntities model = new VanLangHotelEntities();
        public ActionResult Home() //trang chủ ql tài khoản của Admin
        {
            if (Session["user-role"].Equals("Admin")) //Tài khoản thuộc quyền Admin
            {
                var taikhoan = model.Tai_Khoan.ToList();
                return View(taikhoan);
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult SearchAccount(string keyword) //Tìm kiếm tài khoản
        {
            if (Session["user-role"].Equals("Admin")) //Tài khoản thuộc quyền Admin
            {
                var taikhoan = model.Tai_Khoan.Where(t => t.Ten_Dang_Nhap.ToUpper().Contains(keyword.ToUpper())).ToList();
                ViewBag.Keywork = keyword;
                return View("Home", taikhoan);
            }
            return RedirectToAction("Homepage", "Home");
        }

        public ActionResult AddNewAccount() //Thêm mới 1 tài khoản - Admin
        {
            if (Session["user-role"].Equals("Admin")) //Tài khoản thuộc quyền Admin
            {
                Tai_Khoan newAccount = new Tai_Khoan();
                ViewBag.Ma_Quyen = new SelectList(model.Quyen, "Ma_Quyen", "Ten_Quyen");
                ViewBag.Ma_Khach_San = new SelectList(model.Khach_San, "Ma_Khach_San", "Ten_Khach_San");
                return View(newAccount);
            }
            return RedirectToAction("Homepage", "Home");
        }
        [HttpPost]
        public ActionResult AddNewAccount(Tai_Khoan newAccount)
        {
            if (Session["user-role"].Equals("Admin")) //Tài khoản thuộc quyền Admin
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        newAccount.Ma_Tai_Khoan = "1"; //Mã tạm thời (Trigger tại DB sẽ thay đổi sau khi thêm thành công tài khoán)
                        model.Tai_Khoan.Add(newAccount);
                        model.SaveChanges();
                    }
                    catch (DbEntityValidationException e) //Nhận thông tin lỗi thêm tài khoản bị thất bại
                    {
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                eve.Entry.Entity.GetType().Name, eve.Entry.State);
                            foreach (var ve in eve.ValidationErrors)
                            {
                                Console.WriteLine("- Property: \"{0}\", Value: \"{1}\", Error: \"{2}\"",
                                    ve.PropertyName,
                                    eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName),
                                    ve.ErrorMessage);
                            }
                        }
                        throw;
                    }
                    return RedirectToAction("Home");
                } //Thêm thất bại sẽ:
                ViewBag.Ma_Quyen = new SelectList(model.Quyen, "Ma_Quyen", "Ten_Quyen");
                ViewBag.Ma_Khach_San = new SelectList(model.Khach_San, "Ma_Khach_San", "Ten_Khach_San");
                return View(newAccount);
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult UpdateAccount(string id) //Cập nhật thông tin tài khoản - Admin
        {
            if (Session["user-role"].Equals("Admin")) //Tài khoản thuộc quyền Admin
            {
                var taikhoan = model.Tai_Khoan.FirstOrDefault(t => t.Ma_Tai_Khoan.Equals(id));
                ViewBag.Ma_Quyen = new SelectList(model.Quyen, "Ma_Quyen", "Ten_Quyen");
                ViewBag.Ma_Khach_San = new SelectList(model.Khach_San, "Ma_Khach_San", "Ten_Khach_San");
                return View(taikhoan);
            }
            return RedirectToAction("Homepage", "Home");
        }
        [HttpPost]
        public ActionResult UpdateAccount(Tai_Khoan taikhoan, string id)
        {
            if (Session["user-role"].Equals("Admin")) //Tài khoản thuộc quyền Admin
            {
                if (ModelState.IsValid)
                {
                    try
                    {
                        model.Entry(taikhoan).State = EntityState.Modified;
                        model.SaveChanges();
                        return RedirectToAction("Home");
                    }
                    catch (DbEntityValidationException e)
                    {
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                eve.Entry.Entity.GetType().Name, eve.Entry.State);
                            foreach (var ve in eve.ValidationErrors)
                            {
                                Console.WriteLine("- Property: \"{0}\", Value: \"{1}\", Error: \"{2}\"",
                                    ve.PropertyName,
                                    eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName),
                                    ve.ErrorMessage);
                            }
                        }
                        throw;
                    }
                }
                ViewBag.Ma_Quyen = new SelectList(model.Quyen, "Ma_Quyen", "Ten_Quyen");
                ViewBag.Ma_Khach_San = new SelectList(model.Khach_San, "Ma_Khach_San", "Ten_Khach_San");
                return View(taikhoan);
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult DetailtAccount(string id) //Xem thông tin chi tiết của các tài khoản trên hệ thống - Admin
        {
            if (Session["user-role"].Equals("Admin")) //Tài khoản thuộc quyền Admin
            {
                var taikhoan = model.Tai_Khoan.Find(id);
                if (taikhoan != null)
                    return View(taikhoan);
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult Information() //Xem thông tin cá nhân - Admin và Nhân viên
        {
            string id = Session["user-id"].ToString();
            var taikhoan = model.Tai_Khoan.Find(id);
            if (taikhoan != null)
                return View(taikhoan);
            return RedirectToAction("Home");
        }
        public ActionResult RemoveAccount(string id)
        {
            Session["taikhoan-user-deleted"] = null;
            Session["taikhoan-admintop1-deleted"] = null;
            if (Session["user-role"].ToString().Equals("Admin"))
            {
                var taikhoan = model.Tai_Khoan.FirstOrDefault(t => t.Ma_Tai_Khoan.Equals(id));
                if (taikhoan != null)
                {
                    if (taikhoan.Quyen.Ten_Quyen.Equals("Admin") && !taikhoan.Ten_Dang_Nhap.Equals("admin"))
                    {
                        model.Tai_Khoan.Remove(taikhoan);
                        model.SaveChanges();
                        return View("Home");
                    }
                    else if (taikhoan.Ten_Dang_Nhap.Equals("admin"))
                    {
                        //Khong duoc phep xoa tk chu khach san
                        Session["taikhoan-admintop1-deleted"] = true;
                        return View("Home");
                    }
                    //Khong duoc phep xoa tk user
                    Session["taikhoan-user-deleted"] = true;
                }
            }
            return RedirectToAction("Homepage", "Home");
        }
    }
}