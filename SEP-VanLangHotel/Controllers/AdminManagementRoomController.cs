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
    public class AdminManagementRoomController : Controller
    {
        SEP25Team09Entities model = new SEP25Team09Entities();
        private static string ApiKey = "AIzaSyCCu1XV1IcQU2pQ3l0TetO5FTs4zT59kKk";
        private static string Bucket = "sep-project-2022-6fbfd.appspot.com";
        private static string AuthEmail = "sepproject2022@gmail.com";
        private static string AuthPassword = "sepproject2022";

        // GET: AdminManagementRoom
        public ActionResult Home()
        {
            if (Session["user-role"].Equals("Quản lý"))
            {
                var listPhong = model.Phong.ToList();
                return View(listPhong);
            }
            return RedirectToAction("Home", "AdminManagementRoom");
        }

        public ActionResult AddRoom()
        {
            if (Session["user-role"].Equals("Quản lý"))
            {
                Phong phongs = new Phong();
                var clients = model.Loai_Phong
                    .Select(s => new
                    {
                        Text = s.Ten_Loai_Phong + " - " + s.Mo_Ta,
                        Value = s.Ma_Loai_Phong
                    }).ToList();

                ViewBag.Ma_Loai_Phong = new SelectList(clients, "Value", "Text");
                return View(phongs);
            }
            return RedirectToAction("Home", "AdminManagementRoom");
        }
        [HttpPost]
        public async Task<ActionResult> AddRoom(Phong phongs, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string tenphong = phongs.So_Phong.ToLower().Trim();
                    var check = model.Phong.Where(t => t.So_Phong.ToLower().Trim().Equals(tenphong)).ToList();
                    if (check.Count > 0)
                    {
                        Session["error-import-file"] = "Phòng đã tồn tại, thử lại với tên khác!";
                        var clientsS = model.Loai_Phong
                                            .Select(s => new
                                            {
                                                Text = s.Ten_Loai_Phong + " - " + s.Mo_Ta,
                                                Value = s.Ma_Loai_Phong
                                            }).ToList();

                        ViewBag.Ma_Loai_Phong = new SelectList(clientsS, "Value", "Text"); return View(phongs);
                    }
                    FileStream stream;
                    if (file != null)
                    {
                        if (file.ContentLength > 0)
                        {
                            const string src = "abcdefghijklmnopqrstuvwxyz0123456789";
                            int length = 30;
                            var sb = new StringBuilder();
                            Random RNG = new Random();
                            for (var i = 0; i < length; i++)
                            {
                                var c = src[RNG.Next(0, src.Length)];
                                sb.Append(c);
                            }

                            string path = Path.Combine(Server.MapPath("~/Content/images/"), sb.ToString().Trim() + file.FileName); ;
                            file.SaveAs(path);
                            stream = new FileStream(Path.Combine(path), FileMode.Open);
                            //await Task.Run(() => Upload(stream, sb.ToString().Trim() + file.FileName));

                            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
                            var a = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);

                            var cancellation = new CancellationTokenSource();

                            var task = new FirebaseStorage(
                                Bucket,
                                new FirebaseStorageOptions
                                {
                                    AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                                    ThrowOnCancel = true
                                })
                                .Child("images")
                                .Child(sb.ToString().Trim() + file.FileName)
                                .PutAsync(stream, cancellation.Token);
                            try
                            {
                                string link = await task;
                                phongs.Hinh_Anh = link;
                                System.IO.File.Delete(path);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }

                        }
                    }
                    else
                    {
                        phongs.Hinh_Anh = "https://firebasestorage.googleapis.com/v0/b/sep-project-2022-6fbfd.appspot.com/o/images%2Fjessica-jeansoulin-icon-hotel-ortho.jpg?alt=media&token=2f27626f-ef94-44be-b7e8-cdfe0616c38a";
                    }
                    phongs.Ma_Phong = "1";
                    phongs.Ma_Khach_San = "KS202207010001";
                    phongs.Ma_Trang_Thai = "TT202207050001";
                    model.Phong.Add(phongs);
                    model.SaveChanges();
                    Session["thongbaoSuccess"] = "Thêm phòng thành công!";
                    return RedirectToAction("DetailRoom", "AdminManagementRoom", new {id = model.Phong.FirstOrDefault(p => p.So_Phong.ToLower().Trim().Equals(tenphong)).Ma_Phong });
                }
                catch (Exception e)
                {
                    Session["error-import-file"] = "Lỗi: " + e.Message;
                    var clientsS = model.Loai_Phong
                                        .Select(s => new
                                        {
                                            Text = s.Ten_Loai_Phong + " - " + s.Mo_Ta,
                                            Value = s.Ma_Loai_Phong
                                        }).ToList();

                    ViewBag.Ma_Loai_Phong = new SelectList(clientsS, "Value", "Text"); return View(phongs);
                }
            }
            var clients = model.Loai_Phong
                                .Select(s => new
                                {
                                    Text = s.Ten_Loai_Phong + " - " + s.Mo_Ta,
                                    Value = s.Ma_Loai_Phong
                                }).ToList();

            ViewBag.Ma_Loai_Phong = new SelectList(clients, "Value", "Text"); return View(phongs);
        }
        public ActionResult DetailRoom(string id)
        {
            if (Session["user-role"].Equals("Quản lý"))
            {
                var phongs = model.Phong.FirstOrDefault(t => t.Ma_Phong.Equals(id));
                if (phongs != null)
                    return View(phongs);
            }
            return RedirectToAction("Home", "AdminManagementRoom");
        }
        public ActionResult UpdateRoom(string id)
        {
            if (Session["user-role"].Equals("Quản lý"))
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var phongs = model.Phong.FirstOrDefault(t => t.Ma_Phong.Equals(id));
                    if (phongs != null)
                    {
                        var clients = model.Loai_Phong
                                            .Select(s => new
                                            {
                                                Text = s.Ten_Loai_Phong + " - " + s.Mo_Ta,
                                                Value = s.Ma_Loai_Phong
                                            }).ToList();

                        ViewBag.Ma_Loai_Phong = new SelectList(clients, "Value", "Text"); return View(phongs);
                    }
                }
            }
            return RedirectToAction("Home", "AdminManagementRoom");
        }
        [HttpPost]
        public async Task<ActionResult> UpdateRoom(Phong phongs, HttpPostedFileBase file)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string tenphong = phongs.So_Phong.ToLower().Trim();
                    string mphong = phongs.Ma_Phong;
                    var check = model.Phong.Where(t => t.So_Phong.ToLower().Trim().Equals(tenphong) && !t.Ma_Phong.Equals(mphong)).ToList();
                    if (check.Count > 0)
                    {
                        Session["error-import-file"] = "Phòng đã tồn tại, thử lại với tên khác!";
                        var clientsS = model.Loai_Phong
                                            .Select(s => new
                                            {
                                                Text = s.Ten_Loai_Phong + " - " + s.Mo_Ta,
                                                Value = s.Ma_Loai_Phong
                                            }).ToList();

                        ViewBag.Ma_Loai_Phong = new SelectList(clientsS, "Value", "Text"); return View(phongs);
                    }
                    FileStream stream;
                    if (file != null)
                    {
                        if (file.ContentLength > 0)
                        {
                            const string src = "abcdefghijklmnopqrstuvwxyz0123456789";
                            int length = 30;
                            var sb = new StringBuilder();
                            Random RNG = new Random();
                            for (var i = 0; i < length; i++)
                            {
                                var c = src[RNG.Next(0, src.Length)];
                                sb.Append(c);
                            }

                            string path = Path.Combine(Server.MapPath("~/Content/images/"), sb.ToString().Trim() + file.FileName); ;
                            file.SaveAs(path);
                            stream = new FileStream(Path.Combine(path), FileMode.Open);
                            var auth = new FirebaseAuthProvider(new FirebaseConfig(ApiKey));
                            var a = await auth.SignInWithEmailAndPasswordAsync(AuthEmail, AuthPassword);
                            var cancellation = new CancellationTokenSource();

                            var task = new FirebaseStorage(
                                Bucket,
                                new FirebaseStorageOptions
                                {
                                    AuthTokenAsyncFactory = () => Task.FromResult(a.FirebaseToken),
                                    ThrowOnCancel = true
                                })
                                .Child("images")
                                .Child(sb.ToString().Trim() + file.FileName)
                                .PutAsync(stream, cancellation.Token);
                            try
                            {
                                string link = await task;
                                phongs.Hinh_Anh = link;
                                System.IO.File.Delete(path);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }
                        }
                    }
                    model.Entry(phongs).State = EntityState.Modified;
                    model.SaveChanges();
                    Session["thongbaoSuccess"] = "Cập nhật phòng thành công!";
                    return RedirectToAction("DetailRoom", "AdminManagementRoom", new { id = phongs.Ma_Phong });
                }
                catch (Exception e)
                {
                    Session["error-import-file"] = "Lỗi: " + e.Message;
                    var clientsS = model.Loai_Phong
                                        .Select(s => new
                                        {
                                            Text = s.Ten_Loai_Phong + " - " + s.Mo_Ta,
                                            Value = s.Ma_Loai_Phong
                                        }).ToList();

                    ViewBag.Ma_Loai_Phong = new SelectList(clientsS, "Value", "Text"); return View(phongs);
                }
            }
            var clients = model.Loai_Phong
                                .Select(s => new
                                {
                                    Text = s.Ten_Loai_Phong + " - " + s.Mo_Ta,
                                    Value = s.Ma_Loai_Phong
                                }).ToList();

            ViewBag.Ma_Loai_Phong = new SelectList(clients, "Value", "Text"); return View(phongs);
        }

        public ActionResult DeleteRoom(string id)
        {
            if (Session["user-role"].Equals("Quản lý"))
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var phongs = model.Phong.FirstOrDefault(t => t.Ma_Phong.Equals(id));
                    if (phongs != null)
                    {
                        string maphong = phongs.Ma_Phong;
                        var ttdatphong = model.TT_Dat_Phong.Where(t => t.Ma_Phong.Equals(maphong)).ToList();
                        var ttdoiphong = model.TT_Doi_Phong.Where(t => t.Ma_Phong.Equals(maphong)).ToList();
                        if (ttdatphong.Count > 0 || ttdoiphong.Count > 0)
                        {
                            Session["error-import-file"] = "Phòng đã được sử dụng không thể xóa!";
                            return RedirectToAction("Home", "AdminManagementRoom");
                        }
                        model.Phong.Remove(phongs);
                        model.SaveChanges();
                        Session["thongbaoSuccess"] = "Xóa phòng thành công!";
                        return RedirectToAction("Home", "AdminManagementRoom");
                    }
                }
                return RedirectToAction("Home", "AdminManagementRoom");
            }
            return RedirectToAction("Home", "AdminManagementRoom");
        }
    }
}