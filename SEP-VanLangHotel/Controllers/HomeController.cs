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
                return View(model.Tien_Ich.ToList());
            return RedirectToAction("Dashboard"); //Nếu Là tài khoản của admin sẽ đi đến hàm Dashboard

        }
        [HttpPost]
        public ActionResult ImportData(HttpPostedFileBase importFileExcel)
        {
            Session["error-import-file"] = "empthy";
            if (Session["user-role"].Equals("Nhân viên"))
            {
                try
                {
                    string filePath = string.Empty;
                    if (importFileExcel != null)
                    {
                        string path = Server.MapPath("~/Upload/");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        filePath = path + Path.GetFileName(importFileExcel.FileName);
                        string extension = Path.GetExtension(importFileExcel.FileName);
                        importFileExcel.SaveAs(filePath);

                        string conString = string.Empty;
                        switch (extension)
                        {
                            case ".xls":
                                conString = ConfigurationManager.ConnectionStrings["Excel03ConString"].ConnectionString;
                                break;
                            case ".xlsx":
                                conString = ConfigurationManager.ConnectionStrings["Excel07ConString"].ConnectionString;
                                break;
                            default:
                                Session["error-import-file"] = "Hãy nhập file danh sách đặt phòng với định dạng là file Excel";
                                break;
                        }
                        if (!Session["error-import-file"].Equals("empthy"))
                        {
                            return RedirectToAction("Homepage");
                        }
                        else
                        {
                            //đọc dữ liệu từ excel
                            DataTable dtExcel = new DataTable();
                            conString = string.Format(conString, filePath);
                            using (OleDbConnection connExcel = new OleDbConnection(conString))
                            {
                                using (OleDbCommand cmdExcel = new OleDbCommand())
                                {
                                    using (OleDbDataAdapter odaExcel = new OleDbDataAdapter())
                                    {
                                        cmdExcel.Connection = connExcel;
                                        connExcel.Open();
                                        DataTable dtExcelSchema = connExcel.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                                        string sheetName = dtExcelSchema.Rows[1].Field<string>("TABLE_NAME");
                                        connExcel.Close();

                                        connExcel.Open();
                                        cmdExcel.CommandText = "SELECT * from [" + sheetName + "]";
                                        odaExcel.SelectCommand = cmdExcel;
                                        odaExcel.Fill(dtExcel);
                                        connExcel.Close();
                                    }
                                }
                            }

                            //Lấy ra danh sách đặt phòng từ file import
                            List<List<string>> listDSDatPhong = new List<List<string>>();
                            int index = 0;
                            foreach (DataRow dr in dtExcel.Rows)
                            {
                                if (index == 0)
                                {
                                    index++; // bỏ qua hàng tiêu đề
                                }
                                else
                                {
                                    listDSDatPhong.Add(new List<string>());
                                    for (int i = 0; i < dtExcel.Columns.Count; i++)
                                        listDSDatPhong[index - 1].Add(dr[i].ToString()); //thêm dữ liệu từng cột cho mỗi hàng
                                    index++;
                                }
                            }

                            //Kiểm tra ds ngày đi và ds ngày ở có bị sai hay không
                            for (int i = 0; i < listDSDatPhong.Count; i++)
                            {
                                if (!listDSDatPhong[i][4].Trim().Equals(listDSDatPhong[0][4].Trim()))
                                {
                                    Session["error-import-file"] = "Ngày trả phòng của các khách hàng trong danh sách không khớp với nhau, hãy kiểm tra lại!";
                                    return RedirectToAction("Homepage");
                                }
                                if (!listDSDatPhong[i][5].Trim().Equals(listDSDatPhong[0][5].Trim()))
                                {
                                    Session["error-import-file"] = "Ngày trả phòng của các khách hàng trong danh sách không khớp với nhau, hãy kiểm tra lại!";
                                    return RedirectToAction("Homepage");
                                }
                                for (int j = 0; j < listDSDatPhong.Count; j++)
                                {
                                    if (listDSDatPhong[j][7].Trim().Length != 12 && listDSDatPhong[j][7].Trim().Length != 9)
                                    {
                                        Session["error-import-file"] = "Số CMND/CCCD của khách hàng số " + listDSDatPhong[j][0] + " chưa đúng định dạng!";
                                        return RedirectToAction("Homepage");
                                    }
                                    if (i != j)
                                    {
                                        if (listDSDatPhong[i][7].Trim().Equals(listDSDatPhong[j][7].Trim()))
                                        {
                                            Session["error-import-file"] = "CMND/CCCD giữa các khách hàng trong danh sách có sự trùng lặp!";
                                            return RedirectToAction("Homepage");
                                        }
                                    }
                                }
                                for (int j = 0; j < listDSDatPhong.Count; j++)
                                {
                                    if (listDSDatPhong[j][8].Trim().Length != 10)
                                    {
                                        Session["error-import-file"] = "Số điện thoại của khách hàng số " + listDSDatPhong[j][0] + " chưa đúng định dạng!";
                                        return RedirectToAction("Homepage");
                                    }
                                    if (i != j)
                                    {
                                        if (listDSDatPhong[i][8].Trim().Equals(listDSDatPhong[j][8].Trim()))
                                        {
                                            Session["error-import-file"] = "Số điện thoại giữa các khách hàng trong danh sách có sự trùng lặp!";
                                            return RedirectToAction("Homepage");
                                        }
                                    }
                                }
                            }

                            //Lấy danh sách các tiện ích trên database
                            var tienIch = model.Tien_Ich.ToList();
                            List<List<string>> listTienIch = new List<List<string>>();
                            for (int i = 0; i < tienIch.Count; i++)
                            {
                                listTienIch.Add(new List<string>());
                                listTienIch[i].Add(tienIch[i].Ma_Tien_Ich);
                            }

                            //Lấy danh sách tiện ích cần có của từng dòng trong file import
                            List<List<string>> listTienIchCanCo = new List<List<string>>();
                            int soCotTTPhong = 6;
                            int soCotThongTTKhachHang = 6;
                            for (int i = 0; i < listDSDatPhong.Count; i++)
                            {
                                listTienIchCanCo.Add(new List<string>());
                                for (int j = (soCotTTPhong + soCotThongTTKhachHang); j < listDSDatPhong[i].Count; j++) //Bắt đầu từ cột tiện ích thứ 1 trở đi
                                    if (Convert.ToInt32(listDSDatPhong[i][j].ToString().Trim()) == 1) //1 nghĩa là khách hàng chọn tiện ích
                                        listTienIchCanCo[i].Add(listTienIch[((-(listDSDatPhong[i].Count)) + (listDSDatPhong[i].Count - (soCotTTPhong + soCotThongTTKhachHang)) + j)][0]);
                                listTienIchCanCo[i].Add("TI202207070001"); //Tiện ích wifi mặc định có
                            }

                            //Kiểm tra các loại phòng đáp ứng đủ điều kiện của khách hàng
                            List<List<string>> listLoaiPhongDuocDeXuat = new List<List<string>>();
                            for (int l = 0; l < listDSDatPhong.Count; l++) //Lấy ra từng dòng của danh sách đặt phòng từ file import
                            {
                                var dsLoaiPhongModel = model.Loai_Phong.ToList();

                                //Kiểm tra số giường là 1
                                if (Convert.ToInt32(listDSDatPhong[l][3].Trim()) == 1)
                                {
                                    //Kiểm tra số người
                                    if (Convert.ToInt32(listDSDatPhong[l][1].Trim()) <= 2
                                        && Convert.ToInt32(listDSDatPhong[l][2].Trim()) <= 2)
                                    {
                                        //Loại 1
                                        foreach (var item in dsLoaiPhongModel)
                                        {
                                            bool kiemTraDuTienIch = true;
                                            if (item.DS_Tien_Ich.Count == listTienIchCanCo[l].Count
                                                && item.So_Nguoi_Lon <= 2
                                                && item.So_Tre_Em <= 2
                                                && item.So_Giuong == 1) //Kiểm tra số lượng tiện ích của phòng có bằng tiện ích cần có của khách hàng hay không
                                            {
                                                for (int i = 0; i < listTienIchCanCo[l].Count; i++) //lấy danh sách tiện ích cần có
                                                {
                                                    string tienIchCanCo = listTienIchCanCo[l][i];
                                                    var dsTienIch = item.DS_Tien_Ich.FirstOrDefault(t => t.Ma_Tien_Ich.Equals(tienIchCanCo));
                                                    if (dsTienIch == null)
                                                        kiemTraDuTienIch = false;
                                                }
                                                if (kiemTraDuTienIch == true)
                                                {
                                                    listLoaiPhongDuocDeXuat.Add(new List<string>());
                                                    listLoaiPhongDuocDeXuat[l].Add(item.Ma_Loai_Phong);
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Session["error-import-file"] = "Số lượng người ở của nhóm khách số " + listDSDatPhong[l][0].Trim() + " nằm ngoài phạm vị cho phép của khách sạn!";
                                        return RedirectToAction("Homepage");
                                    }
                                }
                                //Kiểm tra số giường là 2
                                else if (Convert.ToInt32(listDSDatPhong[l][3].Trim()) == 2)
                                {
                                    //Kiểm tra số người
                                    if (Convert.ToInt32(listDSDatPhong[l][1].Trim()) <= 2
                                        && Convert.ToInt32(listDSDatPhong[l][2].Trim()) <= 4)
                                    {
                                        //Loại 2
                                        foreach (var item in dsLoaiPhongModel)
                                        {
                                            bool kiemTraDuTienIch = true;
                                            if (item.DS_Tien_Ich.Count == listTienIchCanCo[l].Count
                                                && item.So_Nguoi_Lon <= 2
                                                && item.So_Tre_Em <= 4
                                                && item.So_Giuong == 2) //Kiểm tra số lượng tiện ích, người và giường  của phòng có bằng với số lượng của khách hàng cần hay không
                                            {
                                                for (int i = 0; i < listTienIchCanCo[l].Count; i++) //lấy danh sách tiện ích cần có
                                                {
                                                    string tienIchCanCo = listTienIchCanCo[l][i];
                                                    var dsTienIch = item.DS_Tien_Ich.FirstOrDefault(t => t.Ma_Tien_Ich.Equals(tienIchCanCo));
                                                    if (dsTienIch == null)
                                                        kiemTraDuTienIch = false;
                                                }
                                                if (kiemTraDuTienIch == true)
                                                {
                                                    listLoaiPhongDuocDeXuat.Add(new List<string>());
                                                    listLoaiPhongDuocDeXuat[l].Add(item.Ma_Loai_Phong);
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                    else if (Convert.ToInt32(listDSDatPhong[l][1].Trim()) == 3
                                        && Convert.ToInt32(listDSDatPhong[l][2].Trim()) <= 3)
                                    {
                                        //Loại 3
                                        foreach (var item in dsLoaiPhongModel)
                                        {
                                            bool kiemTraDuTienIch = true;
                                            if (item.DS_Tien_Ich.Count == listTienIchCanCo[l].Count
                                                && item.So_Nguoi_Lon == 3
                                                && item.So_Tre_Em <= 3
                                                && item.So_Giuong == 2) //Kiểm tra số lượng tiện ích, người và giường  của phòng có bằng với số lượng của khách hàng cần hay không
                                            {
                                                for (int i = 0; i < listTienIchCanCo[l].Count; i++) //lấy danh sách tiện ích cần có
                                                {
                                                    string tienIchCanCo = listTienIchCanCo[l][i];
                                                    var dsTienIch = item.DS_Tien_Ich.FirstOrDefault(t => t.Ma_Tien_Ich.Equals(tienIchCanCo));
                                                    if (dsTienIch == null)
                                                        kiemTraDuTienIch = false;
                                                }
                                                if (kiemTraDuTienIch == true)
                                                {
                                                    listLoaiPhongDuocDeXuat.Add(new List<string>());
                                                    listLoaiPhongDuocDeXuat[l].Add(item.Ma_Loai_Phong);
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                    else if (Convert.ToInt32(listDSDatPhong[l][1].Trim()) == 4
                                        && Convert.ToInt32(listDSDatPhong[l][2].Trim()) <= 2)
                                    {
                                        //Loại 4
                                        foreach (var item in dsLoaiPhongModel)
                                        {
                                            bool kiemTraDuTienIch = true;
                                            if (item.DS_Tien_Ich.Count == listTienIchCanCo[l].Count
                                                && item.So_Nguoi_Lon == 4
                                                && item.So_Tre_Em <= 2
                                                && item.So_Giuong == 2) //Kiểm tra số lượng tiện ích, người và giường  của phòng có bằng với số lượng của khách hàng cần hay không
                                            {
                                                for (int i = 0; i < listTienIchCanCo[l].Count; i++) //lấy danh sách tiện ích cần có
                                                {
                                                    string tienIchCanCo = listTienIchCanCo[l][i];
                                                    var dsTienIch = item.DS_Tien_Ich.FirstOrDefault(t => t.Ma_Tien_Ich.Equals(tienIchCanCo));
                                                    if (dsTienIch == null)
                                                        kiemTraDuTienIch = false;
                                                }
                                                if (kiemTraDuTienIch == true)
                                                {
                                                    listLoaiPhongDuocDeXuat.Add(new List<string>());
                                                    listLoaiPhongDuocDeXuat[l].Add(item.Ma_Loai_Phong);
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                continue;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Session["error-import-file"] = "Số lượng người ở của nhóm khách số " + listDSDatPhong[l][0].Trim() + " nằm ngoài phạm vị cho phép của khách sạn!";
                                        return RedirectToAction("Homepage");
                                    }
                                }
                                else //Số giường nhỏ hơn hoặc lớn hơn số giường mà các phòng ở khách sạn có
                                {
                                    Session["error-import-file"] = "Không có phòng nào có số giường đúng với yêu cầu của nhóm khách số " + listDSDatPhong[l][0].Trim();
                                    return RedirectToAction("Homepage");
                                }
                            }

                            //Lấy danh sách phòng thuộc loại phòng mà khách hàng cần
                            List<List<string>> listPhongDuocDeXuat = new List<List<string>>();
                            int itemPhong = 0;
                            for (int k = 0; k < listDSDatPhong.Count; k++)
                            {
                                var phong = model.Phong.Where(p => p.Ma_Trang_Thai.Equals("TT202207050001")).ToList(); //Phòng ở trạng thái trống
                                if (itemPhong == 0) //Phòng đầu tiên thì không cần kiểm tra trùng phòng
                                {
                                    foreach (var item in phong)
                                    {
                                        if (item.Ma_Loai_Phong.Equals(listLoaiPhongDuocDeXuat[itemPhong][0]))
                                        {
                                            listPhongDuocDeXuat.Add(new List<string>());
                                            listPhongDuocDeXuat[itemPhong].Add(item.Ma_Phong);
                                            itemPhong++;
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var item in phong)
                                    {
                                        bool phongKhongTrung = true;
                                        for (int j = 0; j < itemPhong; j++)
                                        {
                                            if (listPhongDuocDeXuat[j][0].Equals(item.Ma_Phong)) //Kiểm tra phòng hiện tại đã sử dụng cho phòng trước hay chưa
                                            {
                                                phongKhongTrung = false;
                                            }
                                        }
                                        if (phongKhongTrung == true) //Không trùng thì sẽ thêm vào đề xuất
                                        {
                                            if (item.Ma_Loai_Phong.Equals(listLoaiPhongDuocDeXuat[itemPhong][0]))
                                            {
                                                listPhongDuocDeXuat.Add(new List<string>());
                                                listPhongDuocDeXuat[itemPhong].Add(item.Ma_Phong);
                                                itemPhong++;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }

                            //Kiểm tra còn đủ phòng hay không
                            if (listDSDatPhong.Count != listPhongDuocDeXuat.Count)
                            {
                                Session["error-import-file"] = "Khách sạn đã không còn đủ phòng để đặt nữa rồi :( ";
                            }
                            else
                            {
                                // string phongne = "";
                                List<TT_Dat_Phong> dsDatPhongTamThoi = new List<TT_Dat_Phong>();
                                decimal tongtien = 0;
                                for (int i = 0; i < listDSDatPhong.Count; i++)
                                {
                                    var maphonggs = listPhongDuocDeXuat[i][0].Trim();
                                    tongtien += model.Phong.FirstOrDefault(l => l.Ma_Phong.Equals(maphonggs)).Loai_Phong.Gia;

                                    // phongne += listPhongDuocDeXuat[i][0] + "\n";
                                    var ttdatphong = new TT_Dat_Phong();

                                    ttdatphong.Ma_TT_Dat_Phong = i.ToString();
                                    ttdatphong.Ho_Ten_KH = listDSDatPhong[i][6].Trim();
                                    ttdatphong.CMND_CCCD_KH = listDSDatPhong[i][7].Trim();
                                    ttdatphong.SDT_KH = listDSDatPhong[i][8].Trim();
                                    // dsDatPhongTamThoi[i].Sinh_Nhat_KH = DateTime.ParseExact(listDSDatPhong[i][9], "dd/MM/yyyy", CultureInfo.InvariantCulture);
                                    ttdatphong.Sinh_Nhat_KH = DateTime.Parse(listDSDatPhong[i][9].Trim());
                                    ttdatphong.Gioi_Tinh_KH = Int32.Parse(listDSDatPhong[i][10].Trim());
                                    ttdatphong.Dia_Chi_KH = listDSDatPhong[i][11].Trim();
                                    ttdatphong.Doi_Tra = 0; //Tra = 0; doi = 1
                                                            //dsDatPhongTamThoi[i].Thoi_Gian_Dat = DateTime.ParseExact(DateTime.Now.ToString("dd/MM/yyyy"), "dd/MM/yyyy", CultureInfo.InvariantCulture); ;
                                    ttdatphong.Thoi_Gian_Dat = DateTime.Parse(listDSDatPhong[i][4].Trim());
                                    ttdatphong.Thoi_Gian_Doi_Tra = DateTime.Parse(listDSDatPhong[i][5].Trim());

                                    ttdatphong.Ma_Tai_Khoan = Session["user-ma"].ToString();
                                    ttdatphong.Nguoi_Lon = Int32.Parse(listDSDatPhong[i][1].Trim());
                                    ttdatphong.Tre_Em = Int32.Parse(listDSDatPhong[i][2].Trim());
                                    ttdatphong.Ma_Phong = listPhongDuocDeXuat[i][0].Trim();
                                    dsDatPhongTamThoi.Add(ttdatphong);

                                    // DateTime.Now.AddDays(1); // Cộng thêm 1 ngày

                                }
                                long songay = long.Parse(DateTime.Parse(listDSDatPhong[0][5].Trim()).ToString("yyyyMMdd")) - long.Parse(DateTime.Parse(listDSDatPhong[0][4].Trim()).ToString("yyyyMMdd"));
                                Session["tong-thanhtoan"] = tongtien * songay;
                                Session["tong-coc"] = (tongtien * songay) * Convert.ToDecimal(0.3);
                                List<string> gioitinh = new List<string>();
                                gioitinh.Add("Nam");
                                gioitinh.Add("Nữ");
                                gioitinh.Add("Khác");
                                ViewBag.Gioi_Tinh = new SelectList(gioitinh);
                                ViewBag.gioiTinh = "";
                                return View("SaveDataImport", dsDatPhongTamThoi);
                                // phongne = "";
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Session["error-import-file"] = "Lỗi " + e.Message.ToString();
                }
            }
            return RedirectToAction("Homepage");
        }
        public ActionResult SaveDataImport(List<TT_Dat_Phong> dsDatPhongTamThoi)
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if (dsDatPhongTamThoi != null)
                {
                    List<string> gioitinh = new List<string>();
                    gioitinh.Add("Nam");
                    gioitinh.Add("Nữ");
                    gioitinh.Add("Khác");
                    ViewBag.Gioi_Tinh = new SelectList(gioitinh);
                    ViewBag.gioiTinh = "";
                    return View(dsDatPhongTamThoi);
                }
            }
            return RedirectToAction("Homepage");
        }

        [HttpPost]
        public ActionResult SaveDataImport(List<TT_Dat_Phong> dsDatPhongTamThoi, string hoten,
            string cmndcccd, string sdt, DateTime ngaysinh, string gioiTinh, string email,
            string diachi = null, decimal sotiencoc = 0, decimal tongthanhtoan = 0)
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                try
                {
                    TOUR tour = new TOUR();
                    string maTour = DateTime.Now.ToString("yyyyMMddHHmmss");

                    tour.Ma_Tour = maTour;
                    tour.Ho_Ten_Chu_Tour = hoten.Trim();
                    tour.CMND_CCCD = cmndcccd.Trim();
                    tour.SDT = sdt.Trim();
                    tour.Sinh_Nhat = ngaysinh;
                    if (gioiTinh.Equals("Nam"))
                        tour.Gioi_Tinh = 1;
                    else if (gioiTinh.Equals("Nữ"))
                        tour.Gioi_Tinh = 0;
                    else
                        tour.Gioi_Tinh = 2;
                    tour.Email = email.Trim();
                    tour.Dia_Chi = diachi.Trim();
                    tour.Thoi_Gian_DatPhong_Coc = DateTime.Now;
                    tour.Thoi_Gian_NhanPhong = dsDatPhongTamThoi[0].Thoi_Gian_Dat;
                    tour.Thoi_Gian_TraPhong = dsDatPhongTamThoi[0].Thoi_Gian_Doi_Tra;
                    tour.So_Tien_Coc = sotiencoc;
                    tour.Tong_Thanh_Toan = tongthanhtoan;
                    tour.Trang_Thai = 0; //0: chờ thanh toán, 1: hoàn thành, 2: đã hủy
                    tour.Ma_Tai_Khoan = dsDatPhongTamThoi[0].Ma_Tai_Khoan;
                    model.TOUR.Add(tour);
                    model.SaveChanges();

                    for (int i = 0; i < dsDatPhongTamThoi.Count; i++)
                    {
                        dsDatPhongTamThoi[i].Ma_Tour = maTour;
                    }
                    model.TT_Dat_Phong.AddRange(dsDatPhongTamThoi);
                    model.SaveChanges();

                    foreach (var item in dsDatPhongTamThoi)
                    {
                        var maPhongs = item.Ma_Phong;
                        var phongs = model.Phong.First(p => p.Ma_Phong.Equals(maPhongs));
                        phongs.Ma_Trang_Thai = "TT202207110001"; //Đã cọc
                        model.Entry(phongs).State = EntityState.Modified;
                        model.SaveChanges();
                    }

                    Session["thongbao-datphongthanhcong"] = true;
                    return RedirectToAction("Homepage");
                }
                catch (Exception e)
                {
                    Session["error-import-file"] = "Lỗi " + e.Message.ToString();
                    List<string> gioitinhs = new List<string>();
                    gioitinhs.Add("Nam");
                    gioitinhs.Add("Nữ");
                    gioitinhs.Add("Khác");
                    ViewBag.Gioi_Tinh = new SelectList(gioitinhs);
                    ViewBag.gioiTinh = "";
                    return View(dsDatPhongTamThoi);
                }
            }
            return RedirectToAction("Homepage");
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