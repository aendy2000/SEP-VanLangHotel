using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using SEP_VanLangHotel.Models;
using SEP_VanLangHotel.Middleware;
using System.Web;
using System.IO;
using System.Configuration;
using System.Data.OleDb;
using System.Data;
using System.Globalization;
using System.Data.Entity;

namespace SEP_VanLangHotel.Controllers
{
    [LoginVerification]
    public class HomeController : Controller
    {
        SEP25Team09Entities model = new SEP25Team09Entities();
        public ActionResult Homepage()
        {
            //Nếu Là tài khoản của nhân viên
            if (Session["user-role"].Equals("Nhân viên"))
            {
                List<Tien_Ich> dstienIch = new List<Tien_Ich>();
                foreach (var item in model.Tien_Ich.ToList())
                {
                    if(!item.Ma_Tien_Ich.Equals("TI202207070001"))
                    {
                        var matienIch = item.Ma_Tien_Ich;
                        var tentienIch = item.Ten_Tien_Ich;
                        dstienIch.Add(new Tien_Ich() { Ma_Tien_Ich = matienIch, Ten_Tien_Ich = tentienIch, IsChecks = false });
                    }
                }
                ListTienIch tienichList = new ListTienIch();
                tienichList.tienIch = dstienIch;

                return View(tienichList);
            }
            return RedirectToAction("Dashboard"); //Nếu Là tài khoản của admin sẽ đi đến hàm Dashboard
        }

        //Tài khoản admin sẽ được đi đến trang dashboard
        public ActionResult Dashboard()
        {
            //Return với tổng doanh thu của khách sạn
            if (Session["user-role"].Equals("Quản lý"))
                return View("Dashboard", Convert.ToDecimal(1000000));
            return RedirectToAction("Homepage");

        }
    }
}