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
using eBookStore.Filters;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using WebApplicationClient.DTOs;

namespace WebApplicationClient.Controllers
{
    public class PesticideController : Controller
    {
        private readonly HttpClient client = null;
        /* private readonly IHttpContextAccessor _httpContextAccessor;*/
        private readonly IConfiguration _configuration;
        private string PesticideApiUrl = "";
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PesticideController(/*IHttpContextAccessor httpContextAccessor,*/ IConfiguration configuration, 
            IToastNotification toastNotification,
            IWebHostEnvironment webHostEnvironment)
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            PesticideApiUrl = "https://localhost:44397/api/Pesticides";
/*            PesticideApiUrl = "https://localhost:44344/apigateway/DiseaseService/Pesticides";
*/            /*_httpContextAccessor = httpContextAccessor;*/
            _configuration = configuration;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
        }
        /*public ISession session { get { return _httpContextAccessor.HttpContext.Session; } }*/
        [Authorize]
        public async Task<IActionResult> Index()
        {
            /*if (session.GetString("User") == null) return RedirectToAction("Index", "Home");*/
            HttpResponseMessage response = await client.GetAsync(PesticideApiUrl);
            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Pesticide> listPesticides = JsonSerializer.Deserialize<List<Pesticide>>(strData, options);
            return View(listPesticides);
        }

        public async Task<ActionResult> Details(int id)
        {
            var model = new Pesticide();
            HttpResponseMessage responsePesticide = await client.GetAsync(PesticideApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (responsePesticide.IsSuccessStatusCode)
            {
                string diseaseData = await responsePesticide.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<Pesticide>(diseaseData, options);
            }

            return View("Details", model);
        }

        public async Task<ActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(PesticideDTO pesticideDTO)
        {
            var uploadImage = pesticideDTO.Image;
            if (uploadImage != null && uploadImage.Length > 0)
            {
                var _PesticideName = pesticideDTO.Title.ToString().Trim();
                _PesticideName = _PesticideName.Replace(" ", String.Empty);
                string _file_name = "";
                int index = uploadImage.FileName.IndexOf('.');
                _file_name = "Pesticide-" + _PesticideName.ToString() + "." + uploadImage.FileName.Substring(index + 1);
                string _dictionaryPath = Path.Combine(_webHostEnvironment.WebRootPath + "/Images/Pesticides/");
                string _filePath = Path.Combine(_dictionaryPath, _file_name);
                using (var stream = new FileStream(_filePath, FileMode.Create))
                {
                    uploadImage.CopyTo(stream);
                }
                String RelativePath = _filePath.Replace(_webHostEnvironment.WebRootPath, String.Empty);

                Pesticide Pesticide = new Pesticide()
                {
                    Title = pesticideDTO.Title,
                    Description = pesticideDTO.Description,
                    Status = pesticideDTO.Status,
                    Image = RelativePath
                };

                string data = JsonSerializer.Serialize(Pesticide);

                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(PesticideApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _toastNotification.AddSuccessToastMessage("Create Pesticide Success!");
                    return RedirectToAction("Index");
                }
            }
            return View();
        }

        public async Task<ActionResult> Edit(int id)
        {
            /*if (session.GetString("Role") == "User") return RedirectToAction("Index", "Home");*/
            var model = new Pesticide();
            HttpResponseMessage responsePesticide = await client.GetAsync(PesticideApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (responsePesticide.IsSuccessStatusCode)
            {
                string diseaseData = await responsePesticide.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<Pesticide>(diseaseData, options);
            }
            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Pesticide disease)
        {
            disease.UpdatedAt = DateTime.Now;
            string data = JsonSerializer.Serialize(disease);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpResponseMessage responseEdit = await client.PutAsync(PesticideApiUrl + "/" + disease.Id, content);
            if (responseEdit.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Update Pesticide Success!");
                return RedirectToAction("Index");
            }
            return View(disease);
        }

        public async Task<ActionResult> Delete(int id)
        {
            var model = new Pesticide();
            HttpResponseMessage responsePesticide = await client.GetAsync(PesticideApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (responsePesticide.IsSuccessStatusCode)
            {
                string diseaseData = await responsePesticide.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<Pesticide>(diseaseData, options);
            }

            return View("Delete", model);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            HttpResponseMessage getPesticide = await client.GetAsync(PesticideApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            string strData = await getPesticide.Content.ReadAsStringAsync();

            var disease = JsonSerializer.Deserialize<Pesticide>(strData, options);

            HttpResponseMessage response = await client.DeleteAsync(PesticideApiUrl + "/" + disease.Id);

            if (response.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Disable Pesticide Success!");
                return RedirectToAction("Index");
            }

            return View();
        }
    }
}
