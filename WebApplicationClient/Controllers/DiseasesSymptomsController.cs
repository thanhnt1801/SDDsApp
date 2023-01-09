using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NToastNotify;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplicationClient.Models;

namespace WebApplicationClient.Controllers
{
    public class DiseasesSymptomsController : Controller
    {
        private readonly HttpClient client = null;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IToastNotification _toastNotification;
        private string DiseasesHasSymptomApiUrl = "";
        private string DiseaseApiUrl = "";

        public DiseasesSymptomsController(IHttpContextAccessor httpContextAccessor, IToastNotification toastNotification)
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            DiseasesHasSymptomApiUrl = "https://localhost:44397/api/DiseasesHasSymptoms";
            DiseaseApiUrl = "https://localhost:44397/api/Diseases";
            _httpContextAccessor = httpContextAccessor;
            _toastNotification = toastNotification;
        }
        public ISession session { get { return _httpContextAccessor.HttpContext.Session; } }

        public IActionResult Index()
        {
            return View();
        }

        private async Task GetSymptomByDiseaseViewBag(long diseaseId)
        {
            HttpResponseMessage responseDisease = await client.GetAsync($"{DiseaseApiUrl}/GetRestSymptoms/{diseaseId}");

            string strData = await responseDisease.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Symptom> listDiseases = JsonSerializer.Deserialize<List<Symptom>>(strData, options);
            ViewBag.listDiseases = listDiseases;
            ViewBag.diseaseId = diseaseId;
        }

        public async Task<IActionResult> AddSymptomByDisease(long diseaseId)
        {
            await GetSymptomByDiseaseViewBag(diseaseId);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddSymptomByDisease(DiseasesHasSymptoms model)
        {
            string data = JsonSerializer.Serialize(model);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(DiseasesHasSymptomApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Add symptom success!");
                await GetSymptomByDiseaseViewBag(model.DiseaseId);
                return View();
            }

            _toastNotification.AddErrorToastMessage("Fail to add symptom for disease!");
            await GetSymptomByDiseaseViewBag(model.DiseaseId);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSymptomByDisease(DiseasesHasSymptoms model)
        {
            HttpResponseMessage response = await client.DeleteAsync(DiseasesHasSymptomApiUrl + "/" + model.DiseaseId + "/" + model.SymptomId);

            if (response.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Delete symptom success!");
                await GetSymptomByDiseaseViewBag(model.DiseaseId);
                return RedirectToAction("DiseaseSymptom", "Disease",new { id = model.DiseaseId });
            }

            _toastNotification.AddErrorToastMessage("Fail to add symptom for disease!");
            await GetSymptomByDiseaseViewBag(model.DiseaseId);
            return RedirectToAction("DiseaseSymptom", "Disease", new { id = model.DiseaseId });
        }
    }
}

