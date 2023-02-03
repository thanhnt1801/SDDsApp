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
using eBookStore.Filters;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace WebApplicationClient.Controllers
{
    public class PreventativeMeasureController : Controller
    {
        private readonly HttpClient client = null;
        /* private readonly IHttpContextAccessor _httpContextAccessor;*/
        private readonly IConfiguration _configuration;
        private string PreventativeMeasureApiUrl = "";
        private string PreventativeMeasureImagesApiUrl = "";
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
            PreventativeMeasureImagesApiUrl = "https://localhost:44397/api/PreventativeMeasures/PostPreventativeMeasureImages";
            /*            PreventativeMeasureApiUrl = "https://localhost:44344/apigateway/DiseaseService/PreventativeMeasures";
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
            HttpResponseMessage response = await client.GetAsync(PreventativeMeasureApiUrl);
            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<PreventativeMeasure> listPreventativeMeasures = JsonSerializer.Deserialize<List<PreventativeMeasure>>(strData, options);
            return View(listPreventativeMeasures);
        }

        [Authorize]
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

        [Authorize("ADMIN")]
        public async Task<IActionResult> PreventativeMeasureImages(int id)
        {
            HttpResponseMessage response;

            response = await client.GetAsync(PreventativeMeasureApiUrl + "/GetImages/" + id);

            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<PreventativeMeasureImages> listImage = JsonSerializer.Deserialize<List<PreventativeMeasureImages>>(strData, options);
            return View(listImage);
        }

        [HttpPost, ActionName("DeleteImage")]
        [Authorize("ADMIN")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteImage(PreventativeMeasureImages model)
        {
            HttpResponseMessage response = await client.DeleteAsync(PreventativeMeasureApiUrl + "/DeleteImages/" + model.Id);

            if (response.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Disable Image Success!");
                return RedirectToAction("PreventativeMeasureImage", "PreventativeMeasure", model.PreventativeMeasureId);
            }

            return View();
        }

        [Authorize("ADMIN")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(PreventativeMeasureDTO preventativeMeasureDTO)
        {
            var uploadImage = preventativeMeasureDTO.Images;
            if (uploadImage != null)
            {
                

                PreventativeMeasure preventMeasure = new PreventativeMeasure()
                {
                    Title = preventativeMeasureDTO.Title,
                    Description = preventativeMeasureDTO.Description,
                    Status = preventativeMeasureDTO.Status,
                };

                string data = JsonSerializer.Serialize(preventMeasure);

                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(PreventativeMeasureApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var lastPreventativeMeasure = await GetLastOfPreventativeMeasureList();

                    foreach (var item in preventativeMeasureDTO.Images)
                    {
                        string stringFileName = UploadFile(item, preventativeMeasureDTO);
                        var PreventativeMeasureImages = new PreventativeMeasureImages
                        {
                            ImageUrl = stringFileName,
                            PreventativeMeasureId = lastPreventativeMeasure.Id
                        };
                        string ImageData = JsonSerializer.Serialize(PreventativeMeasureImages);
                        StringContent ImageContent = new StringContent(ImageData, Encoding.UTF8, "application/json");

                        await client.PostAsync(PreventativeMeasureImagesApiUrl, ImageContent);
                    }

                    _toastNotification.AddSuccessToastMessage("Create PreventativeMeasure Success!");
                    return RedirectToAction("Index");
                }
            }
            return View();
        }

        private string UploadFile(IFormFile file, PreventativeMeasureDTO preventativeMeasureDTO)
        {
            var _PreventativeMeasure = preventativeMeasureDTO.Title.ToString().Trim();
            _PreventativeMeasure = _PreventativeMeasure.Replace(" ", String.Empty);
            string _file_name = "";
            int index = file.FileName.IndexOf('.');
            _file_name = "Measure-" + _PreventativeMeasure.ToString() + DateTime.UtcNow.Millisecond + "." + file.FileName.Substring(index + 1);
            string _dictionaryPath = Path.Combine(_webHostEnvironment.WebRootPath + "/Images/PreventativeMeasures/");
            string _filePath = Path.Combine(_dictionaryPath, _file_name);
            using (var stream = new FileStream(_filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            String RelativePath = _filePath.Replace(_webHostEnvironment.WebRootPath, String.Empty);
            return RelativePath;
        }

        private async Task<PreventativeMeasure> GetLastOfPreventativeMeasureList()
        {
            HttpResponseMessage response = await client.GetAsync(PreventativeMeasureApiUrl);
            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            //test modify
            List<PreventativeMeasure> listPreventativeMeasures = JsonSerializer.Deserialize<List<PreventativeMeasure>>(strData, options);
            var lastPreventativeMeasure = listPreventativeMeasures.LastOrDefault();
            return lastPreventativeMeasure;
        }

        [Authorize("ADMIN")]
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
        public async Task<IActionResult> Edit(PreventativeMeasureDTO preventativeMeasureDTO)
        {
            var uploadImage = preventativeMeasureDTO.Images;
            if (uploadImage != null)
            {
                PreventativeMeasure preventativeMeasure = new PreventativeMeasure()
                {
                    Title = preventativeMeasureDTO.Title,
                    Description = preventativeMeasureDTO.Description,
                    Status = preventativeMeasureDTO.Status,
                    UpdatedAt = DateTime.Now
                };
                string data = JsonSerializer.Serialize(preventativeMeasure);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpResponseMessage responseEdit = await client.PutAsync(PreventativeMeasureApiUrl + "/" + preventativeMeasure.Id, content);
                if (responseEdit.IsSuccessStatusCode)
                {
                    foreach (var item in preventativeMeasureDTO.Images)
                    {
                        string stringFileName = UploadFile(item, preventativeMeasureDTO);
                        var PreventativeMeasureImages = new PreventativeMeasureImages
                        {
                            ImageUrl = stringFileName,
                            PreventativeMeasureId = preventativeMeasureDTO.Id
                        };
                        string ImageData = JsonSerializer.Serialize(PreventativeMeasureImages);
                        StringContent ImageContent = new StringContent(ImageData, Encoding.UTF8, "application/json");

                        await client.PostAsync(PreventativeMeasureImagesApiUrl, ImageContent);
                    }
                    _toastNotification.AddSuccessToastMessage("Update Preventative Measure Success!");
                    return RedirectToAction("Index");
                }
            }

            return View(preventativeMeasureDTO);
        }

        [Authorize("ADMIN")]
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