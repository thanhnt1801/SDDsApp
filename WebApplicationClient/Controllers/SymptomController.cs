using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System;
using WebApplicationClient.Models;
using Microsoft.AspNetCore.Authorization;
using NToastNotify;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using WebApplicationClient.DTOs;

namespace WebApplicationClient.Controllers
{
    public class SymptomController : Controller
    {
        private readonly HttpClient client = null;
        /* private readonly IHttpContextAccessor _httpContextAccessor;*/
        private readonly IConfiguration _configuration;
        private string SymptomApiUrl = "";
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public SymptomController(/*IHttpContextAccessor httpContextAccessor,*/ IConfiguration configuration, 
            IToastNotification toastNotification,
            IWebHostEnvironment webHostEnvironment)
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            SymptomApiUrl = "https://localhost:44397/api/Symptoms";
/*            SymptomApiUrl = "https://localhost:44344/apigateway/DiseaseService/Symptoms";
*/            /*_httpContextAccessor = httpContextAccessor;*/
            _configuration = configuration;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
        }
        /*public ISession session { get { return _httpContextAccessor.HttpContext.Session; } }*/

        public async Task<IActionResult> Index()
        {
            /*if (session.GetString("User") == null) return RedirectToAction("Index", "Home");*/
            HttpResponseMessage response = await client.GetAsync(SymptomApiUrl);
            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Symptom> listSymptoms = JsonSerializer.Deserialize<List<Symptom>>(strData, options);
            return View(listSymptoms);
        }

        public async Task<ActionResult> Details(int id)
        {
            var model = new Symptom();
            HttpResponseMessage responseSymptom = await client.GetAsync(SymptomApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (responseSymptom.IsSuccessStatusCode)
            {
                string diseaseData = await responseSymptom.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<Symptom>(diseaseData, options);
            }

            return View("Details", model);
        }

        public async Task<ActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(SymptomDTO symptomDTO)
        {
            var uploadImage = symptomDTO.Image;
            if (uploadImage != null && uploadImage.Length > 0)
            {
                var _symptomName = symptomDTO.Title.ToString().Trim();
                _symptomName = _symptomName.Replace(" ", String.Empty);
                string _file_name = "";
                int index = uploadImage.FileName.IndexOf('.');
                _file_name = "symptom-" + _symptomName.ToString() + "." + uploadImage.FileName.Substring(index + 1);
                string _dictionaryPath = Path.Combine(_webHostEnvironment.WebRootPath + "/Images/Symptoms/");
                string _filePath = Path.Combine(_dictionaryPath, _file_name);
                using (var stream = new FileStream(_filePath, FileMode.Create))
                {
                    uploadImage.CopyTo(stream);
                }
                String RelativePath = _filePath.Replace(_webHostEnvironment.WebRootPath, String.Empty);

                Symptom symptom = new Symptom()
                {
                    Title = symptomDTO.Title,
                    Description = symptomDTO.Description,
                    Status = symptomDTO.Status,
                    Image = RelativePath
                };

                string data = JsonSerializer.Serialize(symptom);

                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(SymptomApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _toastNotification.AddSuccessToastMessage("Create Symptom Success!");
                    return RedirectToAction("Index");
                }
            }
            return View();
        }

        public async Task<ActionResult> Edit(int id)
        {
            /*if (session.GetString("Role") == "User") return RedirectToAction("Index", "Home");*/
            var model = new Symptom();
            HttpResponseMessage responseSymptom = await client.GetAsync(SymptomApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (responseSymptom.IsSuccessStatusCode)
            {
                string diseaseData = await responseSymptom.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<Symptom>(diseaseData, options);
            }
            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Symptom disease)
        {
            disease.UpdatedAt = DateTime.Now;
            string data = JsonSerializer.Serialize(disease);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpResponseMessage responseEdit = await client.PutAsync(SymptomApiUrl + "/" + disease.Id, content);
            if (responseEdit.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Update Symptom Success!");
                return RedirectToAction("Index");
            }
            return View(disease);
        }

        public async Task<ActionResult> Delete(int id)
        {
            var model = new Symptom();
            HttpResponseMessage responseSymptom = await client.GetAsync(SymptomApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (responseSymptom.IsSuccessStatusCode)
            {
                string diseaseData = await responseSymptom.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<Symptom>(diseaseData, options);
            }

            return View("Delete", model);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            HttpResponseMessage getSymptom = await client.GetAsync(SymptomApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            string strData = await getSymptom.Content.ReadAsStringAsync();

            var disease = JsonSerializer.Deserialize<Symptom>(strData, options);

            HttpResponseMessage response = await client.DeleteAsync(SymptomApiUrl + "/" + disease.Id);

            if (response.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Disable Symptom Success!");
                return RedirectToAction("Index");
            }

            return View();
        }
    }
}