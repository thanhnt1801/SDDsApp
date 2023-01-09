using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplicationClient.DTOs;
using WebApplicationClient.Models;

namespace WebApplicationClient.Controllers
{
    public class CauseController : Controller
    {
        private readonly HttpClient client = null;
        /* private readonly IHttpContextAccessor _httpContextAccessor;*/
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IToastNotification _toastNotification;
        private string CauseApiUrl = "";

        public CauseController(/*IHttpContextAccessor httpContextAccessor,*/ IConfiguration configuration, 
            IWebHostEnvironment webHostEnvironment,
            IToastNotification toastNotification)
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            CauseApiUrl = "https://localhost:44397/api/Causes";
            /*CauseApiUrl = "https://localhost:44344/apigateway/DiseaseService/Causes";*/
            /*_httpContextAccessor = httpContextAccessor;*/
            _configuration = configuration;
            _webHostEnvironment = webHostEnvironment;
            _toastNotification = toastNotification;
        }
        /*public ISession session { get { return _httpContextAccessor.HttpContext.Session; } }*/

        public async Task<IActionResult> Index()
        {
            /*if (session.GetString("User") == null) return RedirectToAction("Index", "Home");*/
            HttpResponseMessage response = await client.GetAsync(CauseApiUrl);
            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Cause> listCauses = JsonSerializer.Deserialize<List<Cause>>(strData, options);
            return View(listCauses);
        }

        public async Task<ActionResult> Details(int id)
        {
            var model = new Cause();
            HttpResponseMessage responseCause = await client.GetAsync(CauseApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (responseCause.IsSuccessStatusCode)
            {
                string diseaseData = await responseCause.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<Cause>(diseaseData, options);
            }

            return View("Details", model);
        }

        public async Task<ActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(CauseDTO causeDTO)
        {
            var uploadImage = causeDTO.Image;
            if (uploadImage != null && uploadImage.Length > 0)
            {
                var _causeName = causeDTO.Title.ToString().Trim();
                _causeName = _causeName.Replace(" ", String.Empty);
                string _file_name = "";
                int index = uploadImage.FileName.IndexOf('.');
                _file_name = "cause-" + _causeName.ToString() + DateTime.UtcNow.Millisecond + "." + uploadImage.FileName.Substring(index + 1);
                string _dictionaryPath = Path.Combine(_webHostEnvironment.WebRootPath + "/Images/Causes/");

                string _filePath = Path.Combine(_dictionaryPath, _file_name);

                using (var stream = new FileStream(_filePath, FileMode.Create))
                {
                    uploadImage.CopyTo(stream);
                }
                String RelativePath = _filePath.Replace(_webHostEnvironment.WebRootPath, String.Empty);

                Cause cause = new Cause()
                {
                    Title = causeDTO.Title,
                    Description = causeDTO.Description,
                    Status = causeDTO.Status,
                    Image = RelativePath
                };

                string data = JsonSerializer.Serialize(cause);

                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(CauseApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    _toastNotification.AddSuccessToastMessage("Create Cause Success!");
                    return RedirectToAction("Index");
                }
            }
            return View();
        }

        public async Task<ActionResult> Edit(int id)
        {
            /*if (session.GetString("Role") == "User") return RedirectToAction("Index", "Home");*/
            var model = new Cause();
            HttpResponseMessage responseCause = await client.GetAsync(CauseApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (responseCause.IsSuccessStatusCode)
            {
                string causeData = await responseCause.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<Cause>(causeData, options);
            }
            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Cause cause)
        {
            cause.UpdatedAt = DateTime.Now;
            string data = JsonSerializer.Serialize(cause);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpResponseMessage responseEdit = await client.PutAsync(CauseApiUrl + "/" + cause.Id, content);
            if (responseEdit.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            return View(cause);
        }

        public async Task<ActionResult> Delete(int id)
        {
            var model = new Cause();
            HttpResponseMessage responseCause = await client.GetAsync(CauseApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (responseCause.IsSuccessStatusCode)
            {
                string diseaseData = await responseCause.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<Cause>(diseaseData, options);
            }

            return View("Delete", model);
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            HttpResponseMessage getPesticide = await client.GetAsync(CauseApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            string strData = await getPesticide.Content.ReadAsStringAsync();

            var cause = JsonSerializer.Deserialize<Cause>(strData, options);

            HttpResponseMessage response = await client.DeleteAsync(CauseApiUrl + "/" + cause.Id);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }

            return View();
        }
    }
}
