using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web;
using SEP_VanLangHotel.Controllers;
using System.Web.Mvc;
using Moq;
using SEP_VanLangHotel.Models;

namespace SEP_VanLangHotel_Test
{
    [TestClass]
    public class UnitTestAccountManagement
    {
        AccountManagementController accountManagement = new AccountManagementController();
        [TestMethod]
        public void Home()
        {
            var mockControllerContext = new Mock<ControllerContext>();
            var mockSession = new Mock<HttpSessionStateBase>();
            mockSession.SetupGet(s => s["user-role"]).Returns("Quản lý");
            mockControllerContext.Setup(p => p.HttpContext.Session).Returns(mockSession.Object);

            accountManagement.ControllerContext = mockControllerContext.Object;
            ViewResult result = accountManagement.Home() as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("Home", result.ViewName);
        }

        [TestMethod]
        public void AddNewAccount()
        {
            var mockControllerContext = new Mock<ControllerContext>();
            var mockSession = new Mock<HttpSessionStateBase>();
            mockSession.SetupGet(s => s["user-role"]).Returns("Quản lý");
            mockControllerContext.Setup(p => p.HttpContext.Session).Returns(mockSession.Object);

            accountManagement.ControllerContext = mockControllerContext.Object;
            ViewResult result = accountManagement.AddNewAccount() as ViewResult; //as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("AddNewAccount", result.ViewName);

        }

        [TestMethod]
        public void UpdateAccountData()
        {
            var mockControllerContext = new Mock<ControllerContext>();
            var mockSession = new Mock<HttpSessionStateBase>();
            mockSession.SetupGet(s => s["user-role"]).Returns("Quản lý");
            mockControllerContext.Setup(p => p.HttpContext.Session).Returns(mockSession.Object);

            accountManagement.ControllerContext = mockControllerContext.Object;
            var result = accountManagement.UpdateAccount("TK202207010001") as ViewResult;
            var account = (Tai_Khoan)result.ViewData.Model;

            Assert.IsNotNull(account);
        }

        [TestMethod]
        public void UpdateAccountView()
        {
            var mockControllerContext = new Mock<ControllerContext>();
            var mockSession = new Mock<HttpSessionStateBase>();
            mockSession.SetupGet(s => s["user-role"]).Returns("Quản lý");
            mockControllerContext.Setup(p => p.HttpContext.Session).Returns(mockSession.Object);

            accountManagement.ControllerContext = mockControllerContext.Object;
            var result = accountManagement.UpdateAccount("TK202207010001") as ViewResult;

            Assert.AreEqual("UpdateAccount", result.ViewName);
        }

        [TestMethod]
        public void DetailtAccount()
        {
            var mockControllerContext = new Mock<ControllerContext>();
            var mockSession = new Mock<HttpSessionStateBase>();
            mockSession.SetupGet(s => s["user-role"]).Returns("Quản lý");
            mockControllerContext.Setup(p => p.HttpContext.Session).Returns(mockSession.Object);

            accountManagement.ControllerContext = mockControllerContext.Object;
            var result = accountManagement.DetailtAccount("TK202207010001") as ViewResult;
            var account = (Tai_Khoan)result.ViewData.Model;

            Assert.IsTrue(account != null ? true : false);
            Assert.AreEqual("DetailtAccount", result.ViewName);
        }

        [TestMethod]
        public void Information()
        {
            var mockControllerContext = new Mock<ControllerContext>();
            var mockSession = new Mock<HttpSessionStateBase>();
            mockSession.SetupGet(s => s["user-id"]).Returns("admin");
            mockControllerContext.Setup(p => p.HttpContext.Session).Returns(mockSession.Object);

            accountManagement.ControllerContext = mockControllerContext.Object;
            var result = accountManagement.Information() as ViewResult;
            var account = (Tai_Khoan)result.ViewData.Model;

            Assert.IsTrue(account != null ? true : false);
            Assert.AreEqual("Information", result.ViewName);
        }

        [TestMethod]
        public void ChangePassword()
        {
            var result = accountManagement.ChangePassword("admin") as ViewResult;
            var account = (Tai_Khoan)result.ViewData.Model;

            Assert.IsTrue(account != null ? true : false);
            Assert.AreEqual("ChangePassword", result.ViewName);
        }

        //Test failt invalid password
        [TestMethod]
        public void ChangePasswordInvalidPassword()
        {
            var mockControllerContext = new Mock<ControllerContext>();
            var mockSession = new Mock<HttpSessionStateBase>();
            mockSession.SetupGet(s => s["passcu-false"]).Returns(null);
            mockControllerContext.Setup(p => p.HttpContext.Session).Returns(mockSession.Object);
            accountManagement.ControllerContext = mockControllerContext.Object;

            //password invalid
            var result = accountManagement.ChangePassword("TK202207010001", "", "admin123", "admin123") as ViewResult;
            var account = (Tai_Khoan)result.ViewData.Model;

            Assert.IsTrue(account != null ? true : false);
            Assert.AreEqual("ChangePassword", result.ViewName);
        }

        //Test failt new password not equals
        [TestMethod]
        public void ChangePasswordNewPasswordNotEquals()
        {
            var mockControllerContext = new Mock<ControllerContext>();
            var mockSession = new Mock<HttpSessionStateBase>();
            mockSession.SetupGet(s => s["passcu-false"]).Returns(null);
            mockSession.SetupGet(s => s["newpass-trung"]).Returns(null);

            mockControllerContext.Setup(p => p.HttpContext.Session).Returns(mockSession.Object);
            accountManagement.ControllerContext = mockControllerContext.Object;

            //new password not equals reNewPassword
            var result = accountManagement.ChangePassword("TK202207010001", "admin", "admin123", "admin321") as ViewResult;
            var account = (Tai_Khoan)result.ViewData.Model;
            Assert.IsTrue(account != null ? true : false);
            Assert.AreEqual("ChangePassword", result.ViewName);
        }
    }
}
