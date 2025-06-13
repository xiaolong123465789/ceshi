using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations;
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
            IEnumerable<Product> prolist = db.Product.Where(p => p.Quantity <= 40).ToList();
            ViewBag.TotalQuantity = db.Product.Sum(p => p.Quantity); // 计算总和
            ViewBag.allQuantity = db.Product.Sum(p => p.Quantity * p.Price); // 计算总和.
            ViewBag.diQuantity = db.Product.Count(p => p.Quantity <= 40); // 计算总和

            DateTime todayStart = DateTime.Today;
            DateTime todayEnd = todayStart.AddDays(1).AddTicks(-1);
            ViewBag.outQuantity = db.Order.Where(p => (p.status == "已发货" || p.status == "已完成") &&
                                p.Time >= todayStart &&
                                p.Time <= todayEnd).Sum(p => (int?)p.quantity) ?? 0;


            return PartialView("_Stock", prolist);
            return PartialView("_Stock");
        }

        //入库
        [HttpPost]
        public ActionResult Inbound(int ProductID, int Quantity, string Deliverer)
        {
            var product = db.Product.FirstOrDefault(x => x.ID == ProductID);
            if (product != null)
            {
                product.Quantity += Quantity;
                db.Set<Product>().AddOrUpdate(product);
                db.SaveChanges();

                // 添加入库记录
                var inventory = new Inventory
                {
                    ProductID = ProductID,
                    quantity = Quantity,
                    Time = DateTime.Now,
                    Deliverer = Deliverer,
                    Admin = User.Identity?.Name // 如果有登录系统，否则可写null或其它
                };
                db.Inventory.Add(inventory);

                db.SaveChanges();
            }




            // 重新获取最新数据
            IEnumerable<Product> prolist = db.Product.Where(p => p.Quantity <= 40).ToList();
            ViewBag.TotalQuantity = db.Product.Sum(p => p.Quantity);
            ViewBag.allQuantity = db.Product.Sum(p => p.Quantity * p.Price);
            ViewBag.diQuantity = db.Product.Count(p => p.Quantity <= 40);
            DateTime todayStart = DateTime.Today;
            DateTime todayEnd = todayStart.AddDays(1).AddTicks(-1);
            ViewBag.outQuantity = db.Order.Where(p => (p.status == "已发货" || p.status == "已完成") &&
                                    p.Time >= todayStart &&
                                    p.Time <= todayEnd).Sum(p => (int?)p.quantity) ?? 0;


            return PartialView("_Stock", prolist);
        }

        // 获取最近操作记录（入库和出库）
        public JsonResult RecentActivities()
        {
            var inbound = db.Inventory
         .OrderByDescending(i => i.Time)
         .Take(5)
         .ToList()
         .Select(i => new {
             Type = "入库",
             Name = i.Product != null ? i.Product.Name : "",
             // 转为本地时间再ToString("s")输出ISO格式
             Time = i.Time.HasValue ? i.Time.Value.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss") : null,
             Quantity = i.quantity
         });

            var outbound = db.Order
                .Where(o => o.status == "已发货" || o.status == "已完成")
                .OrderByDescending(o => o.Time <= DateTime.Now)
                .Take(5)
                .ToList()
                .Select(o => new {
                    Type = "出库",
                    Name = o.Product != null ? o.Product.Name : "",
                    Time = o.Time.HasValue ? o.Time.Value.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss") : null,
                    Quantity = o.quantity
                });

            var activities = inbound.Concat(outbound)
                .OrderByDescending(a => a.Time)
                .Take(5)
                .ToList();

            return Json(activities, JsonRequestBehavior.AllowGet);
        }

        // 搜索低库存商品
        [HttpGet]
        public PartialViewResult SearchLowStock(string keyword)
        {
            var query = db.Product.Where(p => p.Quantity <= 40);
            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(p => p.Name.Contains(keyword));
            }
            var prolist = query.ToList();
            return PartialView("_LowStockTableBody", prolist);
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