using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WmsApplaction.Models;
namespace WmsApplaction.Controllers
{
    public class kucunguanliController : Controller
    {
   
        // GET: kucunguanli
        public ActionResult zhanshi()
        {
            return View();
        }

        [HttpGet]
        public JsonResult GetInventoryList(int page = 1, int pageSize = 10, string keyword = "")
        {
            try
            {
                using (var db = new InventoryManagementEntities())
                {
                    var query = db.Product.AsQueryable();
                    if (!string.IsNullOrEmpty(keyword))
                    {
                        query = query.Where(p => p.Name.Contains(keyword) || p.ID.ToString().Contains(keyword));
                    }
                    var total = query.Count();
                    var list = query
                        .OrderBy(p => p.ID)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .Select(p => new
                        {
                            Barcode = p.ID,
                            Name = p.Name,
                            Price = p.Price ?? 0,
                            Quantity = p.Quantity ?? 0,
                            Immage=p.Image,
                            Status = p.Status == "0" ? "库存正常" : "禁用"
                        }).ToList();

                    return Json(new { total, list }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message, stack = ex.StackTrace }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        public JsonResult DeleteProduct(int id)
        {
            try
            {
                using (var db = new InventoryManagementEntities())
                {
                    var product = db.Product.FirstOrDefault(p => p.ID == id);
                    if (product == null)
                    {
                        return Json(new { success = false, message = "未找到该商品" });
                    }
                    product.Status = "1"; // 1代表禁用
                    db.SaveChanges();
                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.InnerException?.Message ?? ex.Message });
            }
        }

        [HttpPost]
        public JsonResult UpdateProduct(int id, string name, int price, int quantity, string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    return Json(new { success = false, message = "商品名称不能为空" });
                if (price < 0)
                    return Json(new { success = false, message = "价格不能为负数" });
                if (quantity < 0)
                    return Json(new { success = false, message = "数量不能为负数" });

                price = (int)Math.Round((double)price, MidpointRounding.AwayFromZero);
                quantity = (int)Math.Round((double)quantity, MidpointRounding.AwayFromZero);

                if (string.IsNullOrWhiteSpace(status))
                    return Json(new { success = false, message = "请选择状态" });

                using (var db = new InventoryManagementEntities())
                {
                    var product = db.Product.FirstOrDefault(p => p.ID == id);
                    if (product == null)
                        return Json(new { success = false, message = "未找到该商品" });
                    product.Name = name;
                    product.Price = price;
                    product.Quantity = quantity;
                    product.Status = status == "禁用" ? "1" : "0";
                    db.SaveChanges();
                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }



        [HttpPost]
        public JsonResult AddProduct()
        {
            try
            {
                var name = Request.Form["Name"];
                var priceStr = Request.Form["Price"];
                var quantityStr = Request.Form["Quantity"];

                // 后端非空和格式校验
                if (string.IsNullOrWhiteSpace(name))
                    return Json(new { success = false, message = "商品名称不能为空" });

                if (string.IsNullOrWhiteSpace(priceStr) || !decimal.TryParse(priceStr, out decimal price))
                    return Json(new { success = false, message = "价格不能为空且必须为数字" });

                if (string.IsNullOrWhiteSpace(quantityStr) || !int.TryParse(quantityStr, out int quantity))
                    return Json(new { success = false, message = "数量不能为空且必须为整数" });

                if (price < 0)
                    return Json(new { success = false, message = "价格不能为负数" });

                if (quantity < 0)
                    return Json(new { success = false, message = "数量不能为负数" });

                // 价格四舍五入为整数
                int priceInt = (int)Math.Round(price, MidpointRounding.AwayFromZero);

                string dbImageName = null;
                var file = Request.Files["ImageFile"];
                if (file != null && file.ContentLength > 0)
                {
                    var ext = System.IO.Path.GetExtension(file.FileName).ToLower();
                    if (ext != ".jpg" && ext != ".jpeg" && ext != ".png")
                    {
                        return Json(new { success = false, message = "只允许上传jpg或png格式的图片" });
                    }
                    if (file.ContentType != "image/jpeg" && file.ContentType != "image/png")
                    {
                        return Json(new { success = false, message = "图片格式不正确" });
                    }

                    var guidFileName = Guid.NewGuid().ToString("N") + ext;
                    var savePath = Server.MapPath("~/img/");
                    if (!System.IO.Directory.Exists(savePath))
                        System.IO.Directory.CreateDirectory(savePath);
                    file.SaveAs(System.IO.Path.Combine(savePath, guidFileName));
                    dbImageName = file.FileName;
                }

                using (var db = new InventoryManagementEntities())
                {
                    var product = new Product
                    {
                        Name = name,
                        Price = priceInt,
                        Quantity = quantity,
                        Image = dbImageName,
                        Status = "0"
                    };
                    db.Product.Add(product);
                    db.SaveChanges();
                }
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }






        //[HttpGet]
        //public JsonResult GetInventoryList()
        //{
        //    // 每次请求都新建一个 db 实例
        //    using (var db = new InventoryManagementEntities())
        //    {
        //        var list = db.Product.Select(p => new
        //        {
        //            Barcode = p.ID,
        //            Name = p.Name,
        //            Category = "",
        //            Quantity = p.Quantity ?? 0,
        //            Unit = "",
        //            Status = p.Status == "0" ? "库存正常" : "禁用"
        //        }).ToList();

        //        return Json(list, JsonRequestBehavior.AllowGet);
        //    }
        //}
    }
}