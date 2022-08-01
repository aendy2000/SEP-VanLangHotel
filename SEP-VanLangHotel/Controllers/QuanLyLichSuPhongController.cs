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
    public class QuanLyLichSuPhongController : Controller
    {
        SEP25Team09Entities model = new SEP25Team09Entities();
        //trang chu lịch sử phòng khách sạn
        public ActionResult Home()
        {
            var ltshistory = model.TT_Dat_Phong.Where(s => s.Trang_Thai == 1).ToList();
            return View(ltshistory);
            
        }

    }

}