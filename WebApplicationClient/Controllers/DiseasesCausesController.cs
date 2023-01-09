using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using WebApplicationClient.Models;

namespace WebApplicationClient.Controllers
{
    public class DiseasesCausesController : Controller
    {
        private readonly HttpClient client = null;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IToastNotification _toastNotification;
        private string DiseasesHasCausesApiUrl = "";
        private string DiseaseApiUrl = "";

        public DiseasesCausesController(IHttpContextAccessor httpContextAccessor, IToastNotification toastNotification)
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            DiseasesHasCausesApiUrl = "https://localhost:44397/api/DiseasesHasCauses";
            DiseaseApiUrl = "https://localhost:44397/api/Diseases";
            _httpContextAccessor = httpContextAccessor;
            _toastNotification = toastNotification;
        }
        public ISession session { get { return _httpContextAccessor.HttpContext.Session; } }

        public IActionResult Index()
        {
            return View();
        }

        private async Task GetCausesByDiseaseViewBag(long diseaseId)
        {
            HttpResponseMessage responseDisease = await client.GetAsync($"{DiseaseApiUrl}/GetRestCauses/{diseaseId}");

            string strData = await responseDisease.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Cause> listCauses = JsonSerializer.Deserialize<List<Cause>>(strData, options);
            ViewBag.listCauses = listCauses;
            ViewBag.diseaseId = diseaseId;
        }

        public async Task<IActionResult> AddCausesByDisease(long diseaseId)
        {
            await GetCausesByDiseaseViewBag(diseaseId);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddCausesByDisease(DiseasesHasCauses model)
        {
            string data = JsonSerializer.Serialize(model);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(DiseasesHasCausesApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Add Cause success!");
                await GetCausesByDiseaseViewBag(model.DiseaseId);
                return View();
            }

            _toastNotification.AddErrorToastMessage("Fail to add symptom for disease!");
            await GetCausesByDiseaseViewBag(model.DiseaseId);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCausesByDisease(DiseasesHasCauses model)
        {
            HttpResponseMessage response = await client.DeleteAsync(DiseasesHasCausesApiUrl + "/" + model.DiseaseId + "/" + model.CauseId);

            if (response.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Delete Cause success!");
                await GetCausesByDiseaseViewBag(model.DiseaseId);
                return RedirectToAction("DiseaseCause", "Disease", new { id = model.DiseaseId });
            }

            _toastNotification.AddErrorToastMessage("Fail to add Cause for disease!");
            await GetCausesByDiseaseViewBag(model.DiseaseId);
            return RedirectToAction("DiseaseCause", "Disease", new { id = model.DiseaseId });
        }
    }
}
