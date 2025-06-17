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
            if (Session["uid"] == null)
            {
                return RedirectToAction("QtLogin");
            }

            int id = (int)Session["uid"];

            var grxx = db.User.Where(p => p.ID == id).ToList();
            var orderHistory = db.Order
                .Where(o => o.UserID == id)
                .Select(o => new
                {
                    id = o.ID,
                    quan = o.quantity,
                    statu = o.status,
                    time = o.Time,
                    mone = o.money
                }).ToList();
            ViewBag.jieguo = grxx;
            ViewBag.orderHis = orderHistory;
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
                    if (user.Identitys == "管理员")
                    {
                        return Json(new { success = true, redirectUrl = Url.Action("Index", "Home") });
                    }
                    else
                    {
                        return Json(new { success = true, redirectUrl = Url.Action("Index", "Front") });
                    }
                }

                // 登录失败
                return Json(new { success = false, message = "用户名或密码错误" });
            }
        }

        //获得订单记录
        public ActionResult Order()
        {
            if (Session["uid"] == null)
            {
                return RedirectToAction("QtLogin");
            }

            int id = (int)Session["uid"];
            var dingdan = db.Order.Where(p => p.UserID == id && p.status == "1").Select(p => new {
                id = p.ID,
                userid = p.UserID,
                num = p.quantity,
                statu = p.status,
                time = p.Time,
                moneyy = p.money,
            }).ToList();
            return Json(dingdan, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult Person(string email, string iphone, string address, HttpPostedFileBase avatar)
        {
            if (Session["uid"] == null)
                return Json(new { success = false, message = "未登录" });

            int id = (int)Session["uid"];
            var user = db.User.FirstOrDefault(u => u.ID == id);
            if (user == null)
                return Json(new { success = false, message = "用户不存在" });

            user.Emil = email;
            user.Phone = iphone;
            user.Addess = address;

            // 处理头像上传
            if (avatar != null && avatar.ContentLength > 0)
            {
                string ext = System.IO.Path.GetExtension(avatar.FileName);
                string fileName = "avatar_" + id + ext;
                string path = Server.MapPath("~/img/grtx/" + fileName);
                avatar.SaveAs(path);
                user.Photo = fileName; // 只保存文件名
            }

            db.SaveChanges();
            return Json(new { success = true, message = "保存成功" });
        }
    }
}