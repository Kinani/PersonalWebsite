using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PersonalWebsite.Controllers
{
    public class ProjectsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult DXEvents()
        {
            return View();
        }
        public IActionResult SQ88()
        {
            return View();
        }
        public IActionResult Time()
        {
            return View();
        }
        public IActionResult Confess()
        {
            return View();
        }
        public IActionResult FanarCore()
        {
            return View();
        }
    }
}