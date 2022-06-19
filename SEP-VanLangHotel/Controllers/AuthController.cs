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
        VanLangHotelEntities model = new VanLangHotelEntities();
        public ActionResult Login()
        {
            Session["username-incorrect"] = null;
            Session["password-incorrect"] = null;
            return View();
        }
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            var user = model.Tai_Khoan.FirstOrDefault(u => u.Ten_Dang_Nhap.ToUpper().Equals(username.ToUpper()));
            if (user != null)
            {
                Session["username-incorrect"] = null;
                if (user.Mat_Khau.Equals(password))
                {
                    Session["user-fullname"] = user.Ho_Va_Ten;
                    Session["user-id"] = user.Ten_Dang_Nhap;
                    Session["user-role"] = user.Quyen.Ten_Quyen;
                    return RedirectToAction("Homepage", "Home");
                }
                else
                {
                    Session["password-incorrect"] = true;
                    return View();
                }
            }

            Session["username-incorrect"] = true;
            Session["password-incorrect"] = true;
            return View();
        }
        public ActionResult Logout()
        {
            Session["user-fullname"] = null;
            Session["user-id"] = null;
            Session["user-role"] = null;
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


            if (!tendangnhap.Equals(""))
            {
                var taikhoan = model.Tai_Khoan.FirstOrDefault(c => c.Ten_Dang_Nhap.ToUpper().Equals(tendangnhap.Trim().ToUpper()));
                if (taikhoan != null)
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
                    taikhoan.Verify_Password = code;
                    string outoffdate = (ulong.Parse(DateTime.Now.ToString("yyyyMMddHHmmss")) + 1000).ToString();
                    taikhoan.OutOfDate_Code = outoffdate;
                    model.SaveChanges();

                    //Khởi tạo gửi mail
                    MailMessage mailmea = new MailMessage();
                    mailmea.To.Add(taikhoan.Email);
                    mailmea.From = new MailAddress(@"vanlanghotel@gmail.com");
                    mailmea.Subject = "Đặt lại mật khẩu - Văn Lang Hotel";
                    mailmea.IsBodyHtml = true;
                    mailmea.Body = "<font size=5>Mã xác nhận của bạn là: </font><br>" + "<font size=20><b>   " + code + "</b></font>";

                    SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                    smtp.UseDefaultCredentials = true;
                    smtp.EnableSsl = true;
                    smtp.Port = 25;
                    smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtp.Credentials = new NetworkCredential(@"vanlanghotel@gmail.com", "gqhyxnzkauoxoetx");
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
            var taikhoan = model.Tai_Khoan.FirstOrDefault(t => t.Ma_Tai_Khoan.Equals(id));
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

            if (taikhoan.OutOfDate_Code != null && !taikhoan.OutOfDate_Code.Equals(""))
            {
                ulong tgianhethan = ulong.Parse(taikhoan.OutOfDate_Code);
                if (tgianhethan >= ulong.Parse(DateTime.Now.ToString("yyyyMMddHHmmss")))
                {
                    if (!maxacnhan.Equals(""))
                    {
                        if (taikhoan.Verify_Password.Equals(maxacnhan.Trim()))
                        {
                            taikhoan.Verify_Password = null;
                            model.SaveChanges();
                            return RedirectToAction("NewPassword", new { id = taikhoan.Ma_Tai_Khoan });
                        }
                        else
                        {
                            Session["thongbao-maxacnhan-incorrect"] = true;
                            return View(taikhoan);
                        }
                    }
                    Session["thongbao-maxacnhan-null"] = true;
                    return View(taikhoan);
                }
                Session["thongbao-outoffdate"] = true; //Ma da het han
                taikhoan.Verify_Password = null;
                taikhoan.OutOfDate_Code = null;
                model.SaveChanges();
            }
            return RedirectToAction("ForgotPassword");
        }



        public ActionResult NewPassword(string id)
        {
            var taikhoan = model.Tai_Khoan.FirstOrDefault(t => t.Ma_Tai_Khoan.Equals(id));
            if (taikhoan != null)
                return View(taikhoan);
            else
                return RedirectToAction("ForgotPassword");
        }
        [HttpPost]
        public ActionResult NewPassword(string idtaikhoan, string matkhaumoi = null, string nhaplaimatkhaumoi = null)
        {
            Session["matkhaumoi-null"] = null;
            Session["sosanh-matkhaumoi-incorrect"] = null;
            var taikhoan = model.Tai_Khoan.FirstOrDefault(t => t.Ma_Tai_Khoan.Equals(idtaikhoan));
            if (!matkhaumoi.Equals(""))
            {
                if (matkhaumoi.Equals(nhaplaimatkhaumoi))
                {
                    taikhoan.Mat_Khau = matkhaumoi;
                    model.SaveChanges();
                    return RedirectToAction("Homepage", "Home");
                }
                Session["sosanh-matkhaumoi-incorrect"] = true;
                return View(taikhoan);
            }
            Session["matkhaumoi-null"] = true;
            return View(taikhoan);
        }
    }
}