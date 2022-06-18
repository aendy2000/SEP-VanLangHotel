using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SEP_VanLangHotel.Middleware
{
    public class LoginVerification : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session["user-id"] == null)
            {
                filterContext.Result = new RedirectResult("~/Admin/Auth/Login");
                return;
            }
        }

    }
}