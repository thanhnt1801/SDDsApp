using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplicationClient.DTOs;

namespace WebApplicationClient.Controllers
{
    public class FastAPIController : Controller
    {
        private readonly HttpClient client = null;
        public FastAPIController()
        {
            client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            client.DefaultRequestHeaders.Accept.Add(contentType);
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> PostPredictImage(IFormFile file)
        {
            try
            {
                // Read the file and convert it to a byte array
                byte[] imageData = null;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    imageData = memoryStream.ToArray();
                }

                // Create a new HTTP client and set the base address to the FastAPI endpoint
                var FastAPIUrl = "http://127.0.0.1:8000/predict";

                // Create a new multipart form content and add the image file
                var content = new MultipartFormDataContent();
                content.Add(new ByteArrayContent(imageData), "file", file.FileName);

                // Send the HTTP request to the FastAPI endpoint
                var response = await client.PostAsync(FastAPIUrl, content);

                // Read the response content and return the results
                var responseContent = await response.Content.ReadAsStringAsync();
                return Content(responseContent, "application/json");
            }
            catch (Exception e)
            {
                return Content($"{{\"error\": \"{e.Message}\"}}", "application/json");
            }

        }

    }
}
