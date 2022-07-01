using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SEP_VanLangHotel.Models;
using System.Net;
using System.Net.Mail;

namespace SEP_VanLangHotel.Controllers
{
    public class AuthController : Controller
    {
        // GET: Admin/LoginLogout
        SEP25Team09Entities model = new SEP25Team09Entities();
        public ActionResult Login() //Trang đăng nhập
        {
            Session["username-incorrect"] = null;
            Session["password-incorrect"] = null;
            ViewBag.Title = "Đăng Nhập";
            return View();
        }
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            var user = model.Tai_Khoan.FirstOrDefault(u => u.Ten_Dang_Nhap.ToUpper().Equals(username.ToUpper()));
            if (user != null) //Tài khoản tồn tại
            {

                Session["username-incorrect"] = null;
                if (user.Mat_Khau.Equals(password)) //Mật khẩu đúng
                {
                    Session["password-incorrect"] = null;
                    if (user.Lock == 0) //Tài khoản không bị khóa
                    {
                        Session["user-lock"] = null;
                        Session["user-fullname"] = user.Ho_Va_Ten;
                        Session["user-id"] = user.Ten_Dang_Nhap;
                        Session["user-role"] = user.Quyen.Ten_Quyen;
                        @Session["user-vatatar"] = user.Avatar;
                        return RedirectToAction("Homepage", "Home");
                    }
                    Session["user-lock"] = true;
                    return View();
                }
                Session["password-incorrect"] = true;
                return View();
            }
            Session["username-incorrect"] = true;
            Session["password-incorrect"] = true;
            return View();
        }
        public ActionResult Logout() //Đăng xuất
        {
            Session.Clear();
            return RedirectToAction("Login");
        }
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ForgotPassword(string tendangnhap)
        {
            Session["thongbao-taikhoan-null"] = null;
            Session["thongbao-taikhoan-incorrect"] = null;


            if (!tendangnhap.Equals("") || tendangnhap != null) //Tài khoản đã được nhập
            {
                var taikhoan = model.Tai_Khoan.FirstOrDefault(c => c.Ten_Dang_Nhap.ToUpper().Equals(tendangnhap.Trim().ToUpper()));
                if (taikhoan != null) //Tài khoản tồn tại
                {
                    Random r = new Random();
                    int range = 6;
                    string code = "";
                    while (range >= 1)
                    {
                        int ranD = r.Next(0, 9);
                        code += ranD;
                        range -= 1;
                    }
                    taikhoan.Verify_Password = code; //Tạo mã xác minh đặt lại mật khẩu
                    string outoffdate = (ulong.Parse(DateTime.Now.ToString("yyyyMMddHHmmss")) + 1000).ToString(); //Tạo thời hạn sử dụng mã
                    taikhoan.OutOfDate_Code = outoffdate;
                    model.SaveChanges();

                    //Khởi tạo nội dung gửi mail
                    MailMessage mailmea = new MailMessage();
                    mailmea.To.Add(taikhoan.Email);
                    mailmea.From = new MailAddress(@"vanlanghotel@gmail.com");
                    mailmea.Subject = "Đặt lại mật khẩu - Văn Lang Hotel";
                    mailmea.IsBodyHtml = true;
                    mailmea.Body = "<font size=5>Mã xác nhận của bạn là: </font><br>" + "<font size=20><b>   " + code + "</b></font>";

                    //Phương thức gửi mail
                    SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                    smtp.UseDefaultCredentials = true;
                    smtp.EnableSsl = true;
                    smtp.Port = 25;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Credentials = new NetworkCredential(@"vanlanghotel@gmail.com", "gqhyxnzkauoxoetx"); //Email, mật khẩu ứng dụng
                    try
                    {
                        smtp.Send(mailmea);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.ToString());
                    }

                    return RedirectToAction("VerifyEmail", new { id = taikhoan.Ma_Tai_Khoan });


                }
                Session["thongbao-taikhoan-incorrect"] = true;
                return View();
            }
            Session["thongbao-taikhoan-null"] = true;
            return View();
        }
        public ActionResult VerifyEmail(string id)
        {
            var taikhoan = model.Tai_Khoan.FirstOrDefault(t => t.Ma_Tai_Khoan.Equals(id)); //Lấy tài khoản cần xác minh mã đặt lại mật khẩu
            if (taikhoan != null)
                return View(taikhoan);
            else
                return RedirectToAction("ForgotPassword");
        }
        [HttpPost]
        public ActionResult VerifyEmail(string idtaikhoan, string maxacnhan = null)
        {
            Session["thongbao-outoffdate"] = null;
            Session["thongbao-maxacnhan-null"] = null;
            Session["thongbao-maxacnhan-incorrect"] = null;
            var taikhoan = model.Tai_Khoan.FirstOrDefault(t => t.Ma_Tai_Khoan.Equals(idtaikhoan));

            if (taikhoan.OutOfDate_Code != null && !taikhoan.OutOfDate_Code.Equals("")) //Mã tồn tại
            {
                ulong tgianhethan = ulong.Parse(taikhoan.OutOfDate_Code);
                if (tgianhethan >= ulong.Parse(DateTime.Now.ToString("yyyyMMddHHmmss"))) //Kiểm tra thời gian sử dụng mã
                {
                    if (!maxacnhan.Equals("")) //Mã xác nhận không bị bỏ trống
                    {
                        if (taikhoan.Verify_Password.Equals(maxacnhan.Trim())) //Mã xác nhận đúng
                        {
                            taikhoan.Verify_Password = null;
                            taikhoan.OutOfDate_Code = null;
                            model.SaveChanges();
                            return RedirectToAction("NewPassword", new { id = taikhoan.Ma_Tai_Khoan }); //Đi đến trang đặt lại mật khẩu
                        }
                        else
                        {
                            Session["thongbao-maxacnhan-incorrect"] = true; //Mã xác nhận không đúng
                            return View(taikhoan);
                        }
                    }
                    Session["thongbao-maxacnhan-null"] = true; //mã xác nhận bị bỏ trống
                    return View(taikhoan);
                }
                Session["thongbao-outoffdate"] = true; //Ma da het han
                taikhoan.Verify_Password = null; //Xóa bỏ mã
                taikhoan.OutOfDate_Code = null; //Xóa bỏ thời gian chờ mã
                model.SaveChanges();
            }
            return RedirectToAction("ForgotPassword");
        }

        public ActionResult NewPassword(string id)
        {
            //Chọn tài khoản cấp mật khẩu mới
            var taikhoan = model.Tai_Khoan.FirstOrDefault(t => t.Ma_Tai_Khoan.Equals(id));
            if (taikhoan != null)
                return View(taikhoan);
            else
                return RedirectToAction("ForgotPassword");
        }
        //Sau khi nhập mã thành công hệ chuyển đến nơi đặt lại mật khẩu
        [HttpPost]
        public ActionResult NewPassword(string idtaikhoan, string matkhaumoi = null, string nhaplaimatkhaumoi = null)
        {
            //Kiểm tra trường mật khẩu null và kiểm tra 2 trường mật khẩu mới có trùng nhau hay không
            Session["matkhaumoi-null"] = null;
            Session["sosanh-matkhaumoi-incorrect"] = null;
            var taikhoan = model.Tai_Khoan.FirstOrDefault(t => t.Ma_Tai_Khoan.Equals(idtaikhoan));
            //Nếu mật khảu mới không bỏ trống
            if (!matkhaumoi.Equals("") || matkhaumoi != null)
            {
                //Nếu mật khẩu mới trùng khớp
                if (matkhaumoi.Equals(nhaplaimatkhaumoi))
                {
                    taikhoan.Mat_Khau = matkhaumoi;
                    model.SaveChanges();
                    Session["laylaimatkhauthanhcong"] = true;
                    return RedirectToAction("Homepage", "Home");
                }
                //Nếu mật khẩu mới không trùng khớp sẽ thông báo sai và quay lại trang nhập mật khẩu mới
                Session["sosanh-matkhaumoi-incorrect"] = true;
                return View(taikhoan);
            }
            //Nếu mật khẩu mới bỏ trống sẽ trở lại thông báo lỗi
            Session["matkhaumoi-null"] = true;
            return View(taikhoan);
        }
    }
}