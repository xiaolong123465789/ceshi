using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WmsApplaction.Models;

namespace WmsApplaction.Controllers
{
    public class GetProductController : Controller
    {

        InventoryManagementEntities db = new InventoryManagementEntities();

        // GET: GetProduct

        //获取商品
        public ActionResult GetProducts()
        {
            var products = db.Product.Where(p => p.Status == "0").Select(p => new
            {
                id = p.ID,
                name = p.Name,
                price = p.Price,
                num = p.Quantity,
                imgee = p.Image,
                status = p.Status
            }).ToList();

            return Json(products, JsonRequestBehavior.AllowGet);
        }

        //加入购物车
        //public ActionResult addCar() 
        //{ 
            
        //}
    }
}