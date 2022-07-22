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
            if (Session["user-role"].Equals("Nhân viên"))
            {
                var phongDangChoThue = model.TT_Dat_Phong.Where(p => p.Trang_Thai == 0).ToList();
                return View(phongDangChoThue);
            }
            return RedirectToAction("Homepage", "Home");
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
                    ViewBag.Gioi_Tinh = new SelectList(gioitinhA);

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
        public ActionResult UpdateRentingRooms(string id) //mã ttdatphong
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if (!string.IsNullOrEmpty(id))
                {
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

                    var ttdatPhong = model.TT_Dat_Phong.FirstOrDefault(t => t.Ma_TT_Dat_Phong.Equals(id));
                    var phong = ttdatPhong.Phong;
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
            string sdt, DateTime ngaysinh, string gioiTinh, string diachi)
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
                        ttDatPhong.Dia_Chi_KH = diachi;

                        if (gioiTinh.Equals("Nam"))
                            ttDatPhong.Gioi_Tinh_KH = 1;
                        else if (gioiTinh.Equals("Nữ"))
                            ttDatPhong.Gioi_Tinh_KH = 0;
                        else
                            ttDatPhong.Gioi_Tinh_KH = 2;


                        model.Entry(ttDatPhong).State = EntityState.Modified;
                        model.SaveChanges();

                        foreach (var item in lstNhanThan)
                        {
                            string gioitinhs = item.Gioi_Tinh_Name;
                            if (gioitinhs.Equals("Nam"))
                                item.Gioi_Tinh = 1;
                            else if (gioitinhs.Equals("Nữ"))
                                item.Gioi_Tinh = 0;
                            else
                                item.Gioi_Tinh = 2;

                            model.Entry(item).State = EntityState.Modified;
                            model.SaveChanges();
                        }
                        Session["thongbaoSuccess"] = "Chỉnh sửa thành công!";
                        return RedirectToAction("DetailtRentingRooms", new { id = ttDatPhong.Ma_Phong });
                    }
                    catch (Exception e)
                    {
                        Session["error-import-file"] = e.Message;
                        return RedirectToAction("ListOfRentingRooms");
                    }
                }
            }
            return RedirectToAction("Homepage", "Home");
        }

        public ActionResult AddMember(string hoten, string cmndcccd, string moiquanhe, DateTime ngaysinh,
            string gioiTinh, string diachi, string mattdatphong)
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if (!string.IsNullOrEmpty(mattdatphong))
                {
                    try
                    {
                        Nhan_Than newNhanThan = new Nhan_Than();
                        string maNhanThan = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        newNhanThan.Ma_Nhan_Than = maNhanThan;
                        newNhanThan.Ho_Ten = hoten;
                        newNhanThan.CMND_CCCD = cmndcccd;
                        newNhanThan.Sinh_Nhat = ngaysinh;

                        if (gioiTinh.Equals("Nữ"))
                            newNhanThan.Gioi_Tinh = 0;
                        else if (gioiTinh.Equals("Nam"))
                            newNhanThan.Gioi_Tinh = 1;
                        else
                            newNhanThan.Gioi_Tinh = 2;
                        newNhanThan.Dia_Chi = diachi;
                        newNhanThan.Moi_Quan_He = moiquanhe;
                        newNhanThan.Ma_TT_Dat_Phong = mattdatphong;
                        model.Nhan_Than.Add(newNhanThan);
                        model.SaveChanges();

                        var TTDatPhong = model.TT_Dat_Phong.FirstOrDefault(m => m.Ma_TT_Dat_Phong.Equals(mattdatphong));
                        var phong = TTDatPhong.Phong;

                        if (DateTime.Now.Year - ngaysinh.Year <= 14)
                            TTDatPhong.Tre_Em = TTDatPhong.Tre_Em.Value + 1;
                        else
                            TTDatPhong.Nguoi_Lon = TTDatPhong.Nguoi_Lon.Value + 1;

                        if ((TTDatPhong.Tre_Em + TTDatPhong.Nguoi_Lon) <= (phong.Loai_Phong.So_Nguoi_Lon + phong.Loai_Phong.So_Tre_Em))
                        {
                            model.Entry(TTDatPhong).State = EntityState.Modified;
                            model.SaveChanges();
                            Session["thongbaoSuccess"] = "Thêm thành công!";
                        }
                        else
                        {
                            int songay = (int)(TTDatPhong.Thoi_Gian_Doi_Tra.Subtract(TTDatPhong.Thoi_Gian_Dat).TotalDays) + 1;
                            decimal phiPhuThu = 100000 * songay;
                            decimal tongThanhToan = TTDatPhong.Tong_Thanh_Toan.Value;
                            TTDatPhong.Tong_Thanh_Toan = phiPhuThu + tongThanhToan;
                            model.Entry(TTDatPhong).State = EntityState.Modified;
                            model.SaveChanges();
                            Session["thongbaoSuccess"] = "Thêm thành công, phí phụ thu là 100.000đ/ngày đã được cộng vào tổng thanh toán!";
                        }
                        var maPhong = TTDatPhong.Ma_Phong;

                        return RedirectToAction("DetailtRentingRooms", new { id = maPhong });
                    }
                    catch (Exception e)
                    {
                        Session["error-import-file"] = e.Message;
                        var maPhong = model.TT_Dat_Phong.First(p => p.Ma_TT_Dat_Phong.Equals(mattdatphong)).Ma_Phong;
                        return RedirectToAction("DetailtRentingRooms", new { id = maPhong });
                    }
                }
                return RedirectToAction("ListOfRentingRooms");
            }
            return RedirectToAction("Homepage", "Home");
        }

        public ActionResult ChangeRooms(string id) //Mã TT dat phòng
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if(!string.IsNullOrEmpty(id))
                {
                    var TTDatPhong = model.TT_Dat_Phong.First(t => t.Ma_TT_Dat_Phong.Equals(id));
                    var maLoaiPhong = TTDatPhong.Phong.Loai_Phong.Ma_Loai_Phong;
                    var maPhong = TTDatPhong.Ma_Phong;
                    var lstPhong = model.Phong.Where(p => p.Ma_Loai_Phong.Equals(maLoaiPhong) && !p.Ma_Phong.Equals(maPhong)).ToList();
                    
                    Session["thongtindatphong"] = TTDatPhong;
                    Session["thoigian-den"] = TTDatPhong.Thoi_Gian_Dat;
                    Session["thoigian-ve"] = TTDatPhong.Thoi_Gian_Doi_Tra;
                    Session["songuoi-lon"] = TTDatPhong.Nguoi_Lon;
                    Session["songuoi-treem"] = TTDatPhong.Tre_Em;
                    Session["tong-thanhtoan"] = TTDatPhong.Tong_Thanh_Toan;
                    Session["tong-coc"] = TTDatPhong.Tien_Coc;

                    return View(lstPhong);
                }
                return RedirectToAction("ListOfRentingRooms");
            }
            return RedirectToAction("Homepage", "Home");
        }

        //Danh sách phòng đến hạn
        public ActionResult ListRoomDueToPay()
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                DateTime ngayhientai = DateTime.Now;
                var phongDangChoThue = model.TT_Dat_Phong.Where(p => DateTime.Compare(p.Thoi_Gian_Doi_Tra, ngayhientai) < 0 && p.Trang_Thai == 0).ToList();
                return View(phongDangChoThue);
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult CheckOutRooms(string id)
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if (!string.IsNullOrEmpty(id))
                {
                    try
                    {
                        var TTDatPhong = model.TT_Dat_Phong.First(t => t.Ma_TT_Dat_Phong.Equals(id));
                        TTDatPhong.Trang_Thai = 1; //Đã hoàn thành
                        model.Entry(TTDatPhong).State = EntityState.Modified;
                        model.SaveChanges();

                        var phongs = TTDatPhong.Phong;
                        phongs.Ma_Trang_Thai = "TT202207050001";
                        model.Entry(phongs).State = EntityState.Modified;
                        model.SaveChanges();
                        Session["thongbaoSuccess"] = "Trả phòng thành công!";
                        return RedirectToAction("Homepage", "Home");
                        //return RedirectToAction("DetailtHistory", new { id = id });
                    }
                    catch (Exception e)
                    {
                        Session["error-import-file"] = e.Message;
                        var maPhong = model.TT_Dat_Phong.First(p => p.Ma_TT_Dat_Phong.Equals(id)).Ma_Phong;
                        return RedirectToAction("DetailtRentingRooms", new { id = maPhong });
                    }
                }
            }
            return RedirectToAction("Homepage", "Home");
        }
    }
}