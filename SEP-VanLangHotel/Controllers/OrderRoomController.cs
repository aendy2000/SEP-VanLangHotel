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
    public class OrderRoomController : Controller
    {
        SEP25Team09Entities model = new SEP25Team09Entities();
        //Lấy thông tin đặt phòng import
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
                            return RedirectToAction("Homepage", "Home");
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
                            if (!string.IsNullOrEmpty(dtExcel.Rows[1][15].ToString().Trim())
                                && !string.IsNullOrEmpty(dtExcel.Rows[1][16].ToString().Trim()))
                            {
                                try
                                {
                                    DateTime DngayDen = Convert.ToDateTime(dtExcel.Rows[1][15].ToString().Trim());
                                    DateTime DngayVe = Convert.ToDateTime(dtExcel.Rows[1][16].ToString().Trim());

                                    int soSanhNgayHienTai = (int)DngayDen.Subtract(DateTime.Now).TotalDays + 1;
                                    int soSanhNgayDenNgayVe = (int)DngayVe.Subtract(DngayDen).TotalDays + 1;
                                    Session["SoNgay"] = soSanhNgayDenNgayVe;
                                    Session["NgayDen"] = DngayDen;
                                    Session["NgayVe"] = DngayVe;

                                    if (soSanhNgayHienTai < 1)
                                    {
                                        Session["error-import-file"] = "Ngày đến không thể thấp hơn ngày hiện tại, hãy kiểm tra lại!";
                                        return RedirectToAction("Homepage", "Home");
                                    }
                                    else if (soSanhNgayDenNgayVe < 1)
                                    {
                                        Session["error-import-file"] = "Ngày đến không thể thấp hơn ngày về, hãy kiểm tra lại!";
                                        return RedirectToAction("Homepage", "Home");
                                    }
                                }
                                catch (Exception)
                                {
                                    Session["error-import-file"] = "Ngày đến hoặc ngày về chưa đúng định dạng, hãy kiểm tra lại!";
                                    return RedirectToAction("Homepage", "Home");
                                }
                            }
                            else
                            {
                                Session["error-import-file"] = "Ngày đến và ngày về chưa được nhập, hãy kiểm tra lại!";
                                return RedirectToAction("Homepage", "Home");
                            }


                            //Lấy ra danh sách khách hàng theo nhóm từ file import
                            List<List<List<string>>> listDSDatPhong = new List<List<List<string>>>();
                            int indexRowsGroup = 0;
                            int indexRowsMember = 0;
                            foreach (DataRow dr in dtExcel.Rows)
                            {
                                if (indexRowsMember == 0)
                                {
                                    indexRowsMember++; // bỏ qua hàng tiêu đề
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(dr[0].ToString().Trim())) //Ô đầu tiên mà có dữ liệu là ô bắt đầu cho nhóm khách hàng mới
                                    {
                                        indexRowsGroup++;
                                        indexRowsMember = 1;
                                        listDSDatPhong.Add(new List<List<string>>()); //thêm mới 1 mãng cho nhóm khách hàng này
                                        listDSDatPhong[indexRowsGroup - 1].Add(new List<string>()); //Thêm mới 1 mãng thông tin cho từng người trong nhóm

                                        for (int i = 1; i < dtExcel.Columns.Count - 2; i++)
                                        {
                                            if (i != 3) //Bỏ qua cột mối quan hệ, vì ng đại diện k cần ghi mối quan hệ
                                            {
                                                if (string.IsNullOrEmpty(dr[i].ToString().Trim()))
                                                {
                                                    Session["error-import-file"] = "Tồn tại trường thông tin cần thiết bị bỏ trống, hãy kiểm tra lại file import!";
                                                    return RedirectToAction("Homepage", "Home");
                                                }
                                                else
                                                {
                                                    if (i == 4)
                                                    {
                                                        long n;
                                                        bool tryCMND = long.TryParse(dr[4].ToString().Trim(), out n);
                                                        if ((dr[4].ToString().Trim().Length != 12 && dr[4].ToString().Trim().Length != 9
                                                            || tryCMND == false))
                                                        {
                                                            Session["error-import-file"] = "Khách hàng nhóm " + indexRowsGroup + ", tên: " + dr[2].ToString().Trim() + " có CMND/CCCD chưa đúng!";
                                                            return RedirectToAction("Homepage", "Home");
                                                        }
                                                    }
                                                    else if (i == 5)
                                                    {
                                                        long n;
                                                        bool trySDT = long.TryParse(dr[5].ToString().Trim(), out n);
                                                        if ((dr[5].ToString().Trim().Length != 10
                                                            || trySDT == false))
                                                        {
                                                            Session["error-import-file"] = "Khách hàng nhóm " + indexRowsGroup + ", tên: " + dr[2].ToString().Trim() + " có số điện thoại chưa đúng!";
                                                            return RedirectToAction("Homepage", "Home");
                                                        }
                                                    }
                                                    else if (i == 6)
                                                    {

                                                        DateTime n;
                                                        bool trySDT = DateTime.TryParse(dr[6].ToString().Trim(), out n);
                                                        if (trySDT == false)
                                                        {
                                                            Session["error-import-file"] = "Ngày sinh của khách hàng nhóm " + indexRowsGroup + ", tên: " + dr[2].ToString().Trim() + " có ngày sinh không hợp lệ!";
                                                            return RedirectToAction("Homepage", "Home");
                                                        }
                                                    }
                                                    else if (i == 7 && (!dr[7].ToString().Trim().Equals("0") && !dr[7].ToString().Trim().Equals("1") && !dr[7].ToString().Trim().Equals("2")))
                                                    {
                                                        Session["error-import-file"] = "Giới tính của khách hàng nhóm " + indexRowsGroup + ", tên: " + dr[2].ToString().Trim() + " không thuộc giá trị (0 - 2)!";
                                                        return RedirectToAction("Homepage", "Home");
                                                    }
                                                    else if (i == 9 && (!dr[9].ToString().Trim().Equals("1") && !dr[9].ToString().Trim().Equals("2")))
                                                    {
                                                        Session["error-import-file"] = "Số giường của khách hàng nhóm " + indexRowsGroup + " không thuộc giá trị (1 - 2)!";
                                                        return RedirectToAction("Homepage", "Home");
                                                    }
                                                    else if (i == 10 && (!dr[10].ToString().Trim().Equals("0") && !dr[10].ToString().Trim().Equals("1")))
                                                    {
                                                        Session["error-import-file"] = "Tiện ích đầu tiên của khách hàng nhóm " + indexRowsGroup + " không thuộc giá trị (0 hoặc 1)!";
                                                        return RedirectToAction("Homepage", "Home");
                                                    }
                                                    else if (i == 11 && (!dr[11].ToString().Trim().Equals("0") && !dr[11].ToString().Trim().Equals("1")))
                                                    {
                                                        Session["error-import-file"] = "Tiện ích thứ 2 của khách hàng nhóm " + indexRowsGroup + " không thuộc giá trị (0 hoặc 1)!";
                                                        return RedirectToAction("Homepage", "Home");
                                                    }
                                                    else if (i == 12 && (!dr[12].ToString().Trim().Equals("0") && !dr[12].ToString().Trim().Equals("1")))
                                                    {
                                                        Session["error-import-file"] = "Tiện ích thứ 3của khách hàng nhóm " + indexRowsGroup + " không thuộc giá trị (0 hoặc 1)!";
                                                        return RedirectToAction("Homepage", "Home");
                                                    }
                                                    else if (i == 13 && (!dr[13].ToString().Trim().Equals("0") && !dr[13].ToString().Trim().Equals("1")))
                                                    {
                                                        Session["error-import-file"] = "Tiện ích thứ 4 của khách hàng nhóm " + indexRowsGroup + " không thuộc giá trị (0 hoặc 1)!";
                                                        return RedirectToAction("Homepage", "Home");
                                                    }
                                                    else if (i == 14 && (!dr[14].ToString().Trim().Equals("0") && !dr[14].ToString().Trim().Equals("1")))
                                                    {
                                                        Session["error-import-file"] = "Tiện ích thứ 5 của khách hàng nhóm " + indexRowsGroup + " không thuộc giá trị (0 hoặc 1)!";
                                                        return RedirectToAction("Homepage", "Home");
                                                    }
                                                }
                                                listDSDatPhong[indexRowsGroup - 1][indexRowsMember - 1].Add(dr[i].ToString().Trim());
                                            }
                                        }
                                        indexRowsMember++;
                                    }
                                    else
                                    {
                                        listDSDatPhong[indexRowsGroup - 1].Add(new List<string>()); //Thêm mới 1 mãng thông tin cho từng người trong nhóm
                                        for (int i = 1; i <= 8; i++)
                                        {
                                            if (i != 5) //Bỏ qua cột mối quan hệ, vì ng đại diện k cần ghi mối quan hệ
                                            {
                                                if (string.IsNullOrEmpty(dr[i].ToString().Trim()) && i != 4)
                                                {
                                                    Session["error-import-file"] = "Tồn tại trường thông tin cần thiết bị bỏ trống, hãy kiểm tra lại file import!";
                                                    return RedirectToAction("Homepage", "Home");
                                                }
                                                else if (i == 4) //tại cột cmnd nếu hơn 18t mà k có thì báo lỗi
                                                {
                                                    try
                                                    {
                                                        DateTime ngaySinhhs = Convert.ToDateTime(dr[6].ToString().Trim());
                                                        if (DateTime.Now.Year - ngaySinhhs.Year >= 18)
                                                        {
                                                            if (string.IsNullOrEmpty(dr[4].ToString().Trim()))
                                                            {
                                                                Session["error-import-file"] = "Khách hàng nhóm " + indexRowsGroup + ", tên: " + dr[2].ToString().Trim() + " chưa nhập CMND/CCCD!";
                                                                return RedirectToAction("Homepage", "Home");
                                                            }
                                                            else
                                                            {
                                                                long n;
                                                                bool tryCMND = long.TryParse(dr[4].ToString().Trim(), out n);
                                                                if ((dr[4].ToString().Trim().Length != 12 && dr[4].ToString().Trim().Length != 9
                                                                    || tryCMND == false))
                                                                {
                                                                    Session["error-import-file"] = "Khách hàng nhóm " + indexRowsGroup + ", tên: " + dr[2].ToString().Trim() + " có CMND/CCCD chưa đúng!";
                                                                    return RedirectToAction("Homepage", "Home");
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            listDSDatPhong[indexRowsGroup - 1][indexRowsMember - 1].Add(null);
                                                            continue;
                                                        }
                                                    }
                                                    catch (Exception)
                                                    {
                                                        Session["error-import-file"] = "Ngày sinh của khách hàng nhóm " + indexRowsGroup + ", tên: " + dr[2].ToString().Trim() + " có ngày sinh không hợp lệ!";
                                                        return RedirectToAction("Homepage", "Home");
                                                    }
                                                }
                                                else if (i == 6)
                                                {
                                                    DateTime n;
                                                    bool trySinhNhat = DateTime.TryParse(dr[6].ToString().Trim() + "", out n);
                                                    if (trySinhNhat == false)
                                                    {
                                                        Session["error-import-file"] = "Ngày sinh của khách hàng nhóm " + indexRowsGroup + ", tên: " + dr[2].ToString().Trim() + " có ngày sinh không hợp lệ!";
                                                        return RedirectToAction("Homepage", "Home");
                                                    }
                                                }
                                                else if (i == 7 && (!dr[7].ToString().Trim().Equals("0") && !dr[7].ToString().Trim().Equals("1") && !dr[7].ToString().Trim().Equals("2")))
                                                {
                                                    Session["error-import-file"] = "Giới tính của khách hàng nhóm " + indexRowsGroup + ", tên: " + dr[2].ToString().Trim() + " không thuộc giá trị (0 - 2)!";
                                                    return RedirectToAction("Homepage", "Home");
                                                }
                                                listDSDatPhong[indexRowsGroup - 1][indexRowsMember - 1].Add(dr[i].ToString().Trim());
                                            }
                                        }
                                        indexRowsMember++;
                                    }
                                }
                            }


                            List<List<string>> SoNguoi = new List<List<string>>();
                            //Tính số người (trẻ em, người lớn) của từng nhóm khách
                            for (int i = 0; i < listDSDatPhong.Count; i++)
                            {
                                int nguoiLonTemp = 0;
                                int treEmTemp = 0;
                                SoNguoi.Add(new List<string>());
                                for (int j = 0; j < listDSDatPhong[i].Count; j++)
                                {
                                    if (DateTime.Now.Year - Convert.ToDateTime(listDSDatPhong[i][j][4]).Year <= 14)
                                    {
                                        treEmTemp++;
                                    }
                                    else
                                    {
                                        nguoiLonTemp++;
                                    }
                                }
                                SoNguoi[i].Add(nguoiLonTemp.ToString());
                                SoNguoi[i].Add(treEmTemp.ToString());
                            }
                            Session["nguoilon0treem1"] = SoNguoi;


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
                            int soCotThongTTKhachHang = 10 - 2;
                            for (int i = 0; i < listDSDatPhong.Count; i++)
                            {
                                listTienIchCanCo.Add(new List<string>());
                                for (int j = soCotThongTTKhachHang; j < listDSDatPhong[i][0].Count; j++) //Bắt đầu từ cột tiện ích thứ 1 trở đi
                                    if (Convert.ToInt32(listDSDatPhong[i][0][j].ToString().Trim()) == 1) //1 nghĩa là khách hàng chọn tiện ích
                                        listTienIchCanCo[i].Add(listTienIch[(-listDSDatPhong[i][0].Count) + (listDSDatPhong[i][0].Count - soCotThongTTKhachHang) + j][0]);
                                listTienIchCanCo[i].Add("TI202207070001"); //Tiện ích wifi mặc định có
                            }


                            //Kiểm tra các loại phòng đáp ứng đủ điều kiện của khách hàng
                            List<List<string>> listLoaiPhongDuocDeXuat = new List<List<string>>();
                            for (int l = 0; l < listDSDatPhong.Count; l++) //Lấy ra từng dòng của danh sách đặt phòng từ file import
                            {
                                var dsLoaiPhongModel = model.Loai_Phong.ToList();

                                //Kiểm tra số giường là 1
                                if (Convert.ToInt32(listDSDatPhong[l][0][7].Trim()) == 1)
                                {
                                    //Kiểm tra số người
                                    if ((Convert.ToInt32(SoNguoi[l][0]) == 2 && Convert.ToInt32(SoNguoi[l][1]) <= 2)
                                        || (Convert.ToInt32(SoNguoi[l][0]) == 1 && (Convert.ToInt32(SoNguoi[l][1]) - 1) <= 2)) //Trẻ em <= 2
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
                                        Session["error-import-file"] = "Số lượng người ở của nhóm khách số " + (l + 1) + " nằm ngoài phạm vị cho phép của khách sạn!";
                                        return RedirectToAction("Homepage", "Home");
                                    }
                                }
                                //Kiểm tra số giường là 2
                                else if (Convert.ToInt32(listDSDatPhong[l][0][7].Trim()) == 2)
                                {
                                    //Kiểm tra số người
                                    if ((Convert.ToInt32(SoNguoi[l][0]) == 2 && Convert.ToInt32(SoNguoi[l][1]) <= 4)
                                        || (Convert.ToInt32(SoNguoi[l][0]) == 1 && (Convert.ToInt32(SoNguoi[l][1]) - 1) <= 4))
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
                                    else if (Convert.ToInt32(SoNguoi[l][0]) == 3
                                        && Convert.ToInt32(SoNguoi[l][1]) <= 3)
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
                                    else if (Convert.ToInt32(SoNguoi[l][0]) == 4
                                        && Convert.ToInt32(SoNguoi[l][1]) <= 2)
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
                                        Session["error-import-file"] = "Số lượng người ở của nhóm khách số " + (l + 1) + " nằm ngoài phạm vị cho phép của khách sạn!";
                                        return RedirectToAction("Homepage", "Home");
                                    }
                                }
                                else //Số giường nhỏ hơn hoặc lớn hơn số giường mà các phòng ở khách sạn có
                                {
                                    Session["error-import-file"] = "Không có phòng nào có số giường đúng với yêu cầu của nhóm khách số " + (l + 1);
                                    return RedirectToAction("Homepage", "Home");
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
                                decimal tongthanhtoan = 0;

                                for (int i = 0; i < listDSDatPhong.Count; i++)
                                {
                                    string maphongs = listPhongDuocDeXuat[i][0];
                                    var phongs = model.Phong.First(p => p.Ma_Phong.Equals(maphongs));
                                    tongthanhtoan += phongs.Loai_Phong.Gia * Convert.ToInt32(Session["SoNgay"]);

                                    int tempIndexs = listDSDatPhong[i].Count;
                                    for (int j = tempIndexs; j <= tempIndexs; j++)
                                    {
                                        listDSDatPhong[i].Add(new List<string>());
                                        listDSDatPhong[i][j].Add(listPhongDuocDeXuat[i][0]);
                                    }
                                }
                                Session["tong-thanhtoan"] = tongthanhtoan;
                                Session["tong-coc"] = tongthanhtoan * Convert.ToDecimal(0.3);

                                Session["listDSDatPhong"] = listDSDatPhong;

                                List<string> gioitinh = new List<string>();
                                gioitinh.Add("Nam");
                                gioitinh.Add("Nữ");
                                gioitinh.Add("Khác");
                                ViewBag.Gioi_Tinh = new SelectList(gioitinh);
                                ViewBag.gioiTinh = "";

                                return View("SaveDataImport", listDSDatPhong);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Session["error-import-file"] = "Lỗi " + e.Message.ToString();
                }
            }
            return RedirectToAction("Homepage", "Home");
        }

        //View xem danh sách đề xuất phòng import
        public ActionResult SaveDataImport(List<List<List<string>>> dsDatPhongTamThoi)
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
            return RedirectToAction("Homepage", "Home");
        }

        //Lưu thông tin đặt phòng import
        [HttpPost]
        public ActionResult SaveDataImport(string hoten, string sotiencoc,
            string cmndcccd, string sdt, DateTime ngaysinh, string gioiTinh, string email,
            string diachi)
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                List<List<List<string>>> listDSDatPhong = Session["listDSDatPhong"] as List<List<List<string>>>;
                try
                {
                    decimal tienCoc = Convert.ToDecimal(sotiencoc.Trim().Replace(",", ""));
                    string tongCocs = Convert.ToDecimal(Session["tong-coc"]).ToString("0,0").Replace(".", ",");
                    string tongThanhToans = Convert.ToDecimal(Session["tong-thanhtoan"]).ToString("0,0").Replace(".", ",");

                    decimal tongCoc = Convert.ToDecimal(tongCocs.Trim().Replace(",", ""));
                    decimal tongThanhToan = Convert.ToDecimal(tongThanhToans.Trim().Replace(",", ""));

                    if (tienCoc < tongCoc || tienCoc > tongThanhToan)
                    {
                        List<string> gioitinhs = new List<string>();
                        gioitinhs.Add("Nam");
                        gioitinhs.Add("Nữ");
                        gioitinhs.Add("Khác");
                        ViewBag.Gioi_Tinh = new SelectList(gioitinhs);
                        ViewBag.gioiTinh = "";
                        Session["error-import-file"] = "Số tiền cọc không hợp lệ!, tiền cọc ít nhất là 30%:\n" + Convert.ToDecimal(Session["tong-coc"]).ToString("0,0") + " VND\nvà tối đa là:\n" + Convert.ToDecimal(Session["tong-thanhtoan"]).ToString("0,0") + " VND!";
                        return View(listDSDatPhong);
                    }

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
                    tour.Thoi_Gian_NhanPhong = Convert.ToDateTime(Session["NgayDen"].ToString());
                    tour.Thoi_Gian_TraPhong = Convert.ToDateTime(Session["NgayVe"].ToString());
                    tour.So_Tien_Coc = tongCoc;
                    tour.Tong_Thanh_Toan = tongThanhToan;
                    tour.Trang_Thai = 0; //0: chờ thanh toán, 1: hoàn thành, 2: đã hủy
                    tour.Ma_Tai_Khoan = Session["user-ma"].ToString();
                    model.TOUR.Add(tour);
                    model.SaveChanges();

                    List<List<string>> soNguoi = Session["nguoilon0treem1"] as List<List<string>>;
                    for (int i = 0; i < listDSDatPhong.Count; i++)
                    {
                        int indexMaPhong = listDSDatPhong[i].Count - 1;
                        string maTTdp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                        for (int j = 0; j < listDSDatPhong[i].Count - 1; j++)
                        {
                            if (j == 0)
                            {
                                TT_Dat_Phong ttdatphong = new TT_Dat_Phong();

                                ttdatphong.Ma_TT_Dat_Phong = maTTdp;
                                ttdatphong.Ho_Ten_KH = listDSDatPhong[i][j][1];
                                ttdatphong.CMND_CCCD_KH = listDSDatPhong[i][j][2];
                                ttdatphong.SDT_KH = listDSDatPhong[i][j][3];
                                ttdatphong.Sinh_Nhat_KH = Convert.ToDateTime(listDSDatPhong[i][j][4]);
                                ttdatphong.Gioi_Tinh_KH = Convert.ToInt32(listDSDatPhong[i][j][5]);
                                ttdatphong.Dia_Chi_KH = listDSDatPhong[i][j][6];
                                ttdatphong.Doi_Tra = 0;
                                ttdatphong.Thoi_Gian_Dat = Convert.ToDateTime(Session["NgayDen"]);
                                ttdatphong.Thoi_Gian_Doi_Tra = Convert.ToDateTime(Session["NgayVe"]);
                                ttdatphong.Ma_Phong = listDSDatPhong[i][indexMaPhong][0];
                                ttdatphong.Ma_Tai_Khoan = Session["user-ma"].ToString();
                                ttdatphong.Ma_Tour = maTour;
                                ttdatphong.Nguoi_Lon = Convert.ToInt32(soNguoi[i][0]);
                                ttdatphong.Tre_Em = Convert.ToInt32(soNguoi[i][1]);
                                ttdatphong.Trang_Thai = 0; //0: Đang ở, 1: Hoàn thành

                                var maphong = listDSDatPhong[i][indexMaPhong][0];
                                var phongs = model.Phong.First(p => p.Ma_Phong.Equals(maphong));
                                phongs.Ma_Trang_Thai = "TT202207050002";
                                decimal tongtt = phongs.Loai_Phong.Gia * Convert.ToInt32(Session["SoNgay"].ToString());

                                ttdatphong.Tong_Thanh_Toan = tongtt;
                                ttdatphong.Tien_Coc = tongtt * Convert.ToDecimal(0.3);
                                model.TT_Dat_Phong.Add(ttdatphong);
                                model.Entry(phongs).State = EntityState.Modified;
                                model.SaveChanges();
                            }
                            else
                            {
                                Nhan_Than nhanthan = new Nhan_Than();
                                nhanthan.Ma_Nhan_Than = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                                nhanthan.Ho_Ten = listDSDatPhong[i][j][1];
                                nhanthan.CMND_CCCD = listDSDatPhong[i][j][3];
                                nhanthan.Sinh_Nhat = Convert.ToDateTime(listDSDatPhong[i][j][4]);
                                nhanthan.Gioi_Tinh = Convert.ToInt32(listDSDatPhong[i][j][5]);
                                nhanthan.Dia_Chi = listDSDatPhong[i][j][6];
                                nhanthan.Moi_Quan_He = listDSDatPhong[i][j][2];
                                nhanthan.Ma_TT_Dat_Phong = maTTdp;
                                model.Nhan_Than.Add(nhanthan);
                                model.SaveChanges();
                            }
                        }

                    }

                    Session["thongbaoSuccess"] = "Đặt phòng thành công!";
                    return RedirectToAction("Homepage", "Home");
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
                    return View(listDSDatPhong);
                }
            }
            return RedirectToAction("Homepage", "Home");
        }

        //Danh sách phòng đề xuất thủ công
        [HttpPost]
        public ActionResult orderRooms(ListTienIch tienIchs, DateTime ngayDen, DateTime ngayVe,
            int soNguoiLon, int soTreEm, int soGiuong)
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                try
                {
                    int ngaynay = (int)ngayDen.Subtract(DateTime.Now).TotalDays + 1;
                    if (ngaynay < 1)
                    {
                        Session["error-import-file"] = "Ngày bắt đầu không thể thấp hơn ngày hiện tại!";
                        return RedirectToAction("Homepage", "Home");
                    }
                    int songay = (int)ngayVe.Subtract(ngayDen).TotalDays + 1;
                    if (songay < 1)
                    {
                        Session["error-import-file"] = "Ngày bắt đầu không thể thấp hơn ngày kết thúc!";
                        return RedirectToAction("Homepage", "Home");
                    }

                    //Lấy ra list tiện ích cần có để tìm phòng
                    List<string> listTienIchCanCo = new List<string>();
                    for (int i = 0; i < tienIchs.tienIch.Count; i++)
                        if (tienIchs.tienIch[i].IsChecks == true)
                            listTienIchCanCo.Add(tienIchs.tienIch[i].Ma_Tien_Ich);
                    listTienIchCanCo.Add("TI202207070001");

                    var dsLoaiPhongModel = model.Loai_Phong.ToList();
                    string loaiPhong = "";
                    if (soGiuong == 1)
                    {
                        //Kiểm tra số người
                        if ((soNguoiLon == 2 && soTreEm <= 2)
                            || (soNguoiLon == 1 && (soTreEm - 1) <= 2))
                        {
                            //Loại 1
                            foreach (var item in dsLoaiPhongModel)
                            {
                                bool kiemTraDuTienIch = true;
                                if (item.DS_Tien_Ich.Count == listTienIchCanCo.Count
                                    && item.So_Nguoi_Lon <= 2
                                    && item.So_Tre_Em <= 2
                                    && item.So_Giuong == 1) //Kiểm tra số lượng tiện ích của phòng có bằng tiện ích cần có của khách hàng hay không
                                {
                                    for (int i = 0; i < listTienIchCanCo.Count; i++) //lấy danh sách tiện ích cần có
                                    {
                                        string tienIchCanCo = listTienIchCanCo[i];
                                        var dsTienIch = item.DS_Tien_Ich.FirstOrDefault(t => t.Ma_Tien_Ich.Equals(tienIchCanCo));
                                        if (dsTienIch == null)
                                            kiemTraDuTienIch = false;
                                    }
                                    if (kiemTraDuTienIch == true) //Loại phòng phù hợp
                                    {
                                        loaiPhong = item.Ma_Loai_Phong; //dùng loại phòng này
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
                            Session["error-import-file"] = "Phòng 1 giường là không đủ cho " + soNguoiLon + " người lớn và " + soTreEm + " trẻ em ở!\nHãy chọn phòng 2 giường nhé!";
                            return RedirectToAction("Homepage", "Home");
                        }
                    }
                    else if (soGiuong == 2)
                    {
                        //Kiểm tra số người
                        if ((soNguoiLon == 2 && soTreEm <= 4) || (soNguoiLon == 1 && (soTreEm - 1) <= 4))
                        {
                            //Loại 2
                            foreach (var item in dsLoaiPhongModel)
                            {
                                bool kiemTraDuTienIch = true;
                                if (item.DS_Tien_Ich.Count == listTienIchCanCo.Count
                                    && item.So_Nguoi_Lon <= 2
                                    && item.So_Tre_Em <= 4
                                    && item.So_Giuong == 2) //Kiểm tra số lượng tiện ích, người và giường  của phòng có bằng với số lượng của khách hàng cần hay không
                                {
                                    for (int i = 0; i < listTienIchCanCo.Count; i++) //lấy danh sách tiện ích cần có
                                    {
                                        string tienIchCanCo = listTienIchCanCo[i];
                                        var dsTienIch = item.DS_Tien_Ich.FirstOrDefault(t => t.Ma_Tien_Ich.Equals(tienIchCanCo));
                                        if (dsTienIch == null)
                                            kiemTraDuTienIch = false;
                                    }
                                    if (kiemTraDuTienIch == true) //Loại phòng phù hợp
                                    {
                                        loaiPhong = item.Ma_Loai_Phong; //dùng loại phòng này
                                        break;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                        else if (soNguoiLon == 3 && soTreEm <= 3)
                        {
                            //Loại 3
                            foreach (var item in dsLoaiPhongModel)
                            {
                                bool kiemTraDuTienIch = true;
                                if (item.DS_Tien_Ich.Count == listTienIchCanCo.Count
                                    && item.So_Nguoi_Lon == 3
                                    && item.So_Tre_Em <= 3
                                    && item.So_Giuong == 2) //Kiểm tra số lượng tiện ích, người và giường  của phòng có bằng với số lượng của khách hàng cần hay không
                                {
                                    for (int i = 0; i < listTienIchCanCo.Count; i++) //lấy danh sách tiện ích cần có
                                    {
                                        string tienIchCanCo = listTienIchCanCo[i];
                                        var dsTienIch = item.DS_Tien_Ich.FirstOrDefault(t => t.Ma_Tien_Ich.Equals(tienIchCanCo));
                                        if (dsTienIch == null)
                                            kiemTraDuTienIch = false;
                                    }
                                    if (kiemTraDuTienIch == true) //Loại phòng phù hợp
                                    {
                                        loaiPhong = item.Ma_Loai_Phong; //dùng loại phòng này
                                        break;
                                    }
                                }
                                else
                                {
                                    continue;
                                }
                            }
                        }
                        else if (soNguoiLon == 4 && soTreEm <= 2)
                        {
                            //Loại 4
                            foreach (var item in dsLoaiPhongModel)
                            {
                                bool kiemTraDuTienIch = true;
                                if (item.DS_Tien_Ich.Count == listTienIchCanCo.Count
                                    && item.So_Nguoi_Lon == 4
                                    && item.So_Tre_Em <= 2
                                    && item.So_Giuong == 2) //Kiểm tra số lượng tiện ích, người và giường  của phòng có bằng với số lượng của khách hàng cần hay không
                                {
                                    for (int i = 0; i < listTienIchCanCo.Count; i++) //lấy danh sách tiện ích cần có
                                    {
                                        string tienIchCanCo = listTienIchCanCo[i];
                                        var dsTienIch = item.DS_Tien_Ich.FirstOrDefault(t => t.Ma_Tien_Ich.Equals(tienIchCanCo));
                                        if (dsTienIch == null)
                                            kiemTraDuTienIch = false;
                                    }
                                    if (kiemTraDuTienIch == true) //Loại phòng phù hợp
                                    {
                                        loaiPhong = item.Ma_Loai_Phong; //dùng loại phòng này
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
                            Session["error-import-file"] = "Phòng 2 giường là không đủ cho " + soNguoiLon + " người lớn và " + soTreEm + " trẻ em ở!\nHãy chia ra nhiều phòng nhé!";
                            return RedirectToAction("Homepage", "Home");
                        }
                    }
                    else //Số giường nhỏ hơn hoặc lớn hơn số giường mà các phòng ở khách sạn có
                    {
                        Session["error-import-file"] = "Không có phòng nào có số giường đúng với yêu cầu";
                        return RedirectToAction("Homepage", "Home");
                    }

                    //Phòng ở trạng thái trống và thuộc loại phòng của khách hàng tìm
                    var phong = model.Phong.Where(p => p.Ma_Trang_Thai.Equals("TT202207050001") && p.Ma_Loai_Phong.Equals(loaiPhong)).ToList();

                    decimal giaphong = phong[0].Loai_Phong.Gia;
                    Session["thoigian-den"] = ngayDen;
                    Session["thoigian-ve"] = ngayVe;

                    Session["songuoi-lon"] = soNguoiLon;
                    Session["songuoi-treem"] = soTreEm;
                    Session["sogiuong"] = soGiuong;

                    Session["tong-thanhtoan"] = giaphong * songay;
                    Session["tong-coc"] = (decimal)(giaphong * songay) * Convert.ToDecimal(0.3);
                    return View("SaveDataOrderRoom", phong);
                }
                catch (Exception e)
                {
                    Session["error-import-file"] = "lỗi " + e.Message.ToString();
                    return RedirectToAction("Homepage", "Home");
                }

            }
            return RedirectToAction("Homepage", "Home");
        }

        //Trang thông tin đặt phòng thủ công
        public ActionResult AddDataOrderRoom(string maphong = "")
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if (maphong.Length > 0)
                {
                    List<string> gioitinh = new List<string>();
                    gioitinh.Add("Nam");
                    gioitinh.Add("Nữ");
                    gioitinh.Add("Khác");
                    ViewBag.Gioi_Tinh = new SelectList(gioitinh);
                    ViewBag.gioiTinh = "";

                    var phong = model.Phong.First(p => p.Ma_Phong.Equals(maphong));
                    Session["maphongorder"] = phong.Ma_Phong;
                    Session["thongtinphong"] = phong;
                    int tongsonguoi = Int32.Parse(Session["songuoi-lon"].ToString()) + Int32.Parse(Session["songuoi-treem"].ToString());

                    List<Nhan_Than> nhanThans = new List<Nhan_Than>();
                    for (int i = 0; i < tongsonguoi - 1; i++)
                    {
                        Nhan_Than nhanThan = new Nhan_Than();
                        nhanThan.Ma_Nhan_Than = null;
                        nhanThan.Ho_Ten = null;
                        nhanThan.CMND_CCCD = null;
                        nhanThan.Sinh_Nhat = DateTime.Now;
                        nhanThan.Gioi_Tinh = 0;
                        nhanThan.Dia_Chi = null;
                        nhanThan.Moi_Quan_He = null;
                        nhanThans.Add(nhanThan);
                    }

                    return View(nhanThans);
                }
            }
            return RedirectToAction("Homepage", "Home");
        }

        //Lưu thông tin đặt phòng thủ công
        [HttpPost]
        public ActionResult AddDataOrderRoom(List<Nhan_Than> nhanthans, DateTime ngayDen, DateTime ngayVe,
            int soNguoiLon, int soTreEm, string tongtien, string tienCoc, string hoten, string cmndcccd,
            string sdt, DateTime ngaysinh, string gioiTinh, string diachi)
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                try
                {
                    decimal dtienCoc = Convert.ToDecimal(tienCoc.Trim().Replace(",", ""));
                    string tongCocs = Convert.ToDecimal(Session["tong-coc"]).ToString("0,0").Replace(".", ",");
                    string tongThanhToans = Convert.ToDecimal(Session["tong-thanhtoan"]).ToString("0,0").Replace(".", ",");

                    decimal tongCoc = Convert.ToDecimal(tongCocs.Trim().Replace(",", ""));
                    decimal dtongThanhToan = Convert.ToDecimal(tongThanhToans.Trim().Replace(",", ""));



                    if (dtienCoc < tongCoc || dtienCoc > dtongThanhToan)
                    {
                        List<string> gioitinhs = new List<string>();
                        gioitinhs.Add("Nam");
                        gioitinhs.Add("Nữ");
                        gioitinhs.Add("Khác");
                        ViewBag.Gioi_Tinh = new SelectList(gioitinhs);
                        ViewBag.gioiTinh = "";
                        Session["error-import-file"] = "Số tiền cọc không hợp lệ!, tiền cọc ít nhất là 30%:\n" + Convert.ToDecimal(Session["tong-coc"]).ToString("0,0") + " VND\nvà tối đa là:\n" + Convert.ToDecimal(Session["tong-thanhtoan"]).ToString("0,0") + " VND!";
                        return View(nhanthans);
                    }

                    TT_Dat_Phong ttdatphong = new TT_Dat_Phong();
                    string maTTdp = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    ttdatphong.Ma_TT_Dat_Phong = maTTdp;
                    ttdatphong.Ho_Ten_KH = hoten.Trim();
                    ttdatphong.CMND_CCCD_KH = cmndcccd.Trim();
                    ttdatphong.SDT_KH = sdt.Trim();
                    ttdatphong.Sinh_Nhat_KH = ngaysinh;

                    if (gioiTinh.Equals("Nam"))
                        ttdatphong.Gioi_Tinh_KH = 1;
                    else if (gioiTinh.Equals("Nữ"))
                        ttdatphong.Gioi_Tinh_KH = 0;
                    else ttdatphong.Gioi_Tinh_KH = 2;

                    ttdatphong.Dia_Chi_KH = diachi.Trim();
                    ttdatphong.Doi_Tra = 0;
                    ttdatphong.Thoi_Gian_Dat = ngayDen;
                    ttdatphong.Thoi_Gian_Doi_Tra = ngayVe;
                    ttdatphong.Ma_Phong = Session["maphongorder"].ToString();
                    ttdatphong.Ma_Tai_Khoan = Session["user-ma"].ToString();
                    ttdatphong.Nguoi_Lon = soNguoiLon;
                    ttdatphong.Tre_Em = soTreEm;
                    ttdatphong.Tong_Thanh_Toan = dtongThanhToan;
                    ttdatphong.Tien_Coc = tongCoc;
                    ttdatphong.Trang_Thai = 0;

                    var maphongne = Session["maphongorder"].ToString();
                    var phongne = model.Phong.Where(p => p.Ma_Phong.Equals(maphongne)).First();
                    phongne.Ma_Trang_Thai = "TT202207050002";

                    model.TT_Dat_Phong.Add(ttdatphong);
                    model.Entry(phongne).State = EntityState.Modified;

                    model.SaveChanges();
                    if (nhanthans != null)
                    {
                        for (int i = 0; i < nhanthans.Count; i++)
                        {
                            string maNT = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            string gioitinh = nhanthans[i].Ma_Nhan_Than;
                            nhanthans[i].Ma_Nhan_Than = maNT;

                            if (gioitinh.Equals("Nam"))
                                nhanthans[i].Gioi_Tinh = 1;
                            else if (gioitinh.Equals("Nữ"))
                                nhanthans[i].Gioi_Tinh = 0;
                            else nhanthans[i].Gioi_Tinh = 2;

                            nhanthans[i].Ma_TT_Dat_Phong = maTTdp;
                            model.Nhan_Than.Add(nhanthans[i]);
                            model.SaveChanges();
                        }
                    }
                    Session["thongbaoSuccess"] = "Đặt phòng thành công !";
                    return RedirectToAction("DetailtRentingRooms", "RoomManagement", new { id = Session["maphongorder"].ToString() });
                }
                catch (Exception e)
                {
                    List<string> gioitinhs = new List<string>();
                    gioitinhs.Add("Nam");
                    gioitinhs.Add("Nữ");
                    gioitinhs.Add("Khác");
                    ViewBag.Gioi_Tinh = new SelectList(gioitinhs);
                    ViewBag.gioiTinh = "";
                    Session["error-import-file"] = "Lỗi " + e.Message;
                    return View(nhanthans);
                }
            }
            return RedirectToAction("Homepage", "Home");
        }

        //Đặt phòng ở chi tiết phòng trống
        public ActionResult OrderRoomsSateEmpty(string maPhong, DateTime ngayDen, DateTime ngayVe, int soNguoiLon, int soTreEm)
        {
            if (Session["user-role"].Equals("Nhân viên"))
            {
                if (!string.IsNullOrEmpty(maPhong))
                {
                    try
                    {
                        var phong = model.Phong.FirstOrDefault(p => p.Ma_Phong.Equals(maPhong));
                        int ngaynay = (int)ngayDen.Subtract(DateTime.Now).TotalDays + 1;
                        if (ngaynay < 1)
                        {
                            Session["error-import-file"] = "Ngày bắt đầu không thể thấp hơn ngày hiện tại!";
                            return RedirectToAction("DetailtEmptyRooms", "RoomManagement", new { id = maPhong });
                        }
                        int songay = (int)ngayVe.Subtract(ngayDen).TotalDays + 1;
                        if (songay < 1)
                        {
                            Session["error-import-file"] = "Ngày bắt đầu không thể thấp hơn ngày kết thúc!";
                            return RedirectToAction("DetailtEmptyRooms", "RoomManagement", new { id = maPhong });
                        }

                        int soNguoiLonChoPhep = phong.Loai_Phong.So_Nguoi_Lon;
                        int soTreEmChoPhep = phong.Loai_Phong.So_Tre_Em;
                        if (soNguoiLon > soNguoiLonChoPhep)
                        {
                            Session["error-import-file"] = "Số người ở vượt qua giới hạn của phòng, hãy tìm phòng khác phù hợp với khách hàng!";
                            return RedirectToAction("DetailtEmptyRooms", "RoomManagement", new { id = maPhong });
                        }
                        else if (soNguoiLon == soNguoiLonChoPhep && soTreEm > soTreEmChoPhep)
                        {
                            Session["error-import-file"] = "Số người ở vượt qua giới hạn của phòng, hãy tìm phòng khác phù hợp với khách hàng!";
                            return RedirectToAction("DetailtEmptyRooms", "RoomManagement", new { id = maPhong });
                        }
                        else if (soNguoiLon < soNguoiLonChoPhep && (soTreEm - 1) > soTreEmChoPhep)
                        {
                            Session["error-import-file"] = "Số người ở vượt qua giới hạn của phòng, hãy tìm phòng khác phù hợp với khách hàng!";
                            return RedirectToAction("DetailtEmptyRooms", "RoomManagement", new { id = maPhong });
                        }

                        List<string> gioitinh = new List<string>();
                        gioitinh.Add("Nam");
                        gioitinh.Add("Nữ");
                        gioitinh.Add("Khác");
                        ViewBag.Gioi_Tinh = new SelectList(gioitinh);
                        ViewBag.gioiTinh = "";


                        Session["thoigian-ve"] = ngayVe;
                        Session["thoigian-den"] = ngayDen;
                        Session["tong-thanhtoan"] = songay * phong.Loai_Phong.Gia;
                        Session["tong-coc"] = (songay * phong.Loai_Phong.Gia) * Convert.ToDecimal(0.3);

                        Session["maphongorder"] = phong.Ma_Phong;
                        Session["thongtinphong"] = phong;
                        Session["songuoi-lon"] = soNguoiLon;
                        Session["songuoi-treem"] = soTreEm;
                        int tongsonguoi = Int32.Parse(Session["songuoi-lon"].ToString()) + Int32.Parse(Session["songuoi-treem"].ToString());

                        List<Nhan_Than> nhanThans = new List<Nhan_Than>();
                        for (int i = 0; i < tongsonguoi - 1; i++)
                        {
                            Nhan_Than nhanThan = new Nhan_Than();
                            nhanThan.Ma_Nhan_Than = null;
                            nhanThan.Ho_Ten = null;
                            nhanThan.CMND_CCCD = null;
                            nhanThan.Sinh_Nhat = DateTime.Now;
                            nhanThan.Gioi_Tinh = 0;
                            nhanThan.Dia_Chi = null;
                            nhanThan.Moi_Quan_He = null;
                            nhanThans.Add(nhanThan);
                        }
                        return View("AddDataOrderRoom", nhanThans);
                    }
                    catch (Exception e)
                    {
                        Session["error-import-file"] = "Lỗi " + e.Message;
                        return RedirectToAction("DetailtEmptyRooms", "RoomManagement", new { id = maPhong });
                    }
                }
                return RedirectToAction("ListOfEmptyRooms", "RoomManagement");
            }
            return RedirectToAction("Homepage", "Home");
        }
    }
}