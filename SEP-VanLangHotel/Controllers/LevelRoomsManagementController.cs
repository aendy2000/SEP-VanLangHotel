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
    public class LevelRoomsManagementController : Controller
    {
        SEP25Team09Entities model = new SEP25Team09Entities();

        // GET: LevelRoomsManagement
        public ActionResult Home()
        {
            if (Session["user-role"].ToString().Equals("Quản lý"))
            {
                var loaiPhong = model.Loai_Phong.ToList();
                return View(loaiPhong);
            }
            return RedirectToAction("Homepage", "Home");
        }

        public ActionResult AddLevelRooms()
        {
            if (Session["user-role"].Equals("Quản lý")) //Tài khoản thuộc quyền Admin
            {
                List<Tien_Ich> dstienIch = new List<Tien_Ich>();
                foreach (var item in model.Tien_Ich.ToList())
                {
                    if (!item.Ma_Tien_Ich.Equals("TI202207070001"))
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
            return RedirectToAction("Homepage", "Home");
        }

        [HttpPost]
        public ActionResult AddLevelRooms(ListTienIch tienichList, string tenloaiphong, int songuoilon, int sotreem, int sogiuong, decimal gia, string mota )
        {

            if (Session["user-role"].Equals("Quản lý")) //Tài khoản thuộc quyền Admin
            {

                var loaiphong = model.Loai_Phong.Where(t => t.Ten_Loai_Phong.ToLower().Equals(tenloaiphong.ToLower().Trim())).ToList();
                if (loaiphong.Count >= 1)
                {

                    Session["error-import-file"] = "Loại phòng đã tồn tại, không được phép thêm!";
                    List<Tien_Ich> dstienIch = new List<Tien_Ich>();
                    foreach (var item in model.Tien_Ich.ToList())
                    {
                        if (!item.Ma_Tien_Ich.Equals("TI202207070001"))
                        {
                            var matienIch = item.Ma_Tien_Ich;
                            var tentienIch = item.Ten_Tien_Ich;
                            dstienIch.Add(new Tien_Ich() { Ma_Tien_Ich = matienIch, Ten_Tien_Ich = tentienIch, IsChecks = false });
                        }
                    }
                    ListTienIch listTienIch = new ListTienIch();
                    listTienIch.tienIch = dstienIch;

                    return View(listTienIch);
                    
                }
                else
                {
                    Loai_Phong levelRooms = new Loai_Phong();
                    levelRooms.Ten_Loai_Phong = tenloaiphong.Trim();
                    levelRooms.So_Nguoi_Lon = songuoilon;
                    levelRooms.So_Tre_Em = sotreem;
                    levelRooms.So_Giuong = sogiuong;
                    levelRooms.Gia = gia;
                    levelRooms.Mo_Ta = mota.Trim();

                    List<string> listTienIchCanCo = new List<string>();
                    for (int i = 0; i < tienichList.tienIch.Count; i++)
                        if (tienichList.tienIch[i].IsChecks == true)
                            listTienIchCanCo.Add(tienichList.tienIch[i].Ma_Tien_Ich);
                    listTienIchCanCo.Add("TI202207070001");
                    levelRooms.Ma_Loai_Phong = "1";

                    model.Loai_Phong.Add(levelRooms);
                    model.SaveChanges();

                    var maloaiphong = model.Loai_Phong.Where(t => t.Ten_Loai_Phong.ToLower().Equals(tenloaiphong.ToLower().Trim())).First();

                    for (int i = 0; i < listTienIchCanCo.Count; i++)
                    {
                        DS_Tien_Ich dstientich = new DS_Tien_Ich();
                        dstientich.Ma_DS_Tien_Ich = i.ToString();
                        dstientich.Ma_Loai_Phong = maloaiphong.Ma_Loai_Phong;
                        dstientich.Ma_Tien_Ich = listTienIchCanCo[i];
                        model.DS_Tien_Ich.Add(dstientich);
                        model.SaveChanges();
                    }
                    Session["thongbaoSuccess"] = "Đã thêm loại phòng " + tenloaiphong;
                    return RedirectToAction("DetailLevelRooms", new { id = maloaiphong.Ma_Loai_Phong });
                }
                
            }
            return RedirectToAction("Home");
        }

        public ActionResult DetailLevelRooms(string id)
        {

            if (Session["user-role"].Equals("Quản lý")) //Tài khoản thuộc quyền Admin
            {
                var loaiphong = model.Loai_Phong.FirstOrDefault(t => t.Ma_Loai_Phong.Equals(id));
                
                if (loaiphong != null)
                    return View(loaiphong);
            }
            return RedirectToAction("Homepage", "Home");
        }

        public ActionResult UpdateLevelRooms(string id)
        {
            if (Session["user-role"].Equals("Quản lý")) //Tài khoản thuộc quyền Admin
            {
                if(id != null)
                {
                    var loaiphong = model.Loai_Phong.FirstOrDefault(t => t.Ma_Loai_Phong.Equals(id));
                    Session["updateLoaiPhong"] = loaiphong.Ma_Loai_Phong;
                    ViewBag.Ma_Loai_Phong = new SelectList(model.Loai_Phong, "Ma_Loai_Phong");
                    ViewBag.Ma_Tien_Ich = new SelectList(model.Tien_Ich, "Ma_Tien_Ich");

                    string malp = loaiphong.Ma_Loai_Phong;
                    var ds_tienich = model.DS_Tien_Ich.Where(t => t.Ma_Loai_Phong.Equals(malp)).ToList();

                    List<Tien_Ich> dstienIch = new List<Tien_Ich>();
                    foreach (var item in model.Tien_Ich.ToList())
                    {
                        if (!item.Ma_Tien_Ich.Equals("TI202207070001"))
                        {
                            var matienIch = item.Ma_Tien_Ich;
                            var tentienIch = item.Ten_Tien_Ich;
                            var checkTienIch = ds_tienich.Where(t => t.Ma_Tien_Ich.Equals(matienIch)).ToList();
                            if (checkTienIch.Count > 0)
                                dstienIch.Add(new Tien_Ich() { Ma_Tien_Ich = matienIch, Ten_Tien_Ich = tentienIch, IsChecks = true });
                            else
                                dstienIch.Add(new Tien_Ich() { Ma_Tien_Ich = matienIch, Ten_Tien_Ich = tentienIch, IsChecks = false });
                        }
                    }
                    ListTienIch tienichList = new ListTienIch();
                    tienichList.tienIch = dstienIch;

                    return View(tienichList);
                }
            }
            return RedirectToAction("Homepage", "Home");
        }

        [HttpPost]
        public ActionResult UpdateLevelRooms(ListTienIch tienichList, string maloaiphong, string tenloaiphong, int songuoilon, int sotreem, int sogiuong, decimal gia, string mota)
        {
            if (Session["user-role"].Equals("Quản lý")) //Tài khoản thuộc quyền Admin
            {
                var loaiphong = model.Loai_Phong.Where(t => t.Ten_Loai_Phong.ToLower().Equals(tenloaiphong.ToLower().Trim())).ToList();
                if (loaiphong.Count >= 1)
                {

                    Session["error-import-file"] = "Tên loại phòng đã tồn tại!";
                    Session["updateLoaiPhong"] = loaiphong[0].Ma_Loai_Phong;
                    ViewBag.Ma_Loai_Phong = new SelectList(model.Loai_Phong, "Ma_Loai_Phong");
                    ViewBag.Ma_Tien_Ich = new SelectList(model.Tien_Ich, "Ma_Tien_Ich");

                  

                    string malp = loaiphong[0].Ma_Loai_Phong;
                    var ds_tienich = model.DS_Tien_Ich.Where(t => t.Ma_Loai_Phong.Equals(malp)).ToList();

                    List<Tien_Ich> dstienIch = new List<Tien_Ich>();
                    foreach (var item in model.Tien_Ich.ToList())
                    {
                        if (!item.Ma_Tien_Ich.Equals("TI202207070001"))
                        {
                            var matienIch = item.Ma_Tien_Ich;
                            var tentienIch = item.Ten_Tien_Ich;
                            var checkTienIch = ds_tienich.Where(t => t.Ma_Tien_Ich.Equals(matienIch)).ToList();
                            if (checkTienIch.Count > 0)
                                dstienIch.Add(new Tien_Ich() { Ma_Tien_Ich = matienIch, Ten_Tien_Ich = tentienIch, IsChecks = true });
                            else
                                dstienIch.Add(new Tien_Ich() { Ma_Tien_Ich = matienIch, Ten_Tien_Ich = tentienIch, IsChecks = false });
                        }
                    }
                    ListTienIch tienichLists = new ListTienIch();
                    tienichLists.tienIch = dstienIch;

                    return View(tienichLists);

                }
                else
                {
                    var uLoaiPhong = model.Loai_Phong.FirstOrDefault(l => l.Ma_Loai_Phong.Equals(maloaiphong));

                    uLoaiPhong.Ten_Loai_Phong = tenloaiphong.Trim();
                    uLoaiPhong.So_Nguoi_Lon = songuoilon;
                    uLoaiPhong.So_Tre_Em = sotreem;
                    uLoaiPhong.So_Giuong = sogiuong;
                    uLoaiPhong.Gia = gia;
                    uLoaiPhong.Mo_Ta = mota.Trim();

                    List<string> listTienIchCanCo = new List<string>();
                    for (int i = 0; i < tienichList.tienIch.Count; i++)
                        if (tienichList.tienIch[i].IsChecks == true)
                            listTienIchCanCo.Add(tienichList.tienIch[i].Ma_Tien_Ich);
                    listTienIchCanCo.Add("TI202207070001");

                    model.Entry(uLoaiPhong).State = EntityState.Modified;
                   
                    var ds_tienIch = model.DS_Tien_Ich.Where(t => t.Ma_Loai_Phong.Equals(maloaiphong)).ToList();

                    model.DS_Tien_Ich.RemoveRange(ds_tienIch);

                    for (int i = 0; i < listTienIchCanCo.Count; i++)
                    {
                        DS_Tien_Ich dstientich = new DS_Tien_Ich();
                        dstientich.Ma_DS_Tien_Ich = i.ToString();
                        dstientich.Ma_Loai_Phong = maloaiphong;
                        dstientich.Ma_Tien_Ich = listTienIchCanCo[i];
                        model.DS_Tien_Ich.Add(dstientich);
                        model.SaveChanges();
                    }
                    Session["thongbaoSuccess"] = "Đã sửa loại phòng " + tenloaiphong;
                    return RedirectToAction("DetailLevelRooms", new { id = maloaiphong });
                }
            }
            return RedirectToAction("Home");
        }

        public ActionResult RemoveLevelRooms(string id)
        {
            if (Session["user-role"].Equals("Quản lý"))
            {
                var loaiphong = model.Loai_Phong.FirstOrDefault(t => t.Ma_Loai_Phong.Equals(id));
                var phong = model.Phong.FirstOrDefault(t => t.Ma_Loai_Phong.Equals(id));
                if(loaiphong != null)
                {
                    string tenloaiphong = loaiphong.Ten_Loai_Phong;
                    if (phong != null)
                    {
                        Session["error-import-file"] = "Loại phòng đang được sử dụng, không được phép xóa!";
                        return RedirectToAction("Home");

                    }
                    else
                    {
                        var dstienich = model.DS_Tien_Ich.Where(t => t.Ma_Loai_Phong.Equals(id)).ToList();
                        model.DS_Tien_Ich.RemoveRange(dstienich);
                        model.SaveChanges();
                        model.Loai_Phong.Remove(loaiphong);
                        model.SaveChanges();
                        Session["thongbaoSuccess"] = "Đã xóa loại phòng " + tenloaiphong;
                        return RedirectToAction("Home");
                    }
                }
            }
            return RedirectToAction("Home");
        }

    }
}