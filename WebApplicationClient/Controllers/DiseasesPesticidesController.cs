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
    public class DiseasesPesticidesController : Controller
    {
        private readonly HttpClient client = null;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IToastNotification _toastNotification;
        private string DiseasesNeedsPesticidesApiUrl = "";
        private string DiseaseApiUrl = "";

        public DiseasesPesticidesController(IHttpContextAccessor httpContextAccessor, IToastNotification toastNotification)
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            DiseasesNeedsPesticidesApiUrl = "https://localhost:44397/api/DiseasesNeedsPesticides";
            DiseaseApiUrl = "https://localhost:44397/api/Diseases";
            _httpContextAccessor = httpContextAccessor;
            _toastNotification = toastNotification;
        }
        public ISession session { get { return _httpContextAccessor.HttpContext.Session; } }

        public IActionResult Index()
        {
            return View();
        }

        private async Task GetPesticidesByDiseaseViewBag(long diseaseId)
        {
            HttpResponseMessage responseDisease = await client.GetAsync($"{DiseaseApiUrl}/GetRestPesticides/{diseaseId}");

            string strData = await responseDisease.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Pesticide> listPesticides = JsonSerializer.Deserialize<List<Pesticide>>(strData, options);
            ViewBag.listPesticides = listPesticides;
            ViewBag.diseaseId = diseaseId;
        }

        public async Task<IActionResult> AddPesticidesByDisease(long diseaseId)
        {
            await GetPesticidesByDiseaseViewBag(diseaseId);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AddPesticidesByDisease(DiseasesNeedsPesticides model)
        {
            string data = JsonSerializer.Serialize(model);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(DiseasesNeedsPesticidesApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Add Pesticide success!");
                await GetPesticidesByDiseaseViewBag(model.DiseaseId);
                return View();
            }

            _toastNotification.AddErrorToastMessage("Fail to add symptom for disease!");
            await GetPesticidesByDiseaseViewBag(model.DiseaseId);
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> DeletePesticidesByDisease(DiseasesNeedsPesticides model)
        {
            HttpResponseMessage response = await client.DeleteAsync(DiseasesNeedsPesticidesApiUrl + "/" + model.DiseaseId + "/" + model.PesticideId);

            if (response.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Delete Pesticide success!");
                await GetPesticidesByDiseaseViewBag(model.DiseaseId);
                return RedirectToAction("DiseasePesticide", "Disease", new { id = model.DiseaseId });
            }

            _toastNotification.AddErrorToastMessage("Fail to add Pesticide for disease!");
            await GetPesticidesByDiseaseViewBag(model.DiseaseId);
            return RedirectToAction("DiseasePesticide", "Disease", new { id = model.DiseaseId });
        }
    }
}
