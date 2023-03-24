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
        private string PredictionApiUrl = "";

        public HomeController(ILogger<HomeController> logger, IToastNotification toastNotification, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _toastNotification = toastNotification;
            _client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            _client.DefaultRequestHeaders.Accept.Add(contentType);
            _httpContextAccessor = httpContextAccessor;
            DiseaseApiUrl = "https://localhost:44397/api/Diseases";
            PredictionApiUrl = "https://localhost:44351/api/Predictions";
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

        public async Task<IActionResult> Index()
        {
            await CorrectDiagnosisPercent();
            await GetTotalDiagnosisToday();
            await GetTotalDiagnosisLast7days();
            return View();
        }

        public async Task CorrectDiagnosisPercent()
        {
            HttpResponseMessage response = await _client.GetAsync(PredictionApiUrl + "/CorrectDiagnosisPercent");

            string strData = await response.Content.ReadAsStringAsync();

            var jsonObject = JsonSerializer.Deserialize<JsonElement>(strData);

            // Access the "results" array and iterate over its items
            var healthPercent = jsonObject.GetProperty("healthPercent").GetDouble();
            var leafSpotPercent = jsonObject.GetProperty("leafSpotPercent").GetDouble();
            var powderyPercent = jsonObject.GetProperty("powderyPercent").GetDouble();

            session.SetString("healthPercent", healthPercent.ToString());
            session.SetString("leafSpotPercent", leafSpotPercent.ToString());
            session.SetString("powderyPercent", powderyPercent.ToString());
        }

        public async Task GetTotalDiagnosisToday()
        {
            HttpResponseMessage response = await _client.GetAsync(PredictionApiUrl + "/TotalDiagnosisToday");

            string strData = await response.Content.ReadAsStringAsync();

            var jsonObject = JsonSerializer.Deserialize<JsonElement>(strData);

            // Access the "results" array and iterate over its items
            var healthyDiagnosisToday = jsonObject.GetProperty("healthyDiagnosisToday").GetInt32();
            var leafSpotDiagnosisToday = jsonObject.GetProperty("leafSpotDiagnosisToday").GetInt32();
            var powderyDiagnosisToday = jsonObject.GetProperty("powderyDiagnosisToday").GetInt32();

            session.SetInt32("healthyDiagnosisToday", healthyDiagnosisToday);
            session.SetInt32("leafSpotDiagnosisToday", leafSpotDiagnosisToday);
            session.SetInt32("powderyDiagnosisToday", powderyDiagnosisToday);
        }

        public async Task GetTotalDiagnosisLast7days()
        {
            HttpResponseMessage response = await _client.GetAsync(PredictionApiUrl + "/TotalDiagnosisLast7days");

            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            var jsonObject = JsonSerializer.Deserialize<JsonElement>(strData, options);

            // Access the "results" array and iterate over its items
            var jsonListDays = jsonObject.GetProperty("listDays");
            var jsonListPrediction = jsonObject.GetProperty("listPrediction");

            session.SetString("arrayListDays", jsonListDays.ToString());
            session.SetString("arrayListPrediction", jsonListPrediction.ToString());

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
            var limitListDisease = listDiseases.Where(d => d.Status == true).Take(8);
            return View(limitListDisease);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
