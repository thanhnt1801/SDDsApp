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
        private readonly string UserApiUrl = "";

        public PredictionController(IHttpContextAccessor httpContextAccessor, IToastNotification toastNotification,IWebHostEnvironment webHostEnvironment)
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
            PredictionApiUrl = "https://localhost:44351/api/Predictions";
            DiseaseApiUrl = "https://localhost:44397/api/Diseases";
            UserApiUrl = "https://localhost:44318/api/UserManagement";
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
        public async Task<IActionResult> AddPrediction(PredictionDTO predictionDTO)
        {
            try
            {
                // Read the file and convert it to a byte array
                byte[] imageData = null;
                using (var memoryStream = new MemoryStream())
                {
                    await predictionDTO.InputImagePath.CopyToAsync(memoryStream);
                    imageData = memoryStream.ToArray();
                }

                // Create a new HTTP client and set the base address to the FastAPI endpoint
                var FastAPIUrl = "http://127.0.0.1:8000/predict";

                // Create a new multipart form content and add the image file
                var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(imageData), "file", predictionDTO.InputImagePath.FileName);

                // Send the HTTP request to the FastAPI endpoint
                var response = await client.PostAsync(FastAPIUrl, content);

                // Read the response content and return the results
                var responseContent = await response.Content.ReadAsStringAsync();
                var jsonObject = JsonSerializer.Deserialize<JsonElement>(responseContent);

                // Access the "results" array and iterate over its items
                var results = jsonObject.GetProperty("results");

                double bestProbability = 0;
                string bestLabel = "";
                double worstProbability = 0;
                string worstLabel = "";
                double mediumProbability = 0;
                string mediumLabel = "";

                foreach (var result in results.EnumerateArray())
                {
                    // Access the "label" and "probability" properties of each item
                    var resultLabel = result.GetProperty("label").GetString();
                    var resultProbability = result.GetProperty("probability").GetDouble();
                    // Update the best, worst, and other labels and probabilities
                    if (resultProbability > bestProbability)
                    {
                        worstProbability = mediumProbability;
                        worstLabel = mediumLabel;
                        mediumProbability = bestProbability;
                        mediumLabel = bestLabel;
                        bestProbability = resultProbability;
                        bestLabel = resultLabel;
                    }
                    else if (resultProbability > mediumProbability)
                    {
                        worstProbability = mediumProbability;
                        worstLabel = mediumLabel;
                        mediumProbability = resultProbability;
                        mediumLabel = resultLabel;
                    }
                    else if (resultProbability > worstProbability)
                    {
                        worstProbability = resultProbability;
                        worstLabel = resultLabel;
                    }
                }

                var getRandomExpert = await GetRandomExpert();

                var uploadImage = predictionDTO.InputImagePath;
                predictionDTO.PredictResult = bestLabel;
                string stringFileName = UploadFile(uploadImage, predictionDTO);

                var predictionModel = new Prediction()
                {
                    DiseaseId = 1,
                    FarmerId = Guid.Parse(session.GetString("id")),
                    ExpertId = getRandomExpert,
                    InputImagePath = stringFileName,
                    OutputImage = stringFileName,
                    PredictResult = bestLabel,
                    ExpertConfirmation = String.Empty,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    DeletedAt = DateTime.Now,
                    Status = false,
                    PredictionPercent = Convert.ToString(bestProbability),
                };

                string data = JsonSerializer.Serialize(predictionModel);
                StringContent contentPredictModel = new StringContent(data, Encoding.UTF8, "application/json");

                HttpResponseMessage responsePredictModel = await client.PostAsync(PredictionApiUrl, contentPredictModel);

                if (responsePredictModel.IsSuccessStatusCode)
                {


                    _toastNotification.AddSuccessToastMessage("Upload prediction image success!");

                    session.SetString("bestProbability", bestProbability.ToString());
                    session.SetString("bestLabel", bestLabel);
                    session.SetString("mediumProbability", mediumProbability.ToString());
                    session.SetString("mediumLabel", mediumLabel);
                    session.SetString("worstProbability", worstProbability.ToString());
                    session.SetString("worstLabel", worstLabel);

                    return RedirectToAction("DiseaseUploadByUser", "Disease", new { id = predictionModel.DiseaseId });
                }
                else
                {
                    _toastNotification.AddErrorToastMessage("Something wrong when try to upload prediction image!");
                    return View();
                }           
            }
            catch(Exception ex)
            {
                _toastNotification.AddErrorToastMessage("Something wrong when try to upload prediction image!");
                return View();
            }
        }



        private string UploadFile(IFormFile file, PredictionDTO predictionDTO)
        {
            var _predictionName = predictionDTO.PredictResult.ToString().Trim();
            _predictionName = _predictionName.Replace(" ", String.Empty);
            string _file_name = "";
            int index = file.FileName.IndexOf('.');
            _file_name = "prediction-" + _predictionName.ToString() + DateTime.UtcNow.Millisecond + "." + file.FileName.Substring(index + 1);
            string _dictionaryPath = Path.Combine(_webHostEnvironment.WebRootPath + "/Images/Predictions/");
            string _filePath = Path.Combine(_dictionaryPath, _file_name);
            using (var stream = new FileStream(_filePath, FileMode.Create))
            {
                file.CopyTo(stream);
            }
            String RelativePath = _filePath.Replace(_webHostEnvironment.WebRootPath, String.Empty);
            return RelativePath;
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

            HttpResponseMessage response = await client.GetAsync(PredictionApiUrl + "/GetQueueOfPrediction/" + expertId);
            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var listPredictions = JsonSerializer.Deserialize<IEnumerable<Prediction>>(strData, options);
            return View(listPredictions);
        }

        [HttpPost]
        [Authorize("EXPERT")]
        public async Task<IActionResult> HistoryExpert(string expertConfirmation, long predictionId)
        {
            if (!TokenAdded())
            {
                return RedirectToAction("Login", "Authentication");
            }

            var expertId = session.GetString("id");
            HttpResponseMessage response = await client
                .GetAsync(PredictionApiUrl + "/ExpertConfirmation/predictionId=" + predictionId +"?confirm=" + expertConfirmation);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("HistoryExpert");
            }
            else
            {
                return RedirectToAction("HistoryExpert");

            }
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

        [Authorize("MEMBER")]
        private async Task<Guid> GetRandomExpert()
        {
            HttpResponseMessage response = await client.GetAsync(UserApiUrl);
            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<User> listUsers = JsonSerializer.Deserialize<List<User>>(strData, options);
            var expert = listUsers.Where(role => role.RoleId.Equals(3)).ToList();
            try
            {
                var randomExpert = expert[new Random().Next(0, expert.Count)];
                return randomExpert.Id;
            }
            catch (Exception ex)
            {
                throw new Exception("error: ", ex);
            }

        }
    }
}
