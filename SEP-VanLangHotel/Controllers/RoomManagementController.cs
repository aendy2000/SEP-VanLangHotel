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
                var phongTrong = model.Phong.Where(p => p.Ma_Trang_Thai.Equals("TT202207050001") || p.Ma_Trang_Thai.Equals("TT202207050003")).ToList();
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
                    ViewBag.Ma_Trang_Thai = new SelectList(model.Trang_Thai.Where(t => !t.Ma_Trang_Thai.Equals("TT202207050002")), "Ma_Trang_Thai", "Ten_Trang_Thai");
                    var phong = model.Phong.FirstOrDefault(p => p.Ma_Phong.Equals(id) && !p.Ma_Trang_Thai.Equals("TT202207050002"));
                    if(phong != null)
                    {
                        return View(phong);
                    }
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
                            ViewBag.Ma_Trang_Thai = new SelectList(model.Trang_Thai.Where(t => !t.Ma_Trang_Thai.Equals("TT202207050002")), "Ma_Trang_Thai", "Ten_Trang_Thai");
                            Session["thongbaoSuccess"] = "Đã thay đổi trạng thái phòng!";
                            return RedirectToAction("DetailtEmptyRooms", new { id = phong.Ma_Phong });
                        }
                        catch (Exception e)
                        {
                            Session["error-import-file"] = "Lỗi " + e.Message;
                            ViewBag.Ma_Trang_Thai = new SelectList(model.Trang_Thai.Where(t => !t.Ma_Trang_Thai.Equals("TT202207050002")), "Ma_Trang_Thai", "Ten_Trang_Thai");
                            return View(phong);
                        }
                    }
                    Session["error-import-file"] = "Đã có lỗi xảy ra, vui lòng thử lại sau!";
                    ViewBag.Ma_Trang_Thai = new SelectList(model.Trang_Thai.Where(t => !t.Ma_Trang_Thai.Equals("TT202207050002")), "Ma_Trang_Thai", "Ten_Trang_Thai");
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
                var phongDangChoThue = model.TT_Dat_Phong.Where(p => p.Trang_Thai == 0 && p.Phong.Ma_Trang_Thai.Equals("TT202207050002")).ToList();
                Session["TT_DOI_PHONG"] = model.TT_Doi_Phong.ToList();
                return View(phongDangChoThue);
            }
            return RedirectToAction("Homepage", "Home");
        }
        //Xem chi tiết phòng đang cho thuê
        public ActionResult DetailtRentingRooms(string id) //Ma tt dat phòng
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var ttdatPhong = model.TT_Dat_Phong.FirstOrDefault(t => t.Ma_TT_Dat_Phong.Equals(id));
                    int landoi = ttdatPhong.Doi_Tra;
                    if (landoi == 0)
                    {
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
                    else
                    {
                        var ttdoiphong = model.TT_Doi_Phong.First(t => t.Ma_TT_Dat_Phong.Equals(id) && t.Lan_Doi == landoi);
                        var phong = ttdoiphong.Phong;

                        var mattDatPhong = ttdatPhong.Ma_TT_Dat_Phong;
                        var ttNhanThan = model.Nhan_Than.Where(tt => tt.Ma_TT_Dat_Phong.Equals(mattDatPhong)).ToList();

                        Session["thongtindatphong"] = ttdatPhong;
                        Session["thongtinphong"] = phong;

                        Session["thoigian-den"] = ttdatPhong.Thoi_Gian_Dat;
                        Session["thoigian-ve"] = ttdoiphong.TG_Doi_Tra;

                        Session["songuoi-lon"] = ttdatPhong.Nguoi_Lon;
                        Session["songuoi-treem"] = ttdatPhong.Tre_Em;
                        Session["sogiuong"] = phong.Loai_Phong.So_Giuong;

                        int songay = (int)ttdoiphong.TG_Doi_Tra.Subtract(ttdatPhong.Thoi_Gian_Dat).TotalDays + 1;
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
                    int landoi = ttdatPhong.Doi_Tra;
                    Session["thoigian-ve"] = ttdatPhong.Thoi_Gian_Doi_Tra;

                    if (landoi != 0)
                    {
                        var ttdoiphong = model.TT_Doi_Phong.First(t => t.Ma_TT_Dat_Phong.Equals(id) && t.Lan_Doi == landoi);
                        phong = ttdoiphong.Phong;
                        Session["thoigian-ve"] = ttdoiphong.TG_Doi_Tra;
                    }

                    var mattDatPhong = ttdatPhong.Ma_TT_Dat_Phong;
                    var ttNhanThan = model.Nhan_Than.Where(tt => tt.Ma_TT_Dat_Phong.Equals(mattDatPhong)).ToList();

                    Session["thongtindatphong"] = ttdatPhong;
                    Session["thongtinphong"] = phong;

                    Session["thoigian-den"] = ttdatPhong.Thoi_Gian_Dat;

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
                try
                {
                    if (cmndcccd.Length != 9 && cmndcccd.Length != 12)
                    {
                        Session["error-import-file"] = "Số CMND/CCCD chưa đúng!";
                        return RedirectToAction("UpdateRentingRooms", new { id = maTTDatPhong });
                    }

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

                    if (lstNhanThan != null)
                    {
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
                    }
                    Session["thongbaoSuccess"] = "Chỉnh sửa thành công!";
                    return RedirectToAction("DetailtRentingRooms", new { id = maTTDatPhong });
                }
                catch (Exception e)
                {
                    Session["error-import-file"] = e.Message;
                    return RedirectToAction("DetailtRentingRooms", new { id = maTTDatPhong });
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
                            TTDatPhong.Tre_Em = TTDatPhong.Tre_Em + 1;
                        else
                            TTDatPhong.Nguoi_Lon = TTDatPhong.Nguoi_Lon + 1;

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
                            decimal tongThanhToan = TTDatPhong.Tong_Thanh_Toan;
                            TTDatPhong.Tong_Thanh_Toan = phiPhuThu + tongThanhToan;
                            model.Entry(TTDatPhong).State = EntityState.Modified;
                            model.SaveChanges();
                            Session["thongbaoSuccess"] = "Thêm thành công, phí phụ thu là 100.000đ/ngày đã được cộng vào tổng thanh toán!";
                        }

                        return RedirectToAction("DetailtRentingRooms", new { id = mattdatphong });
                    }
                    catch (Exception e)
                    {
                        Session["error-import-file"] = e.Message;
                        return RedirectToAction("DetailtRentingRooms", new { id = mattdatphong });
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
                if (!string.IsNullOrEmpty(id))
                {
                    var TTDatPhong = model.TT_Dat_Phong.First(t => t.Ma_TT_Dat_Phong.Equals(id));
                    int doitra = TTDatPhong.Doi_Tra;

                    if (doitra == 0)
                    {
                        var maLoaiPhong = TTDatPhong.Phong.Loai_Phong.Ma_Loai_Phong;
                        var maPhong = TTDatPhong.Ma_Phong;

                        Session["thongtindatphong"] = TTDatPhong;
                        Session["thoigian-den"] = TTDatPhong.Thoi_Gian_Dat;
                        Session["thoigian-ve"] = TTDatPhong.Thoi_Gian_Doi_Tra;
                        Session["songuoi-lon"] = TTDatPhong.Nguoi_Lon;
                        Session["songuoi-treem"] = TTDatPhong.Tre_Em;
                        Session["tong-thanhtoan"] = TTDatPhong.Tong_Thanh_Toan;
                        Session["tong-coc"] = TTDatPhong.Tien_Coc;

                        //Phòng ở trạng thái trống và thuộc loại phòng của khách hàng tìm
                        string loaiPhong = TTDatPhong.Phong.Ma_Loai_Phong;
                        DateTime tgianNhanPhong = Convert.ToDateTime(TTDatPhong.Thoi_Gian_Dat.ToString("yyyy-MM-dd"));
                        DateTime tgianTraPhong = Convert.ToDateTime(TTDatPhong.Thoi_Gian_Doi_Tra.ToString("yyyy-MM-dd"));
                        List<List<string>> listPhongDuocDeXuat = new List<List<string>>();
                        var phong = model.Phong.Where(p => p.Ma_Loai_Phong.Equals(loaiPhong)).ToList();
                        int index = 0;
                        for (int phg = 0; phg < phong.Count; phg++)
                        {
                            bool checks = false;
                            string maphong = phong[phg].Ma_Phong;
                            var ttCoc = model.Coc_Phong.Where(t => t.Ma_Phong.Equals(maphong) && t.Trang_Thai == 0).ToList();
                            if (ttCoc.Count <= 0)
                            {
                                listPhongDuocDeXuat.Add(new List<string>());
                                listPhongDuocDeXuat[index].Add(maphong);
                                index++;
                                continue;
                            }
                            else
                            {
                                foreach (var temp in ttCoc)
                                {
                                    if ((tgianNhanPhong >= temp.Ngay_Bat_Dau && tgianNhanPhong <= temp.Ngay_Ket_Thuc) ||
                                        (tgianTraPhong >= temp.Ngay_Bat_Dau && tgianTraPhong <= temp.Ngay_Ket_Thuc) ||
                                        (tgianNhanPhong <= temp.Ngay_Bat_Dau && tgianNhanPhong <= temp.Ngay_Ket_Thuc && tgianTraPhong >= temp.Ngay_Bat_Dau && tgianTraPhong >= temp.Ngay_Ket_Thuc))
                                    {
                                        checks = true;
                                        break;
                                    }
                                }
                                if (checks == true)
                                {
                                    if (phg >= phong.Count - 1 && listPhongDuocDeXuat.Count == 0)
                                    {
                                        Session["error-import-file"] = "Loại phòng chứa các tiện ích mà khách hàng" +
                                            " mong muốn đã hết tạm thời. hãy thử lại với phòng có số tiện ích khác!";
                                        return RedirectToAction("Homepage", "Home");
                                    }
                                }
                                else
                                {
                                    listPhongDuocDeXuat.Add(new List<string>());
                                    listPhongDuocDeXuat[index].Add(maphong);
                                    index++;
                                    continue;
                                }
                            }
                        }

                        List<Phong> phongs = new List<Phong>();
                        foreach (var item in listPhongDuocDeXuat)
                        {
                            string maphong = item[0].ToString();
                            var vphong = model.Phong.FirstOrDefault(p => p.Ma_Phong.Equals(maphong));
                            phongs.Add(vphong);
                        }

                        return View(phongs);
                    }
                    else
                    {
                        string mattdatphongs = TTDatPhong.Ma_TT_Dat_Phong;
                        var TTDoiPhong = model.TT_Doi_Phong.First(t => t.Ma_TT_Dat_Phong.Equals(mattdatphongs) && t.Lan_Doi == doitra);
                        var maLoaiPhong = TTDatPhong.Phong.Loai_Phong.Ma_Loai_Phong;
                        var maPhong = TTDoiPhong.Ma_Phong;

                        Session["thongtindatphong"] = TTDatPhong;
                        Session["thoigian-den"] = TTDatPhong.Thoi_Gian_Dat;
                        Session["thoigian-ve"] = TTDoiPhong.TG_Doi_Tra;
                        Session["songuoi-lon"] = TTDatPhong.Nguoi_Lon;
                        Session["songuoi-treem"] = TTDatPhong.Tre_Em;
                        Session["tong-thanhtoan"] = TTDatPhong.Tong_Thanh_Toan;
                        Session["tong-coc"] = TTDatPhong.Tien_Coc;

                        //Phòng ở trạng thái trống và thuộc loại phòng của khách hàng tìm
                        string loaiPhong = TTDatPhong.Phong.Ma_Loai_Phong;
                        DateTime tgianNhanPhong = Convert.ToDateTime(TTDatPhong.Thoi_Gian_Dat.ToString("yyyy-MM-dd"));
                        DateTime tgianTraPhong = Convert.ToDateTime(TTDoiPhong.TG_Doi_Tra.ToString("yyyy-MM-dd"));
                        List<List<string>> listPhongDuocDeXuat = new List<List<string>>();
                        var phong = model.Phong.Where(p => p.Ma_Loai_Phong.Equals(loaiPhong)).ToList();
                        int index = 0;
                        for (int phg = 0; phg < phong.Count; phg++)
                        {
                            bool checks = false;
                            string maphong = phong[phg].Ma_Phong;
                            var ttCoc = model.Coc_Phong.Where(t => t.Ma_Phong.Equals(maphong) && t.Trang_Thai == 0).ToList();
                            if (ttCoc.Count <= 0)
                            {
                                listPhongDuocDeXuat.Add(new List<string>());
                                listPhongDuocDeXuat[index].Add(maphong);
                                index++;
                                continue;
                            }
                            else
                            {
                                foreach (var temp in ttCoc)
                                {
                                    if ((tgianNhanPhong >= temp.Ngay_Bat_Dau && tgianNhanPhong <= temp.Ngay_Ket_Thuc) ||
                                        (tgianTraPhong >= temp.Ngay_Bat_Dau && tgianTraPhong <= temp.Ngay_Ket_Thuc) ||
                                        (tgianNhanPhong <= temp.Ngay_Bat_Dau && tgianNhanPhong <= temp.Ngay_Ket_Thuc && tgianTraPhong >= temp.Ngay_Bat_Dau && tgianTraPhong >= temp.Ngay_Ket_Thuc))
                                    {
                                        checks = true;
                                        break;
                                    }
                                }
                                if (checks == true)
                                {
                                    if (phg >= phong.Count - 1 && listPhongDuocDeXuat.Count == 0)
                                    {
                                        Session["error-import-file"] = "Loại phòng chứa các tiện ích mà khách hàng" +
                                            " mong muốn đã hết tạm thời. hãy thử lại với phòng có số tiện ích khác!";
                                        return RedirectToAction("Homepage", "Home");
                                    }
                                }
                                else
                                {
                                    listPhongDuocDeXuat.Add(new List<string>());
                                    listPhongDuocDeXuat[index].Add(maphong);
                                    index++;
                                    continue;
                                }
                            }
                        }

                        List<Phong> phongs = new List<Phong>();
                        foreach (var item in listPhongDuocDeXuat)
                        {
                            string maphong = item[0].ToString();
                            var vphong = model.Phong.FirstOrDefault(p => p.Ma_Phong.Equals(maphong));
                            phongs.Add(vphong);
                        }

                        return View(phongs);
                    }
                }
                return RedirectToAction("ListOfRentingRooms");
            }
            return RedirectToAction("Homepage", "Home");
        }

        public ActionResult SaveDataChangeRooms(string maphong, string mattdatphong) //Mã TT dat phòng
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if (!string.IsNullOrEmpty(maphong) && !string.IsNullOrEmpty(mattdatphong))
                {
                    try
                    {
                        var ttdatphong = model.TT_Dat_Phong.First(p => p.Ma_TT_Dat_Phong.Equals(mattdatphong));
                        int doitra = ttdatphong.Doi_Tra;
                        if (doitra == 0)
                        {
                            string maphongcu = ttdatphong.Ma_Phong;

                            int landoi = ttdatphong.Doi_Tra + 1;
                            DateTime ngaytraphong = ttdatphong.Thoi_Gian_Doi_Tra;
                            DateTime thoigiandoi = DateTime.Now;

                            ttdatphong.Doi_Tra = landoi;
                            ttdatphong.Thoi_Gian_Doi_Tra = thoigiandoi;

                            var phongCu = model.Phong.First(p => p.Ma_Phong.Equals(maphongcu));
                            phongCu.Ma_Trang_Thai = "TT202207050001";

                            var phongMoi = model.Phong.First(p => p.Ma_Phong.Equals(maphong));
                            phongMoi.Ma_Trang_Thai = "TT202207050002";

                            model.Entry(phongCu).State = EntityState.Modified;
                            model.Entry(phongMoi).State = EntityState.Modified;
                            model.Entry(ttdatphong).State = EntityState.Modified;

                            TT_Doi_Phong ttdoiphong = new TT_Doi_Phong();
                            ttdoiphong.Ma_TT_Doi_Phong = "1";
                            ttdoiphong.Lan_Doi = landoi;
                            ttdoiphong.Doi_Tra = 0;
                            ttdoiphong.TG_Nhan_Phong = thoigiandoi;
                            ttdoiphong.TG_Doi_Tra = ngaytraphong;
                            ttdoiphong.Ma_TT_Dat_Phong = mattdatphong;
                            ttdoiphong.Ma_Phong = maphong;
                            ttdoiphong.Ma_Tai_Khoan = Session["user-ma"].ToString();

                            model.TT_Doi_Phong.Add(ttdoiphong);
                            model.SaveChanges();
                            Session["thongbaoSuccess"] = "Đổi phòng thành công !";
                        }
                        else
                        {
                            var ttdoiphong = model.TT_Doi_Phong.First(t => t.Ma_TT_Dat_Phong.Equals(mattdatphong) && t.Lan_Doi == doitra);
                            string maphongcu = ttdoiphong.Ma_Phong;

                            int landoi = doitra + 1;
                            DateTime ngaytraphong = ttdoiphong.TG_Doi_Tra;
                            DateTime thoigiandoi = DateTime.Now;

                            ttdatphong.Doi_Tra = landoi;
                            ttdoiphong.TG_Doi_Tra = thoigiandoi;
                            ttdoiphong.Doi_Tra = 1;

                            var phongCu = model.Phong.First(p => p.Ma_Phong.Equals(maphongcu));
                            phongCu.Ma_Trang_Thai = "TT202207050001";

                            var phongMoi = model.Phong.First(p => p.Ma_Phong.Equals(maphong));
                            phongMoi.Ma_Trang_Thai = "TT202207050002";

                            model.Entry(phongCu).State = EntityState.Modified;
                            model.Entry(phongMoi).State = EntityState.Modified;
                            model.Entry(ttdatphong).State = EntityState.Modified;
                            model.Entry(ttdoiphong).State = EntityState.Modified;

                            TT_Doi_Phong ttdoiphongMoi = new TT_Doi_Phong();
                            ttdoiphongMoi.Ma_TT_Doi_Phong = "1";
                            ttdoiphongMoi.Lan_Doi = landoi;
                            ttdoiphongMoi.Doi_Tra = 0;
                            ttdoiphongMoi.TG_Nhan_Phong = thoigiandoi;
                            ttdoiphongMoi.TG_Doi_Tra = ngaytraphong;
                            ttdoiphongMoi.Ma_TT_Dat_Phong = mattdatphong;
                            ttdoiphongMoi.Ma_Phong = maphong;
                            ttdoiphongMoi.Ma_Tai_Khoan = Session["user-ma"].ToString();

                            model.TT_Doi_Phong.Add(ttdoiphongMoi);
                            model.SaveChanges();
                            Session["thongbaoSuccess"] = "Đổi phòng thành công !";
                        }
                        return RedirectToAction("DetailtRentingRooms", new { id = mattdatphong });
                    }
                    catch (Exception e)
                    {
                        Session["error-import-file"] = "Lỗi " + e.Message;
                        return RedirectToAction("DetailtRentingRooms", new { id = mattdatphong });
                    }
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
        public ActionResult CheckOutRooms(string id) //ma tt dat phong
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if (!string.IsNullOrEmpty(id))
                {
                    try
                    {
                        var TTDatPhong = model.TT_Dat_Phong.First(t => t.Ma_TT_Dat_Phong.Equals(id));
                        TTDatPhong.Trang_Thai = 1; //Đã hoàn thành
                        TTDatPhong.TaiKhoanThanhToanOrHuy = Session["user-ma"].ToString();
                        int landoi = TTDatPhong.Doi_Tra;
                        if (landoi == 0)
                        {
                            TTDatPhong.Thoi_Gian_ThanhToan = DateTime.Now;
                            var phongs = TTDatPhong.Phong;
                            phongs.Ma_Trang_Thai = "TT202207050001";
                            model.Entry(TTDatPhong).State = EntityState.Modified;
                            model.Entry(phongs).State = EntityState.Modified;
                            model.SaveChanges();
                            Session["thongbaoSuccess"] = "Trả phòng thành công!";
                        }
                        else
                        {
                            var ttdoiphong = model.TT_Doi_Phong.First(t => t.Ma_TT_Dat_Phong.Equals(id) && t.Lan_Doi == landoi);
                            TTDatPhong.Thoi_Gian_ThanhToan = DateTime.Now;
                            var phongs = ttdoiphong.Phong;
                            phongs.Ma_Trang_Thai = "TT202207050001";
                            model.Entry(TTDatPhong).State = EntityState.Modified;
                            model.Entry(ttdoiphong).State = EntityState.Modified;
                            model.Entry(phongs).State = EntityState.Modified;
                            model.SaveChanges();
                            Session["thongbaoSuccess"] = "Trả phòng thành công!";
                        }
                        decimal thanhtoanT = TTDatPhong.Tong_Thanh_Toan - TTDatPhong.Tien_Coc;

                        Sao_Ke saoke = new Sao_Ke();
                        string maSaoKe = "SK" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        saoke.Ma_Sao_Ke = maSaoKe;
                        saoke.Ma_TT_Dat_Phong = id;
                        saoke.Ma_Tai_Khoan = Session["user-ma"].ToString();
                        saoke.So_Tien = thanhtoanT;
                        saoke.Ngay_Giao_Dich = DateTime.Now;
                        saoke.Coc_or_ThanhToan = 1;
                        saoke.Note = "Khách hàng: " + TTDatPhong.Ho_Ten_KH + " (CMND/CCCD: " + TTDatPhong.CMND_CCCD_KH + ") đã thanh toán cọc phòng với số tiền "
                            + thanhtoanT.ToString("0,0") + " VND vào ngày " + saoke.Ngay_Giao_Dich + ". Thực hiện bởi Nhân viên: "
                            + Session["user-fullname"].ToString() + " (" + Session["user-id"].ToString() + ")";
                        model.Sao_Ke.Add(saoke);

                        if (!string.IsNullOrEmpty(TTDatPhong.Ma_Tour))
                        {
                            string matourr = TTDatPhong.Ma_Tour;
                            var datcoc = model.Coc_Phong.FirstOrDefault(c => c.Ma_TT_Dat_Phong.Equals(id) && c.Ma_Tour.Equals(matourr));
                            model.Coc_Phong.Remove(datcoc);
                        }
                        model.SaveChanges();
                        return RedirectToAction("DetailtRentingRooms", new { id = id });

                    }
                    catch (Exception e)
                    {
                        Session["error-import-file"] = e.Message;
                        return RedirectToAction("DetailtRentingRooms", new { id = id });
                    }
                }
            }
            return RedirectToAction("Homepage", "Home");
        }
    }
}