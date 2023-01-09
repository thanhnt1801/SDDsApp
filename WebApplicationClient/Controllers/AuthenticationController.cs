using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGeneration.Contracts.Messaging;
using Newtonsoft.Json;
using NToastNotify;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WebApplicationClient.DTOs;

namespace WebApplicationClient.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly HttpClient _client;
        private readonly string AuthenticationApiUrl = "";
        private readonly string UserApiUrl = "";
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IToastNotification _toastNotification;

        public AuthenticationController(IHttpContextAccessor httpContextAccessor,
            IToastNotification toastNotification)
        {
            _client = new HttpClient();
            var contentType = new MediaTypeWithQualityHeaderValue("application/json");
            _client.DefaultRequestHeaders.Accept.Add(contentType);
            AuthenticationApiUrl = "https://localhost:44318/api/Users/login";
/*            AuthenticationApiUrl = "https://localhost:44344/apigateway/UserService/Users/login";
*/            UserApiUrl = "https://localhost:44318/api/Users/";
            /*            UserApiUrl = "https://localhost:44344/apigateway/UserService/Users/";
            */
            _httpContextAccessor = httpContextAccessor;
            _toastNotification = toastNotification;
        }
        public ISession session { get { return _httpContextAccessor.HttpContext.Session; } }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (!string.IsNullOrWhiteSpace(session.GetString("jwtToken")))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDTO userLoginDTO)
        {
            if (ModelState.IsValid)
            {
                string content = System.Text.Json.JsonSerializer.Serialize(userLoginDTO);
                var data = new StringContent(content, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{AuthenticationApiUrl}", data);
                string strData = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    var message = await response.Content.ReadAsStringAsync();
                    ViewData["message"] = message.Substring(1, message.Length - 2);
                    _toastNotification.AddErrorToastMessage("Your Email or Password is wrong!!");
                    return RedirectToAction("Login", "Authentication");
                }
                else
                {
                    var authorizedUser = JsonConvert.DeserializeObject<AuthorizedUserDTO>(strData);

                    if (authorizedUser != null)
                    {
                        session.SetString("jwtToken", authorizedUser.Token);
                        session.SetString("email", authorizedUser.Email);
                        session.SetString("id", authorizedUser.Id.ToString());
                        session.SetString("role", authorizedUser.AuthorizeRole);
                        _toastNotification.AddSuccessToastMessage("Login Success");
                        return RedirectToAction("Index", "Home");
                    }
                }
            }

            _toastNotification.AddErrorToastMessage("Your Email or Password is wrong!!");
            return View("Login", userLoginDTO);

        }

        [HttpGet]
        public IActionResult Logout()
        {
            session.Clear();
            return RedirectToAction("Login", "Authentication");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (!string.IsNullOrWhiteSpace(session.GetString("jwtToken")))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterDTO registerDTO)
        {
            if (ModelState.IsValid)
            {
                string content = System.Text.Json.JsonSerializer.Serialize(registerDTO);
                var data = new StringContent(content, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{UserApiUrl}Register", data);
                if (!response.IsSuccessStatusCode)
                {
                    var message = await response.Content.ReadAsStringAsync();
                    ViewData["message"] = message;
                }   
                else
                {
                    _toastNotification.AddSuccessToastMessage("Register success, please login!!");
                    return RedirectToAction("VerifyAccount", "Authentication", new {registerDTO.Email});
                }
            }
            return View("Register", registerDTO);
        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> ForgotPassword(string email)
        {
            if (ModelState.IsValid)
            {
                string content = System.Text.Json.JsonSerializer.Serialize(email);
                var data = new StringContent(content, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{UserApiUrl}forgot-password?email={email}", data);
                if (!response.IsSuccessStatusCode)
                {

                    var message = await response.Content.ReadAsStringAsync();
                    _toastNotification.AddErrorToastMessage(message);
                    ViewData["message"] = message.Substring(1, message.Length - 2);
                }
                else
                {
                    string strData = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                    };
                    string TokenToResetPassword = System.Text.Json.JsonSerializer.Deserialize<string>(strData, options);
                    var message = "Please visit your mail to reset your password!";
                    _toastNotification.AddSuccessToastMessage(message);
                    ViewData["message"] = message.Substring(1, message.Length - 2);
                    return RedirectToAction("Login", "Authentication", new { TokenToResetPassword });
                }

            }
            return View();
        }

        public ActionResult VerifyAccount(string email, string token)
        {
            var verifyAccountDTO = new VerifyAccountDTO()
            {
                Email = email,
                Token = token
            };
            return View(verifyAccountDTO);
        }

        [HttpPost]
        public async Task<ActionResult> VerifyAccount(VerifyAccountDTO verifyAccountDTO)
        {
            var token = verifyAccountDTO.Token;
            if (ModelState.IsValid)
            {
                string content = System.Text.Json.JsonSerializer.Serialize(token);
                var data = new StringContent(content, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{UserApiUrl}verify?token={token}", data);
                if (!response.IsSuccessStatusCode)
                {
                    return BadRequest("Something is wrong when trying to send request!");
                }
                else
                {
                    return RedirectToAction("Login", "Authentication");
                }
            }
            return View(verifyAccountDTO);
        }

        public ActionResult ResetPassword(string TokenToResetPassword)
        {
            var resetPasswordDTO = new ResetPasswordDTO()
            {
                Token = TokenToResetPassword
            };
            return View(resetPasswordDTO);
        }

        [HttpPost]
        public async Task<ActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {

            if (ModelState.IsValid)
            {
                string content = System.Text.Json.JsonSerializer.Serialize(resetPasswordDTO);
                var data = new StringContent(content, Encoding.UTF8, "application/json");
                var response = await _client.PostAsync($"{UserApiUrl}reset-password", data);
                if (!response.IsSuccessStatusCode)
                {

                    var message = await response.Content.ReadAsStringAsync();
                    _toastNotification.AddErrorToastMessage(message);
                    ViewData["message"] = message.Substring(1, message.Length - 2);
                }
                else
                {
                    return RedirectToAction("Login", "Authentication");
                }

            }
            return View(resetPasswordDTO);
        }
    }
}
