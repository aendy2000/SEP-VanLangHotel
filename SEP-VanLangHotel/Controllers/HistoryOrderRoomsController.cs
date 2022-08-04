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
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.Globalization;
using System.Threading;

namespace SEP_VanLangHotel.Controllers
{
    [LoginVerification]
    public class HistoryOrderRoomsController : Controller
    {
        SEP25Team09Entities model = new SEP25Team09Entities();
        // GET: HistoryOrderRooms
        public ActionResult Home()
        {
            var history = model.TT_Dat_Phong.Where(t => t.Trang_Thai == 1).ToList();
            return View(history);
        }
    }
}