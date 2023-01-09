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
using NToastNotify;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using WebApplicationClient.DTOs;

namespace WebApplicationClient.Controllers
{
    public class PreventativeMeasureController : Controller
    {
        private readonly HttpClient client = null;
        /* private readonly IHttpContextAccessor _httpContextAccessor;*/
        private readonly IConfiguration _configuration;
        private string PreventativeMeasureApiUrl = "";
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PreventativeMeasureController(/*IHttpContextAccessor httpContextAccessor,*/ 
            IConfiguration configuration, 
            IToastNotification toastNotification,
            IWebHostEnvironment webHostEnvironment)
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            PreventativeMeasureApiUrl = "https://localhost:44397/api/PreventativeMeasures";
/*            PreventativeMeasureApiUrl = "https://localhost:44344/apigateway/DiseaseService/PreventativeMeasures";
*/            /*_httpContextAccessor = httpContextAccessor;*/
            _configuration = configuration;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
        }
        /*public ISession session { get { return _httpContextAccessor.HttpContext.Session; } }*/

        public async Task<IActionResult> Index()
        {
            /*if (session.GetString("User") == null) return RedirectToAction("Index", "Home");*/
            HttpResponseMessage response = await client.GetAsync(PreventativeMeasureApiUrl);
            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<PreventativeMeasure> listPreventativeMeasures = JsonSerializer.Deserialize<List<PreventativeMeasure>>(strData, options);
            return View(listPreventativeMeasures);
        }

        public async Task<ActionResult> Details(int id)
        {
            var model = new PreventativeMeasure();
            HttpResponseMessage responsePreventativeMeasure = await client.GetAsync(PreventativeMeasureApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (responsePreventativeMeasure.IsSuccessStatusCode)
            {
                string diseaseData = await responsePreventativeMeasure.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<PreventativeMeasure>(diseaseData, options);
            }

            return View("Details", model);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(PreventativeMeasureDTO preventativeMeasureDTO)
        {
            var uploadImage = preventativeMeasureDTO.Image;
            if (uploadImage != null && uploadImage.Length > 0)
            {
                var _PreventativeMeasure = preventativeMeasureDTO.Title.ToString().Trim();
                _PreventativeMeasure = _PreventativeMeasure.Replace(" ", String.Empty);
                string _file_name = "";
                int index = uploadImage.FileName.IndexOf('.');
                _file_name = "Measure-" + _PreventativeMeasure.ToString() + "." + uploadImage.FileName.Substring(index + 1);
                string _dictionaryPath = Path.Combine(_webHostEnvironment.WebRootPath + "/Images/PreventativeMeasures/");
                string _filePath = Path.Combine(_dictionaryPath, _file_name);
                using (var stream = new FileStream(_filePath, FileMode.Create))
                {
                    uploadImage.CopyTo(stream);
                }
                String RelativePath = _filePath.Replace(_webHostEnvironment.WebRootPath, String.Empty);

                PreventativeMeasure preventMeasure = new PreventativeMeasure()
                {
                    Title = preventativeMeasureDTO.Title,
                    Description = preventativeMeasureDTO.Description,
                    Status = preventativeMeasureDTO.Status,
                    Image = RelativePath
                };

                string data = JsonSerializer.Serialize(preventMeasure);

                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(PreventativeMeasureApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _toastNotification.AddSuccessToastMessage("Create PreventativeMeasure Success!");
                    return RedirectToAction("Index");
                }
            }
            return View();
        }

        public async Task<ActionResult> Edit(int id)
        {
            /*if (session.GetString("Role") == "User") return RedirectToAction("Index", "Home");*/
            var model = new PreventativeMeasure();
            HttpResponseMessage responsePreventativeMeasure = await client.GetAsync(PreventativeMeasureApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (responsePreventativeMeasure.IsSuccessStatusCode)
            {
                string diseaseData = await responsePreventativeMeasure.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<PreventativeMeasure>(diseaseData, options);
            }
            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PreventativeMeasure disease)
        {
            disease.UpdatedAt = DateTime.Now;
            string data = JsonSerializer.Serialize(disease);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpResponseMessage responseEdit = await client.PutAsync(PreventativeMeasureApiUrl + "/" + disease.Id, content);
            if (responseEdit.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Update PreventativeMeasure Success!");
                return RedirectToAction("Index");
            }
            return View(disease);
        }

        public async Task<ActionResult> Delete(int id)
        {
            var model = new PreventativeMeasure();
            HttpResponseMessage responsePreventativeMeasure = await client.GetAsync(PreventativeMeasureApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (responsePreventativeMeasure.IsSuccessStatusCode)
            {
                string diseaseData = await responsePreventativeMeasure.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<PreventativeMeasure>(diseaseData, options);
            }

            return View("Delete", model);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            HttpResponseMessage getPreventativeMeasure = await client.GetAsync(PreventativeMeasureApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            string strData = await getPreventativeMeasure.Content.ReadAsStringAsync();

            var disease = JsonSerializer.Deserialize<PreventativeMeasure>(strData, options);

            HttpResponseMessage response = await client.DeleteAsync(PreventativeMeasureApiUrl + "/" + disease.Id);

            if (response.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Disable PreventativeMeasure Success!");
                return RedirectToAction("Index");
            }

            return View();
        }
    }
}