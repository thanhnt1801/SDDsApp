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
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace WebApplicationClient.Controllers
{
    public class PesticideController : Controller
    {
        private readonly HttpClient client = null;
        /* private readonly IHttpContextAccessor _httpContextAccessor;*/
        private readonly IConfiguration _configuration;
        private string PesticideApiUrl = "";
        private string PesticideImagesApiUrl = "";
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
            PesticideImagesApiUrl = "https://localhost:44397/api/Pesticides/PostPesticideImages";
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

        [Authorize]
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

        [Authorize("ADMIN")]
        public async Task<IActionResult> PesticideImages(int id)
        {
            HttpResponseMessage response;

            response = await client.GetAsync(PesticideApiUrl + "/GetImages/" + id);

            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<PesticideImages> listImage = JsonSerializer.Deserialize<List<PesticideImages>>(strData, options);
            return View(listImage);
        }

        [HttpPost, ActionName("DeleteImage")]
        [Authorize("ADMIN")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteImage(PesticideImages model)
        {
            HttpResponseMessage response = await client.DeleteAsync(PesticideApiUrl + "/DeleteImages/" + model.Id);

            if (response.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Disable Image Success!");
                return RedirectToAction("PesticideImages", "Pesticide", new { id = model.PesticideId });
            }

            return View();
        }

        [Authorize("ADMIN")]
        public async Task<ActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Create(PesticideDTO pesticideDTO)
        {
            var uploadImage = pesticideDTO.Images;
            if (uploadImage != null)
            {
                Pesticide Pesticide = new Pesticide()
                {
                    Title = pesticideDTO.Title,
                    Description = pesticideDTO.Description,
                    Status = pesticideDTO.Status,
                };

                string data = JsonSerializer.Serialize(Pesticide);

                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.PostAsync(PesticideApiUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    var lastPesticide = await GetLastOfPesticideList();

                    foreach (var item in pesticideDTO.Images)
                    {
                        string stringFileName = UploadFile(item, pesticideDTO);
                        var PesticideImages = new PesticideImages
                        {
                            ImageUrl = stringFileName,
                            PesticideId = lastPesticide.Id
                        };
                        string ImageData = JsonSerializer.Serialize(PesticideImages);
                        StringContent ImageContent = new StringContent(ImageData, Encoding.UTF8, "application/json");

                        await client.PostAsync(PesticideImagesApiUrl, ImageContent);
                    }
                    _toastNotification.AddSuccessToastMessage("Create Pesticide Success!");
                    return RedirectToAction("Index");
                }
            }
            return View();
        }

        private string UploadFile(IFormFile file, PesticideDTO pesticideDTO)
        {
            var _PesticideName = pesticideDTO.Title.ToString().Trim();
            _PesticideName = _PesticideName.Replace(" ", String.Empty);
            string _file_name = "";
            int index = file.FileName.IndexOf('.');
            _file_name = "Pesticide-" + _PesticideName.ToString() + DateTime.UtcNow.Millisecond + "." + file.FileName.Substring(index + 1);
            string _dictionaryPath = Path.Combine(_webHostEnvironment.WebRootPath + "/Images/Pesticides/");
            string _filePath = Path.Combine(_dictionaryPath, _file_name);
            using (var stream = new FileStream(_filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            String RelativePath = _filePath.Replace(_webHostEnvironment.WebRootPath, String.Empty);
            return RelativePath;
        }

        private async Task<Pesticide> GetLastOfPesticideList()
        {
            HttpResponseMessage response = await client.GetAsync(PesticideApiUrl);
            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            //test modify
            List<Pesticide> listPesticides = JsonSerializer.Deserialize<List<Pesticide>>(strData, options);
            var lastPesticide = listPesticides.LastOrDefault();
            return lastPesticide;
        }

        [Authorize("ADMIN")]
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
        public async Task<IActionResult> Edit(PesticideDTO pesticideDTO)
        {
            var uploadImage = pesticideDTO.Images;
            if (uploadImage != null)
            {
                Pesticide pesticide = new Pesticide()
                {
                    Title = pesticideDTO.Title,
                    Description = pesticideDTO.Description,
                    Status = pesticideDTO.Status,
                    UpdatedAt = DateTime.Now
                };
                string data = JsonSerializer.Serialize(pesticide);
                StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

                HttpResponseMessage responseEdit = await client.PutAsync(PesticideApiUrl + "/" + pesticide.Id, content);
                if (responseEdit.IsSuccessStatusCode)
                {
                    foreach (var item in pesticideDTO.Images)
                    {
                        string stringFileName = UploadFile(item, pesticideDTO);
                        var PesticideImages = new PesticideImages
                        {
                            ImageUrl = stringFileName,
                            PesticideId = pesticideDTO.Id
                        };
                        string ImageData = JsonSerializer.Serialize(PesticideImages);
                        StringContent ImageContent = new StringContent(ImageData, Encoding.UTF8, "application/json");

                        await client.PostAsync(PesticideImagesApiUrl, ImageContent);
                    }
                    _toastNotification.AddSuccessToastMessage("Update Pesticide Success!");
                    return RedirectToAction("Index");
                }
            }

            return View(pesticideDTO);
        }

        [Authorize("ADMIN")]
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
