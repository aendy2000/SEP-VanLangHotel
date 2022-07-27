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
    public class QuanLyTrangThaiController : Controller
    {
        SEP25Team09Entities model = new SEP25Team09Entities();
        // Trang chủ Qli Trạng thái
        public ActionResult Home()
        {
            if (Session["user-role"].Equals("Quản lý"))
            {
                var ltstrangThai = model.Trang_Thai.ToList();
                return View(ltstrangThai);
            }
            return RedirectToAction("Homepage", "Home");

        }
        [HttpPost]
        public ActionResult AddNewStatus(string tentrangthai, string motatrangthai)
        {
            if (Session["user-role"].Equals("Quản lý"))
            {
                if (!string.IsNullOrEmpty(tentrangthai))
                {
                    try
                    {
                        var trangthai = model.Trang_Thai.ToList();
                        foreach (var item in trangthai)
                        {
                            if (item.Ten_Trang_Thai.ToLower().Equals(tentrangthai.ToLower().Trim()))
                            {
                                Session["thongbao-loi"] = "Trạng thái đã tồn tại!";
                                return RedirectToAction("Home");
                            }
                        }
                        Trang_Thai newTrangThai = new Trang_Thai();
                        newTrangThai.Ma_Trang_Thai = "1";
                        newTrangThai.Ten_Trang_Thai = tentrangthai;
                        newTrangThai.Mo_Ta_Trang_Thai = motatrangthai;
                        model.Trang_Thai.Add(newTrangThai);
                        model.SaveChanges();
                        Session["thongbaoSuccess"] = "Thêm thành công trạng thái: " + newTrangThai.Ten_Trang_Thai;
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
        public ActionResult UpdateStatus(string matrangthai, string tentrangthai, string motatrangthai)
        {
            if (Session["user-role"].Equals("Quản lý"))
            {
                if (!string.IsNullOrEmpty(matrangthai) && !string.IsNullOrEmpty(tentrangthai) && !string.IsNullOrEmpty(motatrangthai))
                {
                    var trangThai = model.Trang_Thai.First(t => t.Ma_Trang_Thai.Equals(matrangthai));
                        if (trangThai != null)
                    {
                        try
                        {
                            var ltsTrangThai = model.Trang_Thai.Where(t => !t.Ma_Trang_Thai.Equals(matrangthai)).ToList();
                            bool checkTrangThai = false;
                            foreach (var trangthai in ltsTrangThai)
                            {
                                if (trangthai.Ten_Trang_Thai.ToLower().Trim().Equals(tentrangthai.ToLower().Trim()))
                                    checkTrangThai = true;
                            }
                            if (checkTrangThai == true)
                            {
                                Session["thongbao-loi"] = "Trạng thái này đã tồn tại!";
                                return RedirectToAction("Home");
                            }
                            else
                            {
                                trangThai.Ten_Trang_Thai = tentrangthai;
                                trangThai.Mo_Ta_Trang_Thai = motatrangthai;
                                model.Entry(trangThai).State = EntityState.Modified;
                                model.SaveChanges();
                                Session["thongbaoSuccess"] = "Chỉnh sửa trạng thái thành công!";
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
        public ActionResult DeleteStatus(string matrangthai)
        {
            if (Session["user-role"].Equals("Quản lý"))
            {                               
                if (!string.IsNullOrEmpty(matrangthai))
                {
                    var trangThai = model.Trang_Thai.First(t => t.Ma_Trang_Thai.Equals(matrangthai));
                    if(trangThai != null)
                    {                        
                        string tentrangThai = trangThai.Ten_Trang_Thai;
                        model.Trang_Thai.Remove(trangThai);
                        model.SaveChanges();
                        Session["thongbaoSuccess"] = "Đã xóa trạng thái " + tentrangThai;
                        return RedirectToAction("Home");
                    }                   
                }
                return RedirectToAction("Home");
            }
            return RedirectToAction("Homepage", "Home");
        }
    }
}