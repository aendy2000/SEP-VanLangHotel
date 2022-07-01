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
    public class AccountManagementController : Controller
    {
        // GET: AcountManagement
        SEP25Team09Entities model = new SEP25Team09Entities();
        private static string ApiKey = "AIzaSyCCu1XV1IcQU2pQ3l0TetO5FTs4zT59kKk";
        private static string Bucket = "sep-project-2022-6fbfd.appspot.com";
        private static string AuthEmail = "sepproject2022@gmail.com";
        private static string AuthPassword = "sepproject2022";

        public ActionResult Home() //trang chủ ql tài khoản của Admin
        {
            if (Session["user-role"].Equals("Quản lý")) //Tài khoản thuộc quyền Admin
            {
                var taikhoan = model.Tai_Khoan.ToList();
                return View(taikhoan);
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult SearchAccount(string keyword) //Tìm kiếm tài khoản
        {
            if (Session["user-role"].Equals("Quản lý")) //Tài khoản thuộc quyền Admin
            {
                if (keyword.Equals("") || keyword == null)
                {
                    var taikhoan = model.Tai_Khoan.ToList();
                    return View("Home", taikhoan);
                }
                else
                {
                    var taikhoan = model.Tai_Khoan.Where(t => t.Ten_Dang_Nhap.ToUpper().Contains(keyword.ToUpper())).ToList();
                    ViewBag.Keywork = keyword;
                    return View("Home", taikhoan);
                }
            }
            return RedirectToAction("Homepage", "Home");
        }

        public ActionResult AddNewAccount() //Thêm mới 1 tài khoản - Admin
        {
            if (Session["user-role"].Equals("Quản lý")) //Tài khoản thuộc quyền Admin
            {
                Tai_Khoan newAccount = new Tai_Khoan();
                ViewBag.Ma_Quyen = new SelectList(model.Quyen, "Ma_Quyen", "Ten_Quyen");
                ViewBag.Ma_Khach_San = new SelectList(model.Khach_San, "Ma_Khach_San", "Ten_Khach_San");
                List<string> gioitinh = new List<string>();
                gioitinh.Add("Nam");
                gioitinh.Add("Nữ");
                gioitinh.Add("Khác");
                ViewBag.Gioi_Tinh = new SelectList(gioitinh);
                ViewBag.gioiTinh = "";
                return View(newAccount);
            }
            return RedirectToAction("Homepage", "Home");
        }
        [HttpPost]
        public async Task<ActionResult> AddNewAccount(Tai_Khoan newAccount, string gioiTinh, HttpPostedFileBase file)
        {
            if (Session["user-role"].Equals("Quản lý")) //Tài khoản thuộc quyền Admin
            {
                if (ModelState.IsValid)
                {
                    try
                    {
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
                                    newAccount.Avatar = link;
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
                            newAccount.Avatar = "https://firebasestorage.googleapis.com/v0/b/sep-project-2022-6fbfd.appspot.com/o/images%2FDefaultAvatar.png?alt=media&token=c1099848-26b6-4ddb-8cce-209ec415fe7a";
                        }

                        newAccount.Ma_Tai_Khoan = "1"; //Mã tạm thời (Trigger tại DB sẽ thay đổi sau khi thêm thành công tài khoán)
                        newAccount.Lock = 0;
                        newAccount.Ma_Khach_San = "KS202207010001";
                        newAccount.Gioi_Tinh = gioiTinh.Equals("Nam") ? 1 : (gioiTinh.Equals("Nữ") ? 0 : 3);
                        model.Tai_Khoan.Add(newAccount);
                        model.SaveChanges();
                        Session["addnewaccountsuccess"] = true;
                    }
                    catch (DbEntityValidationException e) //Nhận thông tin lỗi thêm tài khoản bị thất bại
                    {
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                eve.Entry.Entity.GetType().Name, eve.Entry.State);
                            foreach (var ve in eve.ValidationErrors)
                            {
                                Console.WriteLine("- Property: \"{0}\", Value: \"{1}\", Error: \"{2}\"",
                                    ve.PropertyName,
                                    eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName),
                                    ve.ErrorMessage);
                            }
                        }
                        throw;
                    }
                    return RedirectToAction("Home");
                } //Thêm thất bại sẽ:
                ViewBag.Ma_Quyen = new SelectList(model.Quyen, "Ma_Quyen", "Ten_Quyen");
                ViewBag.Ma_Khach_San = new SelectList(model.Khach_San, "Ma_Khach_San", "Ten_Khach_San");
                return View(newAccount);
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult UpdateAccount(string id) //Cập nhật thông tin tài khoản - Admin
        {
            if (Session["user-role"].Equals("Quản lý")) //Tài khoản thuộc quyền Admin
            {
                if (id != null)
                {
                    var taikhoan = model.Tai_Khoan.FirstOrDefault(t => t.Ma_Tai_Khoan.Equals(id));
                    ViewBag.Ma_Quyen = new SelectList(model.Quyen, "Ma_Quyen", "Ten_Quyen");
                    ViewBag.Ma_Khach_San = new SelectList(model.Khach_San, "Ma_Khach_San", "Ten_Khach_San");
                    ViewBag.Birthday = taikhoan.Sinh_Nhat.ToString("dd/MM/yyyy");
                    List<string> gioitinh = new List<string>();
                    if (taikhoan.Gioi_Tinh == 1)
                    {
                        gioitinh.Add("Nam");
                        gioitinh.Add("Nữ");
                        gioitinh.Add("Khác");
                    }
                    else if (taikhoan.Gioi_Tinh == 0)
                    {
                        gioitinh.Add("Nữ");
                        gioitinh.Add("Nam");
                        gioitinh.Add("Khác");
                    }
                    else
                    {
                        gioitinh.Add("Khác");
                        gioitinh.Add("Nam");
                        gioitinh.Add("Nữ");
                    }

                    ViewBag.Gioi_Tinh = new SelectList(gioitinh);
                    return View(taikhoan);
                }
            }
            return RedirectToAction("Home", "AccountManagement");
        }
        [HttpPost]
        public async Task<ActionResult> UpdateAccount(Tai_Khoan taikhoan, string gioiTinh, HttpPostedFileBase file)
        {
            if (Session["user-role"].Equals("Quản lý")) //Tài khoản thuộc quyền Admin
            {
                if (ModelState.IsValid)
                {
                    try
                    {
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
                                    taikhoan.Avatar = link;
                                    System.IO.File.Delete(path);
                                }
                                catch (Exception e)
                                {
                                    Console.WriteLine(e);
                                }
                            }
                        }
                        taikhoan.Gioi_Tinh = gioiTinh.Equals("Nam") ? 1 : (gioiTinh.Equals("Nữ") ? 0 : 3);
                        model.Entry(taikhoan).State = EntityState.Modified;
                        model.SaveChanges();
                        if (taikhoan.Ten_Dang_Nhap.Equals(Session["user-id"]))
                        {
                            Session["user-vatatar"] = taikhoan.Avatar;
                            return RedirectToAction("Information", new { id = taikhoan.Ma_Tai_Khoan });
                        } 
                        else
                        {
                            return RedirectToAction("DetailtAccount", new { id = taikhoan.Ma_Tai_Khoan });
                        }
                    }
                    catch (DbEntityValidationException e)
                    {
                        foreach (var eve in e.EntityValidationErrors)
                        {
                            Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                                eve.Entry.Entity.GetType().Name, eve.Entry.State);
                            foreach (var ve in eve.ValidationErrors)
                            {
                                Console.WriteLine("- Property: \"{0}\", Value: \"{1}\", Error: \"{2}\"",
                                    ve.PropertyName,
                                    eve.Entry.CurrentValues.GetValue<object>(ve.PropertyName),
                                    ve.ErrorMessage);
                            }
                        }
                        throw;
                    }
                }
                ViewBag.Ma_Quyen = new SelectList(model.Quyen, "Ma_Quyen", "Ten_Quyen");
                ViewBag.Ma_Khach_San = new SelectList(model.Khach_San, "Ma_Khach_San", "Ten_Khach_San");
                List<string> gioitinh = new List<string>();
                if (taikhoan.Gioi_Tinh == 1)
                {
                    gioitinh.Add("Nam");
                    gioitinh.Add("Nữ");
                    gioitinh.Add("Khác");
                }
                else if (taikhoan.Gioi_Tinh == 0)
                {
                    gioitinh.Add("Nữ");
                    gioitinh.Add("Nam");
                    gioitinh.Add("Khác");
                }
                else
                {
                    gioitinh.Add("Khác");
                    gioitinh.Add("Nam");
                    gioitinh.Add("Nữ");
                }

                ViewBag.Gioi_Tinh = new SelectList(gioitinh);
                return View(taikhoan);
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult DetailtAccount(string id) //Xem thông tin chi tiết của các tài khoản trên hệ thống - Admin
        {
            if (Session["user-role"].Equals("Quản lý")) //Tài khoản thuộc quyền Admin
            {
                var taikhoan = model.Tai_Khoan.FirstOrDefault(t => t.Ma_Tai_Khoan.Equals(id));
                if (taikhoan != null)
                    return View(taikhoan);
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult Information() //Xem thông tin cá nhân - Admin và Nhân viên
        {
            string id = Session["user-id"].ToString();
            var taikhoan = model.Tai_Khoan.FirstOrDefault(t => t.Ten_Dang_Nhap.Equals(id));
            if (taikhoan != null)
                return View(taikhoan);
            return RedirectToAction("Home");
        }
        public ActionResult RemoveAccount(string id)
        {
            Session["taikhoan-user-deleted"] = null;
            Session["taikhoan-admintop1-deleted"] = null;
            if (Session["user-role"].ToString().Equals("Quản lý"))
            {
                var taikhoan = model.Tai_Khoan.FirstOrDefault(t => t.Ma_Tai_Khoan.Equals(id));
                if (taikhoan != null)
                {
                    if (taikhoan.Quyen.Ten_Quyen.Equals("Quản lý") && !taikhoan.Ten_Dang_Nhap.Equals("Quản lý"))
                    {
                        model.Tai_Khoan.Remove(taikhoan);
                        model.SaveChanges();
                        return RedirectToAction("Home");
                    }
                    else if (taikhoan.Ten_Dang_Nhap.Equals("Quản lý"))
                    {
                        //Khong duoc phep xoa tk chu khach san
                        Session["taikhoan-admintop1-deleted"] = true;
                        return RedirectToAction("Home");
                    }
                    //Khong duoc phep xoa tk user
                    Session["taikhoan-user-deleted"] = true;
                }
                return RedirectToAction("Home");
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult LockAccount(string id)
        {
            if (Session["user-role"].ToString().Equals("Quản lý"))
            {
                var taikhoan = model.Tai_Khoan.FirstOrDefault(t => t.Ma_Tai_Khoan.Equals(id));
                if (taikhoan != null)
                {
                    if (taikhoan.Lock == 0)
                        taikhoan.Lock = 1;
                    else
                        taikhoan.Lock = 0;
                    model.SaveChanges();
                    return RedirectToAction("DetailtAccount", new { id = taikhoan.Ma_Tai_Khoan });
                }
                return RedirectToAction("Home");
            }
            return RedirectToAction("Homepage", "Home");
        }
        public ActionResult ChangePassword(string id)
        {
            if (Session["user-role"].ToString().Equals("Quản lý"))
            {
                var taikhoan = model.Tai_Khoan.FirstOrDefault(t => t.Ten_Dang_Nhap.Equals(id));
                if (taikhoan != null)
                {
                    return View(taikhoan);
                }
                return RedirectToAction("Home");
            }
            return RedirectToAction("Homepage", "Home");
        }

        [HttpPost]
        public ActionResult ChangePassword(string id, string password = null, string newpassword = null, string renewpassword = null)
        {
            var taikhoan = model.Tai_Khoan.FirstOrDefault(t => t.Ma_Tai_Khoan.Equals(id));
            if (Session["user-role"].ToString().Equals("Quản lý"))
            {
                if (taikhoan != null)
                {
                    if (taikhoan.Mat_Khau.Equals(password))
                    {
                        Session["passcu-false"] = null;
                        string newpass = (password.Trim().Equals(newpassword.Trim())) ? "trung" : (newpassword.Trim().Equals(renewpassword.Trim())) ? "true" : "false";
                        if (!newpass.Equals("trung"))
                        {
                            Session["newpass-trung"] = null;
                            if (newpass.Equals("true"))
                            {
                                Session["newpass-khongtrung"] = null;
                                taikhoan.Mat_Khau = newpassword;
                                model.SaveChanges();
                                return RedirectToAction("Homepage", "Home");
                            }
                            Session["newpass-khongtrung"] = true; //mật khẩu mới không trùng nhau
                            return View(taikhoan);
                        }
                        Session["newpass-trung"] = true; // trùng với mk cũ
                        return View(taikhoan);
                    }
                    Session["passcu-false"] = true; //mk cũ sai
                    return View(taikhoan);
                }
                return RedirectToAction("Homepage", "Home");
            }
            return RedirectToAction("Homepage", "Home");
        }
        public async void Upload(FileStream stream, string fileName)
        {
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
                .Child(fileName)
                .PutAsync(stream, cancellation.Token);
            try
            {
                string link = await task;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}