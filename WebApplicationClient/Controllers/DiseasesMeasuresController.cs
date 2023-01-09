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
    public class DiseasesMeasuresController : Controller
    {
        private readonly HttpClient client = null;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IToastNotification _toastNotification;
        private string DiseasesNeedsMeasuresApiUrl = "";
        private string DiseaseApiUrl = "";

        public DiseasesMeasuresController(IHttpContextAccessor httpContextAccessor, IToastNotification toastNotification)
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            DiseasesNeedsMeasuresApiUrl = "https://localhost:44397/api/DiseasesNeedsMeasures";
            DiseaseApiUrl = "https://localhost:44397/api/Diseases";
            _httpContextAccessor = httpContextAccessor;
            _toastNotification = toastNotification;
        }
        public ISession session { get { return _httpContextAccessor.HttpContext.Session; } }

        public IActionResult Index()
        {
            return View();
        }

        private async Task GetMeasuresByDiseaseViewBag(long diseaseId)
        {
            HttpResponseMessage responseDisease = await client.GetAsync($"{DiseaseApiUrl}/GetRestMeasures/{diseaseId}");

            string strData = await responseDisease.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<PreventativeMeasure> listMeasures = JsonSerializer.Deserialize<List<PreventativeMeasure>>(strData, options);
            ViewBag.listMeasures = listMeasures;
            ViewBag.diseaseId = diseaseId;
        }

        public async Task<IActionResult> AddMeasuresByDisease(long diseaseId)
        {
            await GetMeasuresByDiseaseViewBag(diseaseId);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddMeasuresByDisease(DiseasesNeedsMeasures model)
        {
            string data = JsonSerializer.Serialize(model);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(DiseasesNeedsMeasuresApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Add Measure success!");
                await GetMeasuresByDiseaseViewBag(model.DiseaseId);
                return View();
            }

            _toastNotification.AddErrorToastMessage("Fail to add symptom for disease!");
            await GetMeasuresByDiseaseViewBag(model.DiseaseId);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteMeasuresByDisease(DiseasesNeedsMeasures model)
        {
            HttpResponseMessage response = await client.DeleteAsync(DiseasesNeedsMeasuresApiUrl + "/" + model.DiseaseId + "/" + model.PreventativeMeasureId);

            if (response.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Delete Measure success!");
                await GetMeasuresByDiseaseViewBag(model.DiseaseId);
                return RedirectToAction("DiseaseMeasure", "Disease", new { id = model.DiseaseId });
            }

            _toastNotification.AddErrorToastMessage("Fail to add Mesures for disease!");
            await GetMeasuresByDiseaseViewBag(model.DiseaseId);
            return RedirectToAction("DiseaseMeasure", "Disease", new { id = model.DiseaseId });
        }
    }
}

