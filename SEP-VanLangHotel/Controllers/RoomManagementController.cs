using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SEP_VanLangHotel.Models;
using SEP_VanLangHotel.Middleware;
using System.Data.Entity.Validation;
using System.Data.Entity;
using System.IO;
using Firebase.Auth;
using System.Threading;
using Firebase.Storage;
using System.Threading.Tasks;
using System.Text;

namespace SEP_VanLangHotel.Controllers
{
    [LoginVerification]
    public class RoomManagementController : Controller
    {
        SEP25Team09Entities model = new SEP25Team09Entities();

        //Phòng trống
        public ActionResult ListOfEmptyRooms()
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                var phongTrong = model.Phong.Where(p => p.Ma_Trang_Thai.Equals("TT202207050001") || p.Ma_Trang_Thai.Equals("TT202207050004")).ToList();
                return View(phongTrong);
            }
            return RedirectToAction("Homepage", "Home");

        }
        public ActionResult DetailtEmptyRooms(string id)
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if (!string.IsNullOrEmpty(id))
                {
                    ViewBag.Ma_Trang_Thai = new SelectList(model.Trang_Thai.Where(t => !t.Ma_Trang_Thai.Equals("TT202207110001") && !t.Ma_Trang_Thai.Equals("TT202207050003")), "Ma_Trang_Thai", "Ten_Trang_Thai");
                    var phong = model.Phong.Where(p => p.Ma_Phong.Equals(id) && (p.Ma_Trang_Thai.Equals("TT202207050001") || p.Ma_Trang_Thai.Equals("TT202207050004"))).First();
                    return View(phong);
                }
                return RedirectToAction("ListOfEmptyRooms");
            }
            return RedirectToAction("Homepage", "Home");
        }
        [HttpPost]
        public ActionResult DetailtEmptyRooms(Phong phong)
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if (phong != null)
                {
                    if (ModelState.IsValid)
                    {
                        try
                        {
                            model.Entry(phong).State = EntityState.Modified;
                            model.SaveChanges();
                            ViewBag.Ma_Trang_Thai = new SelectList(model.Trang_Thai.Where(t => !t.Ma_Trang_Thai.Equals("TT202207110001") && !t.Ma_Trang_Thai.Equals("TT202207050003")), "Ma_Trang_Thai", "Ten_Trang_Thai");
                            return RedirectToAction("DetailtEmptyRooms", new { id = phong.Ma_Phong });
                        }
                        catch (Exception e)
                        {
                            Session["error-import-file"] = "Lỗi " + e.Message;
                            ViewBag.Ma_Trang_Thai = new SelectList(model.Trang_Thai.Where(t => !t.Ma_Trang_Thai.Equals("TT202207110001") && !t.Ma_Trang_Thai.Equals("TT202207050003")), "Ma_Trang_Thai", "Ten_Trang_Thai");
                            return View(phong);
                        }
                    }
                    ViewBag.Ma_Trang_Thai = new SelectList(model.Trang_Thai.Where(t => !t.Ma_Trang_Thai.Equals("TT202207110001") && !t.Ma_Trang_Thai.Equals("TT202207050003")), "Ma_Trang_Thai", "Ten_Trang_Thai");
                    return View(phong);
                }
                return RedirectToAction("ListOfEmptyRooms");
            }
            return RedirectToAction("Homepage", "Home");
        }

        //Phòng đang cho thuê
        public ActionResult ListOfRentingRooms()
        {
            var phongDangChoThue = model.TT_Dat_Phong.Where(p => p.Phong.Ma_Trang_Thai.Equals("TT202207110001")).ToList();
            return View(phongDangChoThue);
        }
        //Xem chi tiết phòng đang cho thuê
        public ActionResult DetailtRentingRooms(string id)
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var phong = model.Phong.FirstOrDefault(p => p.Ma_Phong.Equals(id));
                    var ttdatPhong = model.TT_Dat_Phong.FirstOrDefault(t => t.Ma_Phong.Equals(id));

                    var mattDatPhong = ttdatPhong.Ma_TT_Dat_Phong;
                    var ttNhanThan = model.Nhan_Than.Where(tt => tt.Ma_TT_Dat_Phong.Equals(mattDatPhong)).ToList();

                    Session["thongtindatphong"] = ttdatPhong;
                    Session["thongtinphong"] = phong;

                    Session["thoigian-den"] = ttdatPhong.Thoi_Gian_Dat;
                    Session["thoigian-ve"] = ttdatPhong.Thoi_Gian_Doi_Tra;

                    Session["songuoi-lon"] = ttdatPhong.Nguoi_Lon;
                    Session["songuoi-treem"] = ttdatPhong.Tre_Em;
                    Session["sogiuong"] = phong.Loai_Phong.So_Giuong;

                    int songay = (int)ttdatPhong.Thoi_Gian_Doi_Tra.Subtract(ttdatPhong.Thoi_Gian_Dat).TotalDays + 1;
                    Session["tong-thanhtoan"] = ttdatPhong.Tong_Thanh_Toan;
                    Session["tong-coc"] = ttdatPhong.Tien_Coc;

                    List<string> gioitinhA = new List<string>();
                    gioitinhA.Add("Nam");
                    gioitinhA.Add("Nữ");
                    gioitinhA.Add("Khác");
                    ViewBag.Gioi_TinhNam = new SelectList(gioitinhA);

                    List<string> gioitinhB = new List<string>();
                    gioitinhB.Add("Nữ");
                    gioitinhB.Add("Nam");
                    gioitinhB.Add("Khác");
                    ViewBag.Gioi_TinhNu = new SelectList(gioitinhB);

                    List<string> gioitinhC = new List<string>();
                    gioitinhC.Add("Khác");
                    gioitinhC.Add("Nam");
                    gioitinhC.Add("Nữ");
                    ViewBag.Gioi_TinhKhac = new SelectList(gioitinhC);

                    return View(ttNhanThan);
                }
                return RedirectToAction("ListOfRentingRooms");
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult UpdateRentingRooms(string id)
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if (!string.IsNullOrEmpty(id))
                {

                    List<Gioi_Tinh> gioiTinhNe0 = new List<Gioi_Tinh>();
                    gioiTinhNe0.Add(new Gioi_Tinh());
                    gioiTinhNe0[0].ID = 0;
                    gioiTinhNe0[0].Name = "Nữ";
                    gioiTinhNe0.Add(new Gioi_Tinh());
                    gioiTinhNe0[1].ID = 1;
                    gioiTinhNe0[1].Name = "Nam";
                    gioiTinhNe0.Add(new Gioi_Tinh());
                    gioiTinhNe0[2].ID = 2;
                    gioiTinhNe0[2].Name = "Khác";
                    ViewBag.Gioi_Tinh0 = new SelectList(gioiTinhNe0, "ID", "Name");

                    List<Gioi_Tinh> gioiTinhNe1 = new List<Gioi_Tinh>();
                    gioiTinhNe1.Add(new Gioi_Tinh());
                    gioiTinhNe1[0].ID = 0;
                    gioiTinhNe1[0].Name = "Nam";
                    gioiTinhNe1.Add(new Gioi_Tinh());
                    gioiTinhNe1[1].ID = 1;
                    gioiTinhNe1[1].Name = "Nữ";
                    gioiTinhNe1.Add(new Gioi_Tinh());
                    gioiTinhNe1[2].ID = 2;
                    gioiTinhNe1[2].Name = "Khác";
                    ViewBag.Gioi_Tinh1 = new SelectList(gioiTinhNe1, "ID", "Name");

                    List<Gioi_Tinh> gioiTinhNe2 = new List<Gioi_Tinh>();
                    gioiTinhNe2.Add(new Gioi_Tinh());
                    gioiTinhNe2[0].ID = 0;
                    gioiTinhNe2[0].Name = "Khác";
                    gioiTinhNe2.Add(new Gioi_Tinh());
                    gioiTinhNe2[1].ID = 1;
                    gioiTinhNe2[1].Name = "Nam";
                    gioiTinhNe2.Add(new Gioi_Tinh());
                    gioiTinhNe2[2].ID = 2;
                    gioiTinhNe2[2].Name = "Nữ";
                    ViewBag.Gioi_Tinh2 = new SelectList(gioiTinhNe2, "ID", "Name");

                    var phong = model.Phong.FirstOrDefault(p => p.Ma_Phong.Equals(id));
                    var ttdatPhong = model.TT_Dat_Phong.FirstOrDefault(t => t.Ma_Phong.Equals(id));

                    var mattDatPhong = ttdatPhong.Ma_TT_Dat_Phong;
                    var ttNhanThan = model.Nhan_Than.Where(tt => tt.Ma_TT_Dat_Phong.Equals(mattDatPhong)).ToList();

                    Session["thongtindatphong"] = ttdatPhong;
                    Session["thongtinphong"] = phong;

                    Session["thoigian-den"] = ttdatPhong.Thoi_Gian_Dat;
                    Session["thoigian-ve"] = ttdatPhong.Thoi_Gian_Doi_Tra;

                    Session["songuoi-lon"] = ttdatPhong.Nguoi_Lon;
                    Session["songuoi-treem"] = ttdatPhong.Tre_Em;
                    Session["sogiuong"] = phong.Loai_Phong.So_Giuong;

                    int songay = (int)ttdatPhong.Thoi_Gian_Doi_Tra.Subtract(ttdatPhong.Thoi_Gian_Dat).TotalDays + 1;
                    Session["tong-thanhtoan"] = ttdatPhong.Tong_Thanh_Toan;
                    Session["tong-coc"] = ttdatPhong.Tien_Coc;

                    return View(ttNhanThan);
                }
                return RedirectToAction("ListOfRentingRooms");
            }
            return RedirectToAction("Homepage", "Home");
        }
        [HttpPost]
        public ActionResult UpdateRentingRooms(List<Nhan_Than> lstNhanThan, string maTTDatPhong, string hoten, string cmndcccd,
            string sdt, DateTime ngaysinh, int gioiTinh, string diachi)
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if (lstNhanThan != null)
                {
                    try
                    {
                        var ttDatPhong = model.TT_Dat_Phong.FirstOrDefault(t => t.Ma_TT_Dat_Phong.Equals(maTTDatPhong));
                        ttDatPhong.Ho_Ten_KH = hoten;
                        ttDatPhong.CMND_CCCD_KH = cmndcccd;
                        ttDatPhong.SDT_KH = sdt;
                        ttDatPhong.Sinh_Nhat_KH = ngaysinh;
                        ttDatPhong.Gioi_Tinh_KH = gioiTinh;
                        ttDatPhong.Dia_Chi_KH = diachi;

                        model.Entry(ttDatPhong).State = EntityState.Modified;
                        model.SaveChanges();

                        foreach (var item in lstNhanThan)
                        {
                            model.Entry(item).State = EntityState.Modified;
                            model.SaveChanges();
                        }
                        return RedirectToAction("DetailtRentingRooms", new { id = ttDatPhong.Ma_Phong });
                    }
                    catch (Exception e)
                    {
                        return RedirectToAction("ListOfRentingRooms");
                    }
                }
            }
            return RedirectToAction("Homepage", "Home");
        }

        //Danh sách phòng đến hạn
        public ActionResult ListRoomDueToPay()
        {
            DateTime ngayhientai = DateTime.Now;
            var phongDangChoThue = model.TT_Dat_Phong.Where(p => DateTime.Compare(p.Thoi_Gian_Doi_Tra, ngayhientai) < 1 && p.Phong.Ma_Trang_Thai.Equals("TT202207110001")).ToList();
            return View(phongDangChoThue);
        }
    }
}