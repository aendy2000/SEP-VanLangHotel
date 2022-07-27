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
                        var tienich = model.Tien_Ich.ToList();
                        foreach (var item in tienich)
                        {
                            if (item.Ten_Tien_Ich.ToLower().Equals(tentienich.ToLower().Trim()))
                            {
                                Session["thongbao-loi"] = "Tiện ích đã tồn tại!";
                                return RedirectToAction("Home");
                            }
                        }
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
        [HttpPost]
        public ActionResult UpdateUtilities(string matienich, string tentienich)
        {
            if (Session["user-role"].Equals("Quản lý"))
            {
                if (!string.IsNullOrEmpty(matienich) && !string.IsNullOrEmpty(tentienich))
                {
                    var tienIch = model.Tien_Ich.First(t => t.Ma_Tien_Ich.Equals(matienich));
                    if (tienIch != null)
                    {
                        try
                        {
                            var lstTienIch = model.Tien_Ich.Where(t => !t.Ma_Tien_Ich.Equals(matienich)).ToList();
                            bool checkTienIch = false;
                            foreach (var tienIchs in lstTienIch)
                            {
                                if (tienIchs.Ten_Tien_Ich.ToLower().Trim().Equals(tentienich.ToLower().Trim()))
                                    checkTienIch = true;
                            }

                            if (checkTienIch == true)
                            {
                                Session["thongbao-loi"] = "Tiện ích này đã tồn tại!";
                                return RedirectToAction("Home");
                            }
                            else
                            {
                                tienIch.Ten_Tien_Ich = tentienich;
                                model.Entry(tienIch).State = EntityState.Modified;
                                model.SaveChanges();
                                Session["thongbaoSuccess"] = "Chỉnh sửa thành công!";
                                return RedirectToAction("Home");
                            }
                        }
                        catch (Exception e)
                        {
                            Session["thongbao-loi"] = e.Message;
                            return RedirectToAction("Home");
                        }
                    }
                }
                return RedirectToAction("Home");
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult DeleteUtilities(string id)
        {
            if (Session["user-role"].Equals("Quản lý"))
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var tienIch = model.Tien_Ich.First(t => t.Ma_Tien_Ich.Equals(id));
                    if (tienIch != null)
                    {
                        string tentienIch = tienIch.Ten_Tien_Ich;
                        model.Tien_Ich.Remove(tienIch);
                        model.SaveChanges();
                        Session["thongbaoSuccess"] = "Đã xóa tiện ích " + tentienIch;
                        return RedirectToAction("Home");
                    }
                }
                return RedirectToAction("Home");
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult SearchUtilities(string keyword) //Tìm kiếm tiện ích
        {
            if (Session["user-role"].Equals("Quản lý")) //Tài khoản thuộc quyền Admin
            {
                if (keyword.Equals("") || keyword == null)
                {
                    var tienich = model.Tien_Ich.ToList();
                    return View("Home", tienich);
                }
                else
                {
                    var tienich = model.Tien_Ich.Where(t => t.Ten_Tien_Ich.ToUpper().Contains(keyword.ToUpper())).ToList();
                    ViewBag.Keywork = keyword;
                    return View("Home", tienich);
                }
            }
            return RedirectToAction("Homepage", "Home");
        }
    }
}
