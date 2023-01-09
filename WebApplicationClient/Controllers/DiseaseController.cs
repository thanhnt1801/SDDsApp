using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplicationClient.Models;
using System.Text;
using System;
using static System.Collections.Specialized.BitVector32;
using NToastNotify;
using eBookStore.Filters;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using WebApplicationClient.DTOs;

namespace WebApplicationClient.Controllers
{
    public class DiseaseController : Controller
    {
        private readonly HttpClient client = null;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private string DiseaseApiUrl = "";

        public DiseaseController(IHttpContextAccessor httpContextAccessor, 
            IConfiguration configuration, IToastNotification toastNotification, 
            IWebHostEnvironment webHostEnvironment)
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            DiseaseApiUrl = "https://localhost:44397/api/Diseases";
            /*DiseaseApiUrl = "https://localhost:44344/apigateway/DiseaseService/Diseases"; */
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
        }
        public ISession session { get { return _httpContextAccessor.HttpContext.Session; } }

        [Authorize]
        public async Task<IActionResult> Index(string query)
        {
            /*if (session.GetString("role") == "ADMIN") return RedirectToAction("Login", "Authentication");*/
            HttpResponseMessage response;

            if (string.IsNullOrEmpty(query))
            {
                response = await client.GetAsync(DiseaseApiUrl);
            }
            else
            {
                response = await client.GetAsync(DiseaseApiUrl + "/search?query=" + query.ToUpper());
            }

            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Disease> listDiseases = JsonSerializer.Deserialize<List<Disease>>(strData, options);
            return View(listDiseases);
        }
        [Authorize]
        public async Task<ActionResult> Details(int id)
        {
            var model = new Disease();
            HttpResponseMessage responseDisease = await client.GetAsync(DiseaseApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (responseDisease.IsSuccessStatusCode)
            {
                string diseaseData = await responseDisease.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<Disease>(diseaseData, options);
            }

            return View("Details", model);
        }

        [Authorize("ADMIN")]
        public async Task<ActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize("ADMIN")]
        public async Task<ActionResult> CreateAsync(DiseaseDTO diseaseDTO)
        {
            var uploadImage = diseaseDTO.Image;
            if (uploadImage != null && uploadImage.Length > 0)
            {
                var _diseaseName = diseaseDTO.Name.ToString().Trim();
                _diseaseName = _diseaseName.Replace(" ", String.Empty);
                string _file_name = "";
                int index = uploadImage.FileName.IndexOf('.');
                _file_name = "disease-" + _diseaseName.ToString() + "." + uploadImage.FileName.Substring(index + 1);
                string _dictionaryPath = Path.Combine(_webHostEnvironment.WebRootPath + "/Images/Diseases/");
                string _filePath = Path.Combine(_dictionaryPath, _file_name);
                using (var stream = new FileStream(_filePath, FileMode.Create))
                {
                    uploadImage.CopyTo(stream);
                }
                String RelativePath = _filePath.Replace(_webHostEnvironment.WebRootPath, String.Empty);
                Disease disease = new Disease()
                {
                    Name = diseaseDTO.Name,
                    Description = diseaseDTO.Description,
                    Status = diseaseDTO.Status,
                    Image = RelativePath
                };

                string data = JsonSerializer.Serialize(disease);

                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(DiseaseApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _toastNotification.AddSuccessToastMessage("Create Disease Success!");
                    return RedirectToAction("Index");
                }
            }

            return View();
        }


        [Authorize("ADMIN")]
        public async Task<ActionResult> Edit(int id)
        {
            if (session.GetString("Role") == "User") return RedirectToAction("Index", "Home");
            var model = new Disease();
            HttpResponseMessage responseDisease = await client.GetAsync(DiseaseApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (responseDisease.IsSuccessStatusCode)
            {
                string diseaseData = await responseDisease.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<Disease>(diseaseData, options);
            }
            return View("Edit", model);
        }

        [HttpPost]
        [Authorize("ADMIN")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Disease disease)
        {
            disease.UpdatedAt = DateTime.Now;
            string data = JsonSerializer.Serialize(disease);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpResponseMessage responseEdit = await client.PutAsync(DiseaseApiUrl + "/" + disease.Id, content);
            if (responseEdit.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Update Disease Success!");
                return RedirectToAction("Index");
            }
            return View(disease);
        }

        [Authorize("ADMIN")]
        public async Task<ActionResult> Delete(int id)
        {
            var model = new Disease();
            HttpResponseMessage responseDisease = await client.GetAsync(DiseaseApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (responseDisease.IsSuccessStatusCode)
            {
                string diseaseData = await responseDisease.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<Disease>(diseaseData, options);
            }

            return View("Delete", model);
        }


        [HttpPost, ActionName("Delete")]
        [Authorize("ADMIN")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            HttpResponseMessage getDisease = await client.GetAsync(DiseaseApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            string strData = await getDisease.Content.ReadAsStringAsync();

            var disease = JsonSerializer.Deserialize<Disease>(strData, options);

            HttpResponseMessage response = await client.DeleteAsync(DiseaseApiUrl + "/" + disease.Id);

            if (response.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Disable Disease Success!");
                return RedirectToAction("Index");
            }

            return View();
        }

        [Authorize]
        public async Task<IActionResult> DiseaseSymptom(long id)
        {
            HttpResponseMessage responseDisease = await client.GetAsync(DiseaseApiUrl + "/" + id + "/Symptom");

            string strData = await responseDisease.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Symptom> listDiseases = JsonSerializer.Deserialize<List<Symptom>>(strData, options);
            ViewBag.listDiseases = listDiseases;
            ViewBag.diseaseId = id;
            return View();
        }

        [Authorize]
        public async Task<IActionResult> DiseaseMeasure(long id)
        {
            HttpResponseMessage responseMeasure = await client.GetAsync(DiseaseApiUrl + "/" + id + "/Measure");

            string strData = await responseMeasure.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<PreventativeMeasure> listMeasures = JsonSerializer.Deserialize<List<PreventativeMeasure>>(strData, options);
            ViewBag.listMeasures = listMeasures;
            ViewBag.diseaseId = id;
            return View();
        }

        [Authorize]
        public async Task<IActionResult> DiseasePesticide(long id)
        {
            HttpResponseMessage responsePesticide = await client.GetAsync(DiseaseApiUrl + "/" + id + "/Pesticide");

            string strData = await responsePesticide.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Pesticide> listPesticides = JsonSerializer.Deserialize<List<Pesticide>>(strData, options);
            ViewBag.listPesticides = listPesticides;
            ViewBag.diseaseId = id;
            return View();
        }

        [Authorize]
        public async Task<IActionResult> DiseaseCause(long id)
        {
            HttpResponseMessage responseCause = await client.GetAsync(DiseaseApiUrl + "/" + id + "/Cause");

            string strData = await responseCause.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Cause> listCauses = JsonSerializer.Deserialize<List<Cause>>(strData, options);
            ViewBag.listCauses = listCauses;
            ViewBag.diseaseId = id;
            return View();
        }
       
        public async Task<IActionResult> DiseaseUploadByUser(long id)
        {
            var model = new Disease();
            HttpResponseMessage responseDisease = await client.GetAsync(DiseaseApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (responseDisease.IsSuccessStatusCode)
            {
                string diseaseData = await responseDisease.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<Disease>(diseaseData, options);
            }

            HttpResponseMessage responseCause = await client.GetAsync(DiseaseApiUrl + "/" + id + "/Cause");

            string strData = await responseCause.Content.ReadAsStringAsync();

            List<Cause> listCauses = JsonSerializer.Deserialize<List<Cause>>(strData, options);
            ViewBag.listCauses = listCauses;

            HttpResponseMessage responseSymptom = await client.GetAsync(DiseaseApiUrl + "/" + id + "/Symptom");

            string strData1 = await responseSymptom.Content.ReadAsStringAsync();

            List<Symptom> listSymptoms = JsonSerializer.Deserialize<List<Symptom>>(strData1, options);
            ViewBag.listSymptoms = listSymptoms;

            HttpResponseMessage responseMeasure = await client.GetAsync(DiseaseApiUrl + "/" + id + "/Measure");

            string strData3 = await responseMeasure.Content.ReadAsStringAsync();

            List<PreventativeMeasure> listMeasures = JsonSerializer.Deserialize<List<PreventativeMeasure>>(strData3, options);
            ViewBag.listMeasures = listMeasures;

            HttpResponseMessage responsePesticide = await client.GetAsync(DiseaseApiUrl + "/" + id + "/Pesticide");

            string strData4 = await responsePesticide.Content.ReadAsStringAsync();

            List<Pesticide> listPesticides = JsonSerializer.Deserialize<List<Pesticide>>(strData4, options);
            ViewBag.listPesticides = listPesticides;


            return View(model);
        }
    }
}
