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
                    }
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
                                string sheetName = dtExcelSchema.Rows[0]["TABLE_NAME"].ToString();
                                connExcel.Close();

                                connExcel.Open();
                                cmdExcel.CommandText = "SELECT * from [" + sheetName + "]";
                                odaExcel.SelectCommand = cmdExcel;
                                odaExcel.Fill(dtExcel);
                                connExcel.Close();
                            }
                        }
                    }
                    //Take list order room in file import
                    List<List<string>> list = new List<List<string>>();
                    int index = 0;
                    foreach (DataRow dr in dtExcel.Rows)
                    {
                        if (index == 0)
                        {
                            index++;
                        }
                        else
                        {
                            list.Add(new List<string>());
                            for (int i = 0; i < dtExcel.Columns.Count; i++)
                                list[index - 1].Add(dr[i].ToString());
                            index++;
                        }
                    }

                    //Take list tiện ích in db
                    var tienIch = model.Tien_Ich.ToList();
                    List<List<string>> listTienIch = new List<List<string>>();
                    for (int i = 0; i < tienIch.Count; i++)
                    {
                        listTienIch.Add(new List<string>());
                        listTienIch[i].Add(tienIch[i].Ma_Tien_Ich);
                    }
                    //take list tiện ích cần có của từng dòng in file import
                    List<List<string>> listTienIchCanCo = new List<List<string>>();
                    for (int i = 0; i < list.Count; i++)
                    {
                        listTienIchCanCo.Add(new List<string>());
                        for (int j = 12; j < list[i].Count; j++) 
                            if (Convert.ToInt32(list[i][j]) == 1)
                                listTienIchCanCo[i].Add(listTienIch[((-(list[i].Count)) + (list[i].Count - 12) + j)][0]);
                        listTienIchCanCo[i].Add("TI202207070001");
                    }

                    //Kiểm tra các loại phòng đáp ứng đủ điều kiện của khách hàng
                    List<List<string>> listLoaiPhongDuocDeXuat = new List<List<string>>();
                    for (int l = 0; l < list.Count; l++)
                    {
                        var deXuatLoaiPhong = model.Loai_Phong.ToList();
                        foreach (var item in deXuatLoaiPhong)
                        {
                            bool kiemtra = true;
                            if (item.DS_Tien_Ich.Count != listTienIchCanCo[l].Count)
                            {
                                continue;
                            }
                            else
                            {
                                for (int i = 0; i < listTienIchCanCo[l].Count; i++)
                                {
                                    string tienIchCanCo = listTienIchCanCo[l][i];
                                    var dsTienIch = item.DS_Tien_Ich.FirstOrDefault(t => t.Ma_Tien_Ich.Equals(tienIchCanCo));
                                    if (dsTienIch == null)
                                        kiemtra = false;
                                }
                                if (kiemtra == true)
                                {
                                    listLoaiPhongDuocDeXuat.Add(new List<string>());
                                    listLoaiPhongDuocDeXuat[l].Add(item.Ma_Loai_Phong);
                                    break;
                                }
                            }
                        }
                    }

                    List<List<string>> listPhongDuocDeXuat = new List<List<string>>();
                    int itemPhong = 0;
                    for (int k = 0; k < list.Count; k++)
                    {
                        var phong = model.Phong.Where(p => p.Ma_Trang_Thai.Equals("TT202207050001")).ToList();
                        if (itemPhong == 0)
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
                                    if (listPhongDuocDeXuat[j][0].Equals(item.Ma_Phong))
                                    {
                                        phongKhongTrung = false;
                                    }
                                }
                                if (phongKhongTrung == true)
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

                    if (list.Count == listPhongDuocDeXuat.Count)
                    {
                        string xepphong = "";
                        for (int i = 0; i < list.Count; i++)
                        {
                            xepphong += listPhongDuocDeXuat[i][0]+ "\n";
                        }
                        xepphong = xepphong.Trim();
                    }                    
                    else
                    {
                        Session["hetphong"] = true;
                    }
                }
            }
            catch (Exception e)
            {
                e.GetBaseException();
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