using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WmsApplaction.Models;

namespace WmsApplaction.Controllers
{
    public class FrontController : Controller
    {

        InventoryManagementEntities db = new InventoryManagementEntities();
        // GET: Front
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult geren() 
        {
            if (Session["uid"] == null){
                return RedirectToAction("QtLogin");
            }

            int id = (int)Session["uid"];
            
            var grxx = db.User.Where(p => p.ID == id).ToList();
            
            ViewBag.jieguo = grxx;
            return View(grxx);
        }

        public ActionResult QtLogin() 
        {
            return View();
        }


        // 处理登录请求 (Ajax)
        [HttpPost]
        public JsonResult Login(string username, string password)
        {
            using (InventoryManagementEntities db = new InventoryManagementEntities())
            {
                // 验证用户
                var user = db.User
                    .FirstOrDefault(u => u.Account == username && u.Password == password);

                if (user != null)
                {
                    // 登录成功
                    // 设置Session
                    Session["CurrentUser"] = user;
                    Session["uid"] = user.ID;
                    // 记住我功能（可选）
                    //if (rememberMe)
                    //{
                    //    HttpCookie cookie = new HttpCookie("UserAuth");
                    //    cookie.Value = user.UserID.ToString();
                    //    cookie.Expires = DateTime.Now.AddDays(7);
                    //    Response.Cookies.Add(cookie);
                    //}

                    return Json(new { success = true, redirectUrl = Url.Action("Index", "Front") });
                }

                // 登录失败
                return Json(new { success = false, message = "用户名或密码错误" });
            }
        }
    }
}