using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WmsApplaction.Controllers
{
    public class QtContentController : Controller
    {
        // GET: QtContent
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Shoping() { 
            return View();
        }

        public ActionResult qtgrxx()
        {
            return View();
        }

        public ActionResult zhxx()
        {
            return View();
        }

        public ActionResult gwc()
        {
            return View();
        }
    }
}