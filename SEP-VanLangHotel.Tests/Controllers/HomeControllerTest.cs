using Microsoft.VisualStudio.TestTools.UnitTesting;
using SEP_VanLangHotel;
using SEP_VanLangHotel.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace SEP_VanLangHotel.Tests.Controllers
{
    [TestClass]
    public class HomeControllerTest
    {
        [TestMethod]
        public void Home()
        {
            // Arrange
            HomeController controller = new HomeController();

            // Act
            ViewResult result = controller.Homepage() as ViewResult;

            // Assert
            Assert.IsNotNull(result);
        }

    }
}
