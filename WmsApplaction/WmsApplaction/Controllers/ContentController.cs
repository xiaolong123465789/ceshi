using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WmsApplaction.Models;

namespace WmsApplaction.Controllers
{
    public class ContentController : Controller
    {
        Models.InventoryManagementEntities db = new Models.InventoryManagementEntities();
        // GET: Content
        //控制面板
        public ActionResult Stock()
        {
            return PartialView("_Stock");
        }

        //库存管理
        public ActionResult Inventory() { 
            return PartialView("_Inventory");
        }

        //出库管理
        public ActionResult Outbound()
        {
            return PartialView("_Outbound");
        }

        //商户管理
        public ActionResult Merchant() {
            var quanshow = db.User.ToList();
            return PartialView("_Merchant", quanshow);
        }
    
        public ActionResult Merchantss(string ShopName)
        {
            var quanshow = string.IsNullOrEmpty(ShopName)
                ? db.User.ToList()
                : db.User.Where(x => x.ShopName.Contains(ShopName)).ToList();
            return PartialView("_Merchant", quanshow);
        }

        [HttpPost]
        public ActionResult ChangeStatus(int id, int status)
        {
            var user = db.User.Find(id);
            if (user != null)
            {
                user.Status = status.ToString();
                db.SaveChanges();
            }
            var quanshow = db.User.ToList();
            return PartialView("_Merchant", quanshow);
        }

        [HttpPost]
        public ActionResult add(User user, HttpPostedFileBase photo)
        {
            
            string photoFileName = photo.FileName;
            if (photo != null && photo.ContentLength > 0)
            {
                // 生成唯一文件名，防止重名
                photo.SaveAs(Server.MapPath("/img/grtx/" + photo.FileName));
            }

            var user1 = new User
            {
                ShopName = user.ShopName,
                Name = user.Name,
                Account = user.Account,
                Password = user.Password,
                Addess = user.Addess,
                Emil = user.Emil,
                Photo = photo.FileName,
                Status = user.Status,
                Identitys= "商户"
            };
            db.User.Add(user1);
            if (db.SaveChanges() > 0)
            {
                var quanshow = db.User.ToList();
                return PartialView("_Merchant", quanshow);
            }
            else
            {
                return RedirectToAction("Stock");
            }

        }
    }
}