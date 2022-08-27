using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web;
using SEP_VanLangHotel.Controllers;
using System.Web.Mvc;
using Moq;
using SEP_VanLangHotel.Models;
using System.Collections.Generic;
using System;

namespace SEP_VanLangHotel_Test.Controller
{
    [TestClass]
    public class UnitTestOrderRoom
    {
        OrderRoomController orderRoom = new OrderRoomController();
        //Test return view
        [TestMethod]
        public void SaveDataImport()
        {
            var mockControllerContext = new Mock<ControllerContext>();
            var mockSession = new Mock<HttpSessionStateBase>();
            mockSession.SetupGet(s => s["user-role"]).Returns("Nhân viên");
            mockControllerContext.Setup(p => p.HttpContext.Session).Returns(mockSession.Object);
            orderRoom.ControllerContext = mockControllerContext.Object;

            List<List<List<string>>> listTemp = new List<List<List<string>>>();
            listTemp.Add(new List<List<string>>());
            listTemp[0].Add(new List<string>());
            listTemp[0][0].Add("TestData");
            ViewResult result = orderRoom.SaveDataImport(listTemp) as ViewResult;

            Assert.IsNotNull(result);
            Assert.AreEqual("SaveDataImport", result.ViewName);
        }

        [TestMethod]
        public void orderRooms_selectPastDay()
        {
            var mockControllerContext = new Mock<ControllerContext>();
            var mockSession = new Mock<HttpSessionStateBase>();
            mockSession.SetupGet(s => s["user-role"]).Returns("Nhân viên");
            mockControllerContext.Setup(p => p.HttpContext.Session).Returns(mockSession.Object);
            orderRoom.ControllerContext = mockControllerContext.Object;

            List<Tien_Ich> dstienIch = new List<Tien_Ich>();
            var matienIch = "TI202207060001";
            var tentienIch = "Máy tính, máy chiếu, in ấn";
            dstienIch.Add(new Tien_Ich() { Ma_Tien_Ich = matienIch, Ten_Tien_Ich = tentienIch, IsChecks = false });

            ListTienIch tienichList = new ListTienIch();
            tienichList.tienIch = dstienIch;

            var result = orderRoom.orderRooms(tienichList, DateTime.Now, 1, 1, 1) as ViewResult;

            Assert.IsNotNull(result.Model);
            Assert.AreEqual("SaveDataOrderRoom", result.ViewName);
        }
    }
}
