using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WmsApplaction.Models;

namespace WmsApplaction.Controllers
{
    public class ShopCarController : Controller
    {
        InventoryManagementEntities db = new InventoryManagementEntities();

        public ActionResult ChaCar()
        {
            // 未登录时返回Json，前端需根据success=false跳转登录页
            if (Session["uid"] == null)
            {
                return Json(new { success = false, message = "请先登录" }, JsonRequestBehavior.AllowGet);
            }
            int id = (int)Session["uid"];

            var quary = from a in db.Order
                        join b in db.Product on a.ProductID equals b.ID
                        where a.UserID == id && a.status == "0"
                        select new
                        {
                            id = a.ProductID,
                            img = b.Image,
                            name = b.Name,
                            qucNum = b.Quantity, // 库存数量
                            gwsl = a.quantity,   // 购物数量
                            price = b.Price,     // 商品价格
                            sum = a.money        // 商品总价
                        };
            var result = quary.ToList();
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// 加入购物车
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult AddCar(int id)
        {
            if (Session["uid"] == null)
                return Json(new { success = false, message = "请先登录" });

            int userId = (int)Session["uid"];
            var product = db.Product.FirstOrDefault(p => p.ID == id);
            if (product == null)
                return Json(new { success = false, message = "商品不存在" });

            // 检查购物车是否已有该商品
            var cartItem = db.Order.FirstOrDefault(o => o.UserID == userId && o.ProductID == id && o.status == "0");
            if (cartItem != null)
            {
                cartItem.quantity += 1; // 数量+1
                cartItem.money = cartItem.quantity * product.Price;
            }
            else
            {
                db.Order.Add(new Order
                {
                    UserID = userId,
                    ProductID = id,
                    quantity = 1,
                    money = product.Price,
                    status = "0"
                });
            }
            db.SaveChanges();
            return Json(new { success = true, message = "添加成功" });
        }

        //增加减少数量
        [HttpPost]
        public ActionResult UpdateQuantity(int id, int change)
        {
            if (Session["uid"] == null)
                return Json(new { success = false, message = "请先登录" });

            int userId = (int)Session["uid"];
            var cartItem = db.Order.FirstOrDefault(o => o.UserID == userId && o.ProductID == id && o.status == "0");
            if (cartItem == null)
                return Json(new { success = false, message = "购物车中无此商品" });

            cartItem.quantity += change;
            if (cartItem.quantity < 1) cartItem.quantity = 1;

            // 更新总价
            var product = db.Product.FirstOrDefault(p => p.ID == id);
            if (product != null)
                cartItem.money = cartItem.quantity * product.Price;

            db.SaveChanges();
            return Json(new { success = true });
        }

        //删除某一条购物数据
        [HttpPost]
        public ActionResult RemoveItem(int id)
        {
            if (Session["uid"] == null)
                return Json(new { success = false, message = "请先登录" });

            int userId = (int)Session["uid"];
            var cartItem = db.Order.FirstOrDefault(o => o.UserID == userId && o.ProductID == id && o.status == "0");
            if (cartItem == null)
                return Json(new { success = false, message = "购物车中无此商品" });

            db.Order.Remove(cartItem);
            db.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult ClearCart()
        {
            if (Session["uid"] == null)
                return Json(new { success = false, message = "请先登录" });

            int userId = (int)Session["uid"];
            var cartItems = db.Order.Where(o => o.UserID == userId && o.status == "0").ToList();
            db.Order.RemoveRange(cartItems);
            db.SaveChanges();
            return Json(new { success = true });
        }

        //结算
        [HttpPost]
        public ActionResult Checkout()
        {
            if (Session["uid"] == null)
                return Json(new { success = false, message = "请先登录" });

            int userId = (int)Session["uid"];
            var cartItems = db.Order.Where(o => o.UserID == userId && o.status == "0").ToList();
            if (!cartItems.Any())
                return Json(new { success = false, message = "购物车为空" });

            // 获取当前OrdeID最大值（排除null）
            int maxOrderId = db.Order.Where(o => o.OrdeID != null).Select(o => o.OrdeID.Value).DefaultIfEmpty(0).Max();
            int newOrderId = maxOrderId + 1;

            foreach (var item in cartItems)
            {
                item.OrdeID = newOrderId;
                item.status = "1"; // 1=已结算
                item.Time = DateTime.Now;
            }
            db.SaveChanges();
            return Json(new { success = true, message = "结算成功" });
        }
    }
}