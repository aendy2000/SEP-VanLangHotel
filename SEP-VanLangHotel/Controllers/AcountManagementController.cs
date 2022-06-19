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
    public class AcountManagementController : Controller
    {
        // GET: AcountManagement
        VanLangHotelEntities model = new VanLangHotelEntities();
        public ActionResult Home()
        {
            if (Session["user-role"].Equals("Admin"))
            {
                var taikhoan = model.Tai_Khoan.ToList();
                return View(taikhoan);
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult AddNewAccount()
        {
            if (Session["user-role"].Equals("Admin"))
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
            if (Session["user-role"].Equals("Admin"))
            {
                if (ModelState.IsValid)
                {
                    if (Session["user-role"].Equals("Admin"))
                    {
                        try
                        {
                            newAccount.Ma_Tai_Khoan = "1";
                            model.Tai_Khoan.Add(newAccount);
                            model.SaveChanges();
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
                        return RedirectToAction("Home");
                    }
                }
                ViewBag.Ma_Quyen = new SelectList(model.Quyen, "Ma_Quyen", "Ten_Quyen");
                ViewBag.Ma_Khach_San = new SelectList(model.Khach_San, "Ma_Khach_San", "Ten_Khach_San");
                return View(newAccount);
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult UpdateAccount(string id)
        {
            if (Session["user-role"].Equals("Admin"))
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
            if (Session["user-role"].Equals("Admin"))
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
            }
            ViewBag.Ma_Quyen = new SelectList(model.Quyen, "Ma_Quyen", "Ten_Quyen");
            ViewBag.Ma_Khach_San = new SelectList(model.Khach_San, "Ma_Khach_San", "Ten_Khach_San");
            return View(taikhoan);
        }
        public ActionResult DetailtAccount(string id)
        {
            if (Session["user-role"].Equals("Admin"))
            {
                var taikhoan = model.Tai_Khoan.Find(id);
                if (taikhoan != null)
                    return View(taikhoan);
            }
            return RedirectToAction("Home");
        }
        public ActionResult Information()
        {
            if (Session["user-role"].Equals("Staff"))
            {
                string id = Session["user-id"].ToString();
                var taikhoan = model.Tai_Khoan.Find(id);
                if (taikhoan != null)
                    return View(taikhoan);
            }
            return RedirectToAction("Homepage", "Home");
        }
    }
}