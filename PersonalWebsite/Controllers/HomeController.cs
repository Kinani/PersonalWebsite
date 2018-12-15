using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PersonalWebsite.Models;
using PersonalWebsite.Utils;

namespace PersonalWebsite.Controllers
{
    public class HomeController : Controller
    {
        private IConfiguration _configuration;
        private ILogger<HomeController> _logger;
        private readonly IEmailService _emailService;
        private readonly IHostingEnvironment _hostingEnvironment;

        public HomeController(IConfiguration Configuration, ILogger<HomeController> Logger, 
            IEmailService EmailService,
            IHostingEnvironment HostingEnvironment)
        {
            _configuration = Configuration;
            _logger = Logger;
            _emailService = EmailService;
            _hostingEnvironment = HostingEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult About()
        {
            return View();
        }
        public IActionResult Portfolio()
        {
            return View();
        }

        public FileResult DownloadCV()
        {
            var fileName = $"Abdelrahman_F_ElKinani.pdf";
            string wwwrootPath = _hostingEnvironment.WebRootPath;
            var path = Path.Combine(
                           wwwrootPath,"docs", fileName);
            byte[] fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, "application/x-msdownload", fileName);
        }

        [HttpGet]
        public IActionResult Contact()
        {
            ViewData["ReCaptchaKey"] = _configuration.GetSection("GoogleReCaptcha:key").Value;
            return View();
        }

        [HttpPost]
        public IActionResult Contact(ContactModel contactdata)
        {
            ViewData["ReCaptchaKey"] = _configuration.GetSection("GoogleReCaptcha:key").Value;
            if (ModelState.IsValid)
            {
                if (!ReCaptchaPassed(
                    Request.Form["g-recaptcha-response"], 
                    _configuration.GetSection("GoogleReCaptcha:secret").Value,
                    _logger
                    ))
                {
                    ModelState.AddModelError(string.Empty, "You failed the CAPTCHA, YOU ROBOT.");
                    ViewData["CaptchaError"] = "You failed the CAPTCHA, YOU ROBOT.";
                    return View();
                }

                EmailMessage newEmail = new EmailMessage();
                EmailAddress myemail = new EmailAddress();
                EmailAddress webappemail = new EmailAddress();

                myemail.Address = @"kinani95@outlook.com";
                myemail.Name = "Abdelrahman El Kinani";

                webappemail.Address = @"ab.kinani95@gmail.com";
                webappemail.Name = "My website";

                newEmail.ToAddresses.Add(myemail);
                newEmail.FromAddresses.Add(webappemail);

                newEmail.Subject = contactdata.name;
                newEmail.Content = string.Format("Email: {0} \nSubject:\n{1}", contactdata.email, contactdata.subject);

                _emailService.Send(newEmail);

                return RedirectToAction("Index");
            }

            return View("Index");
        }

        public static bool ReCaptchaPassed(string gRecaptchaResponse, string secret, ILogger logger)
        {
            HttpClient httpClient = new HttpClient();
            var res = httpClient.GetAsync($"https://www.google.com/recaptcha/api/siteverify?secret={secret}&response={gRecaptchaResponse}").Result;
            if (res.StatusCode != HttpStatusCode.OK)
            {
                logger.LogError("Error while sending request to ReCaptcha");
                return false;
            }

            string JSONres = res.Content.ReadAsStringAsync().Result;
            dynamic JSONdata = JObject.Parse(JSONres);
            if (JSONdata.success != "true")
            {
                return false;
            }

            return true;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
