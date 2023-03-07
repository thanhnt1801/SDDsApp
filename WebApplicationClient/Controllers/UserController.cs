using eBookStore.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;
using NToastNotify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
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
        private readonly IToastNotification _toastNotification;

        public UserController(IHttpContextAccessor httpContextAccessor, IToastNotification toastNotification)
        {
            _client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            _client.DefaultRequestHeaders.Accept.Add(contentType);
            UserApiUrl = "https://localhost:44318/api/UserManagement";

            /*UserApiUrl = "https://localhost:44344/apigateway/UserService/UserManagement";*/
            _httpContextAccessor = httpContextAccessor;
            _toastNotification = toastNotification;
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
            var distinctRole = listUsers.Select(user => user.Role).Distinct(new DistinctRoleComparer());
            ViewBag.Roles = new SelectList(distinctRole, "Id", "Name");
            return View(listUsers);
        }

        [HttpPost]
        public async Task<IActionResult> ChangeRole(int roleId, string userId)
        {
            HttpResponseMessage response = await _client
                .GetAsync("https://localhost:44318/api/UserManagement/EditRoleUserAsync/userId=" + userId + "?roleId=" + roleId);
            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("Index");

            }
        }

        public async Task<ActionResult> Delete(Guid id)
        {
            var model = new User();
            HttpResponseMessage responseUser = await _client.GetAsync(UserApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            if (responseUser.IsSuccessStatusCode)
            {
                string userData = await responseUser.Content.ReadAsStringAsync();
                model = JsonSerializer.Deserialize<User>(userData, options);
            }

            return View(model);
        }



        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(Guid id)
        {
            HttpResponseMessage getUser = await _client.GetAsync(UserApiUrl + "/" + id);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };

            string strData = await getUser.Content.ReadAsStringAsync();

            var user = JsonSerializer.Deserialize<User>(strData, options);

            HttpResponseMessage response = await _client.DeleteAsync(UserApiUrl + "/" + user.Id);

            if (response.IsSuccessStatusCode)
            {
                _toastNotification.AddSuccessToastMessage("Disable User Success!");
                return RedirectToAction("Index");
            }

            return View();
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
                Address = (string)temp["address"],
                PhoneNumber = (long)temp["phoneNumber"],
                FullName = (string)temp["fullName"],
                DateOfBirth = (DateTime)temp["dateOfBirth"],
                Age = (int)temp["getAge"]
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
                Address = (string)temp["address"],
                PhoneNumber = (long)temp["phoneNumber"],
                FirstName = (string)temp["firstName"],
                LastName = (string)temp["lastName"],
                DateOfBirth = (DateTime)temp["dateOfBirth"],
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
                RoleId = 2,
                UpdatedAt = DateTime.Now,
                LastName = user.LastName,
                FirstName = user.FirstName,
                PhoneNumber = user.PhoneNumber,
                DateOfBirth = user.DateOfBirth,
            };
            var now = DateTime.Now;
            var age = now.Year - userEdit.DateOfBirth.Value.Year;
            if(userEdit.DateOfBirth > now || age < 12)
            {
                _toastNotification.AddErrorToastMessage("Invalid Date Of Birth, you must be over 12");
                return View(user);
            }
            var userToEdit = JsonSerializer.Serialize(userEdit);
            HttpContent content = new StringContent(userToEdit, Encoding.UTF8, "application/json");
            var response = await _client.PutAsync(UserApiUrl + "/" + userEdit.Id, content);
            if (!response.IsSuccessStatusCode)
            {
                return View(user);
            }
            _toastNotification.AddSuccessToastMessage("Update Profile Successfully!");
            return RedirectToAction("UserProfile", "User", new { Id = user.Id});
        }
    }
}
