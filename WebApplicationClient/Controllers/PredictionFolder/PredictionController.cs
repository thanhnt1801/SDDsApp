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
using System;
using System.Collections;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using WebApplicationClient.DTOs;
using eBookStore.Filters;

namespace WebApplicationClient.Controllers.PredictionFolder
{
    public class PredictionController : Controller
    {
        private readonly HttpClient client = null;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IToastNotification _toastNotification;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private string PredictionApiUrl = "";
        private string DiseaseApiUrl = "";

        public PredictionController(IHttpContextAccessor httpContextAccessor, IToastNotification toastNotification,IWebHostEnvironment webHostEnvironment)
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            PredictionApiUrl = "https://localhost:44351/api/Predictions";
            DiseaseApiUrl = "https://localhost:44397/api/Diseases";
            _httpContextAccessor = httpContextAccessor;
            _toastNotification = toastNotification;
            _webHostEnvironment = webHostEnvironment;
        }
        public ISession session { get { return _httpContextAccessor.HttpContext.Session; } }

        private bool TokenAdded()
        {
            string token = session.GetString("jwtToken");

            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            return true;
        }

        [Authorize("MEMBER")]
        public IActionResult AddPrediction()
        {
            if (!TokenAdded())
            {
                return RedirectToAction("Login","Authentication");
            }
            return View();
        }

        [HttpPost]
        [Authorize("MEMBER")]
        public async Task<IActionResult> AddPrediction(PredictionDTO model)
        {
            HttpResponseMessage responseDisease = await client.GetAsync(DiseaseApiUrl);

            string strData = await responseDisease.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<Disease> listDiseases = JsonSerializer.Deserialize<List<Disease>>(strData, options);

            var array = new List<long>();

            listDiseases.ForEach(d =>{ array.Add(d.Id); });

            Random random = new Random();
            int predictionIdRandom = random.Next(1, array.ToArray().Length);

            var RelativePath = String.Empty;

            var uploadImage = model.InputImagePath;
            if (uploadImage != null && uploadImage.Length > 0)
            {
                var _predictName = "PredictionName" + DateTime.UtcNow.Millisecond;
                _predictName = _predictName.Replace(" ", String.Empty);
                string _file_name = "";
                int index = uploadImage.FileName.IndexOf('.');
                _file_name = "predict-" + _predictName.ToString() + "." + uploadImage.FileName.Substring(index + 1);
                string _dictionaryPath = Path.Combine(_webHostEnvironment.WebRootPath + "/Images/Predictions/");
                string _filePath = Path.Combine(_dictionaryPath, _file_name);
                using (var stream = new FileStream(_filePath, FileMode.Create))
                {
                    uploadImage.CopyTo(stream);
                }
                RelativePath = _filePath.Replace(_webHostEnvironment.WebRootPath, String.Empty);
            }



            var predictionModel = new Prediction()
            {
                DiseaseId = array[predictionIdRandom],
                FarmerId = Guid.Parse(session.GetString("id")),
                ExpertId = Guid.Parse("bdcb2feb-a225-4aec-699f-08dac002fd31"),
                InputImagePath = RelativePath,
                OutputImage = RelativePath,
                PredictResult = "Angular leaf spot",
                ExpertConfirmation = String.Empty,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                DeletedAt = DateTime.Now,
                Status = true,
                PredictionPercent = "98%"
            };
            
            string data = JsonSerializer.Serialize(predictionModel);
            StringContent content = new StringContent(data, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(PredictionApiUrl, content);

            if (response.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Upload prediction image success!");
                return RedirectToAction("DiseaseUploadByUser", "Disease", new { id = predictionModel.DiseaseId });
            }

            _toastNotification.AddErrorToastMessage("Something wrong when try to upload prediction image!");
            return View();
        }

        [Authorize("MEMBER")]
        public async Task<IActionResult> HistoryFarmer()
        {
            if (!TokenAdded())
            {
                return RedirectToAction("Login", "Authentication");
            }

            var farmerId = session.GetString("id");

            HttpResponseMessage response =  await client.GetAsync(PredictionApiUrl + "/GetHistoryByFarmer/" + farmerId);
            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var listPredictions = JsonSerializer.Deserialize<IEnumerable<Prediction>>(strData, options);
            return View(listPredictions);
        }

        [Authorize("EXPERT")]
        public async Task<IActionResult> HistoryExpert()
        {
            if (!TokenAdded())
            {
                return RedirectToAction("Login", "Authentication");
            }

            var expertId = session.GetString("id");

            HttpResponseMessage response = await client.GetAsync(PredictionApiUrl + "/GetHistoryByExpert/" + expertId);
            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var listPredictions = JsonSerializer.Deserialize<IEnumerable<Prediction>>(strData, options);
            return View(listPredictions);
        }

        /* [HttpPost]
         public async Task<IActionResult> DeleteCausesByDisease(DiseasesHasCauses model)
         {
             HttpResponseMessage response = await client.DeleteAsync(DiseasesHasCausesApiUrl + "/" + model.DiseaseId + "/" + model.CauseId);

             if (response.IsSuccessStatusCode)
             {
                 _toastNotification.AddSuccessToastMessage("Delete Cause success!");
                 await GetCausesByDiseaseViewBag(model.DiseaseId);
                 return RedirectToAction("DiseaseCause", "Disease", new { id = model.DiseaseId });
             }

             _toastNotification.AddErrorToastMessage("Fail to add Cause for disease!");
             await GetCausesByDiseaseViewBag(model.DiseaseId);
             return RedirectToAction("DiseaseCause", "Disease", new { id = model.DiseaseId });
         }*/
    }
}
