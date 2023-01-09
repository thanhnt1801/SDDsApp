using eBookStore.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplicationClient.DTOs;
using WebApplicationClient.Models;

namespace WebApplicationClient.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly HttpClient _client;
        private readonly string UserApiUrl = "";
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserController(IHttpContextAccessor httpContextAccessor)
        {
            _client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            _client.DefaultRequestHeaders.Accept.Add(contentType);
            UserApiUrl = "https://localhost:44318/api/UserManagement";

            /*UserApiUrl = "https://localhost:44344/apigateway/UserService/UserManagement";*/
            _httpContextAccessor = httpContextAccessor;
        }

        public ISession session { get { return _httpContextAccessor.HttpContext.Session; } }

        [Authorize("ADMIN")]
        public async Task<IActionResult> Index()
        {
            HttpResponseMessage response = await _client.GetAsync(UserApiUrl);
            string strData = await response.Content.ReadAsStringAsync();

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            List<User> listUsers = JsonSerializer.Deserialize<List<User>>(strData, options);
            return View(listUsers);
        }

        private bool TokenAdded()
        {
            string token = session.GetString("jwtToken");

            if (string.IsNullOrEmpty(token))
            {
                return false;
            }

            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);
            return true;
        }

        private bool IsCurrentUser(Guid id)
        {
            string currentUserId = session.GetString("id");
            if (currentUserId != id.ToString()) return false;
            return true;
        }

        [Authorize("MEMBER")]
        public async Task<IActionResult> UserProfile(Guid id)
        {
            if (!TokenAdded())
            {
                return View("Unauthorized");
            }

            if (!IsCurrentUser(id)) return BadRequest();

            var response = await _client.GetAsync($"{UserApiUrl}/({id})");
            string strData = await response.Content.ReadAsStringAsync();

            var temp = JObject.Parse(strData);

            if (temp == null)
            {
                return View("NotFound");
            }

            var user = new User()
            {
                Id =id,
                Email = (string)temp["email"],
                Address = (string)temp["address"]
            };

            if (user == null)
            {
                return NotFound();
            }

            return View(user);

        }

        [Authorize("MEMBER")]
        public async Task<IActionResult> UserEdit(Guid id)
        {
            if (!TokenAdded())
            {
                return View("Unauthorized");
            }

            if (!IsCurrentUser(id)) return BadRequest();

            var response = await _client.GetAsync($"{UserApiUrl}/({id})");
            string strData = await response.Content.ReadAsStringAsync();

            var temp = JObject.Parse(strData);

            if (temp == null)
            {
                return View("NotFound");
            }

            var user = new User()
            {
                Email = (string)temp["email"],
                Address = (string)temp["address"]
            };

            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [Authorize("MEMBER")]
        public async Task<IActionResult> UserEdit(string? id, User user)
        {
            if (!IsCurrentUser(user.Id)) return BadRequest();

            var userEdit = new User()   
            {
                Id = Guid.Parse(id),
                Email = user.Email,
                Address = user.Address,
                RoleId = 2
            };

            var userToEdit = JsonSerializer.Serialize(userEdit);
            HttpContent content = new StringContent(userToEdit, Encoding.UTF8, "application/json");
            var response = await _client.PutAsync(UserApiUrl + "/" + id, content);

            if(!response.IsSuccessStatusCode)
            {
                return BadRequest("Something is wrong when trying to Edit User");
            }

            return RedirectToAction("UserProfile", "User", new { Id = user.Id});
        }
    }
}
