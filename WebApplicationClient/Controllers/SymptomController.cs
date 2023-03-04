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
using Microsoft.AspNetCore.Authorization;
using AuthorizeAttribute = eBookStore.Filters.AuthorizeAttribute;

namespace WebApplicationClient.Controllers
{
    public class SymptomController : Controller
    {
        private readonly HttpClient client = null;
        /* private readonly IHttpContextAccessor _httpContextAccessor;*/
        private readonly IConfiguration _configuration;
        private string SymptomApiUrl = "";
        private string SymptomImagesApiUrl = "";
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
            SymptomImagesApiUrl = "https://localhost:44397/api/Symptoms/PostSymptomImages";
            /*            SymptomApiUrl = "https://localhost:44344/apigateway/DiseaseService/Symptoms";
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
            HttpResponseMessage response = await client.GetAsync(SymptomApiUrl);
            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Symptom> listSymptoms = JsonSerializer.Deserialize<List<Symptom>>(strData, options);
            return View(listSymptoms);
        }

        [AllowAnonymous]
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

        [Authorize("ADMIN")]
        public async Task<IActionResult> SymptomImages(int id)
        {
            HttpResponseMessage response;

            response = await client.GetAsync(SymptomApiUrl + "/GetImages/" + id);

            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            ViewBag.Id = id;
            List<SymptomImages> listImage = JsonSerializer.Deserialize<List<SymptomImages>>(strData, options);
            return View(listImage);
        }

        [HttpPost, ActionName("DeleteImage")]
        [Authorize("ADMIN")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteImage(SymptomImages model)
        {
            HttpResponseMessage response = await client.DeleteAsync(SymptomApiUrl + "/DeleteImages/" + model.Id);

            if (response.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Disable Image Success!");
                return RedirectToAction("SymptomImages", "Symptom", new { id = model.SymptomId });
            }

            return View();
        }

        [Authorize("ADMIN")]
        public async Task<ActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(SymptomDTO symptomDTO)
        {
            var uploadImage = symptomDTO.Images;
            if (uploadImage != null)
            {
                Symptom symptom = new Symptom()
                {
                    Title = symptomDTO.Title,
                    Description = symptomDTO.Description,
                    Status = symptomDTO.Status
                };

                string data = JsonSerializer.Serialize(symptom);

                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(SymptomApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var lastSymptom = await GetLastOfSymptomList();

                    foreach (var item in symptomDTO.Images)
                    {
                        string stringFileName = UploadFile(item, symptomDTO);
                        var SymptomImages = new SymptomImages
                        {
                            ImageUrl = stringFileName,
                            SymptomId = lastSymptom.Id
                        };
                        string ImageData = JsonSerializer.Serialize(SymptomImages);
                        StringContent ImageContent = new StringContent(ImageData, Encoding.UTF8, "application/json");

                        await client.PostAsync(SymptomImagesApiUrl, ImageContent);
                    }
                    _toastNotification.AddSuccessToastMessage("Create Symptom Success!");
                    return RedirectToAction("Index");
                }
            }
            return View();
        }

        private string UploadFile(IFormFile file, SymptomDTO symptomDTO)
        {
            var _symptomName = symptomDTO.Title.ToString().Trim();
            _symptomName = _symptomName.Replace(" ", String.Empty);
            string _file_name = "";
            int index = file.FileName.IndexOf('.');
            _file_name = "symptom-" + _symptomName.ToString() + DateTime.UtcNow.Millisecond + "." + file.FileName.Substring(index + 1);
            string _dictionaryPath = Path.Combine(_webHostEnvironment.WebRootPath + "/Images/Symptoms/");
            string _filePath = Path.Combine(_dictionaryPath, _file_name);
            using (var stream = new FileStream(_filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            String RelativePath = _filePath.Replace(_webHostEnvironment.WebRootPath, String.Empty);
            return RelativePath;
        }

        private async Task<Symptom> GetLastOfSymptomList()
        {
            HttpResponseMessage response = await client.GetAsync(SymptomApiUrl);
            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            //test modify
            List<Symptom> listSymptoms = JsonSerializer.Deserialize<List<Symptom>>(strData, options);
            var lastSymptom = listSymptoms.LastOrDefault();
            return lastSymptom;
        }

        [Authorize("ADMIN")]
        public async Task<ActionResult> Edit(int id)
        {
            /*if (session.GetString("Role") == "User") return RedirectToAction("Index", "Home");*/
            var model = new SymptomDTO();
            HttpResponseMessage responseSymptom = await client.GetAsync(SymptomApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (responseSymptom.IsSuccessStatusCode)
            {
                string diseaseData = await responseSymptom.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<SymptomDTO>(diseaseData, options);
            }
            return View("Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(SymptomDTO symptomDTO)
        {
            var uploadImage = symptomDTO.Images;
            
                Symptom symptom = new Symptom()
                {
                    Id = symptomDTO.Id,
                    Title = symptomDTO.Title,
                    Description = symptomDTO.Description,
                    Status = symptomDTO.Status,
                    UpdatedAt = DateTime.Now
                };
                string data = JsonSerializer.Serialize(symptom);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpResponseMessage responseEdit = await client.PutAsync(SymptomApiUrl + "/" + symptom.Id, content);
                if (responseEdit.IsSuccessStatusCode)
                {
                    if(uploadImage != null)
                    {
                        try
                        {
                            foreach (var item in symptomDTO.Images)
                            {
                                string stringFileName = UploadFile(item,symptomDTO);
                                var SymptomImages = new SymptomImages
                                {
                                    ImageUrl = stringFileName,
                                    SymptomId = symptomDTO.Id
                                };
                                string ImageData = JsonSerializer.Serialize(SymptomImages);
                                StringContent ImageContent = new StringContent(ImageData, Encoding.UTF8, "application/json");

                                await client.PostAsync(SymptomImagesApiUrl, ImageContent);
                            }
                            _toastNotification.AddSuccessToastMessage("Update Pesticide Success!");
                            return RedirectToAction("Index");

                        }catch (Exception ex)
                        {
                            _toastNotification.AddErrorToastMessage("Something is wrong while updating!");
                            return View(symptomDTO.Id);
                        }

                    }
                    _toastNotification.AddSuccessToastMessage("Update Cause Success!");
                    return RedirectToAction("Index");
                }
            

            return View(symptomDTO);
        }

        [Authorize("ADMIN")]
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