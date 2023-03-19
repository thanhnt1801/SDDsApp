using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplicationClient.Models;
using WebApplicationClient.Models.Disease;

namespace WebApplicationClient.Controllers
{
    public class HomeController : Controller
    {   

        private readonly ILogger<HomeController> _logger;
        private readonly IToastNotification _toastNotification;
        private HttpClient _client;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private string DiseaseApiUrl = "";

        public HomeController(ILogger<HomeController> logger, IToastNotification toastNotification, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _toastNotification = toastNotification;
            _client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            _client.DefaultRequestHeaders.Accept.Add(contentType);
            _httpContextAccessor = httpContextAccessor;
            DiseaseApiUrl = "https://localhost:44397/api/Diseases";
        }

        public ISession session { get { return _httpContextAccessor.HttpContext.Session; } }

        private bool TokenAdded()
        {
            string token = session.GetString("jwtToken");

            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            return true;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult UploadImage()
        {
            if (!TokenAdded())
            {
                return View("userhome");
            }
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ConfirmExpert()
        {
            _toastNotification.AddSuccessToastMessage("Send Disease to expert success!");
            return RedirectToAction("userhome", "Home");
        }

        public async Task<IActionResult> UserHome()
        {
            HttpResponseMessage response;

            response = await _client.GetAsync(DiseaseApiUrl);

            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Disease> listDiseases = JsonSerializer.Deserialize<List<Disease>>(strData, options);
            var limitListDisease = listDiseases.Take(9);
            return View(limitListDisease);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
