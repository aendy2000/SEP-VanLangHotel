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
                        //Kiem tra giới hạn số lượng người và giường


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
                        for (int i = 0; i < listDSDatPhong.Count; i++)
                        {
                            listTienIchCanCo.Add(new List<string>());
                            for (int j = 12; j < listDSDatPhong[i].Count; j++) //Bắt đầu từ cột tiện ích thứ 1 trở đi
                                if (Convert.ToInt32(listDSDatPhong[i][j].ToString().Trim()) == 1) //1 nghĩa là khách hàng chọn tiện ích
                                    listTienIchCanCo[i].Add(listTienIch[((-(listDSDatPhong[i].Count)) + (listDSDatPhong[i].Count - 12) + j)][0]);
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
                            string hshs = "";
                            Session["error-import-file"] = "Có danh sách rồi !!";
                            List<List<string>> dsDeXuatPhong = new List<List<string>>();
                            for (int i = 0; i < listDSDatPhong.Count; i++)
                            {
                                //Đưa phòng và tt cá nhân vào 1 danh sách dsDeXuatPhong
                                hshs += listPhongDuocDeXuat[i][0] + "\n";
                            }
                            hshs += Session["error-import-file"];
                            hshs.Trim();
                            return RedirectToAction("Homepage");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Session["error-import-file"] = "Lỗi " + e.Message.ToString();
                return RedirectToAction("Homepage");
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