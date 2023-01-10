using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UserService.Data;
using UserService.DTOs;
using UserService.Interfaces;
using UserService.Models;
using static UserService.Services.EmailService;

namespace UserService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUserServices _services;
        private readonly IEmailService _emailService;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IServer _server;

        public UsersController(DataContext context, 
            IUserServices services, 
            IEmailService emailService,
            IWebHostEnvironment webHostEnvironment,
            IServer server)
        {
            _context = context;
            _services = services;
            _emailService = emailService;
            _webHostEnvironment = webHostEnvironment;
            _server = server;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDTO userLoginDTO)
        {
            var user = await _context.Users.Include(u => u.Role).FirstOrDefaultAsync(u => u.Email == userLoginDTO.Email);
            if (user == null)
            {
                return BadRequest("User Not Found");
            }

            if (!VerifyPasswordHash(userLoginDTO.Password, user.passwordHash, user.passwordSalt))
            {
                return BadRequest("Password is incorrect!");
            }

            if (user.verifiedAt == null)
            {
                return BadRequest("Not verified!");
            }

            return Ok(new AuthorizedUserDTO(user));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDTO userRegisterDTO)
        {
            string memberRole = "MEMBER";
            if (_context.Users.Any(u => u.Email == userRegisterDTO.Email))
            {
                return BadRequest("User Already Exist!");
            }
            var password = userRegisterDTO.Password;
            var confirmPassword = userRegisterDTO.ConfirmPassword;
            if (password != confirmPassword)
            {
                return BadRequest("Password must match!");
            }
            if (password.Length < 6)
            {
                return BadRequest("Please enter at least 6 characters");
            }


            _services.CreatePasswordHash(userRegisterDTO.Password,
                 out byte[] passwordHash,
                 out byte[] passwordSalt);

            var user = new User
            {
                Email = userRegisterDTO.Email,
                Address = userRegisterDTO.Address,
                passwordHash = passwordHash,
                passwordSalt = passwordSalt,
                verificationToken = _services.CreateRandomToken(userRegisterDTO.Email.ToString(), memberRole.ToString()),
                RoleId = 2
            };

            #region Add Email Template
            try
            {
                var builder = new BodyBuilder();
                //Giao thuc IO Truyen file
                using (StreamReader SourceReader = System.IO.File.OpenText($"{_webHostEnvironment.WebRootPath}Templates/VerifyAccountTemplate.html"))
                {
                    builder.HtmlBody = SourceReader.ReadToEnd();
                }

                // replace chữ trong indexs
                string htmlBody = builder.HtmlBody.Replace("Welcome!", $"Welcome {user.Email}!")
                .Replace("#Token", user.verificationToken)
                .Replace("#memberEmail", user.Email);
                string messagebody = string.Format("{0}", htmlBody);

                #region Send Verification Mail To User
                try
                {
                    var mailContent1 = new MailContent();
                    mailContent1.To = "wall-nguyen@mailinator.com"; //temp email
                    mailContent1.Subject = "Welcome To SSD!";
                    mailContent1.Body = messagebody;
                    await _emailService.SendMail(mailContent1);
                }
                catch (System.ArgumentNullException)
                {
                    return BadRequest("Some error occur during sending email, please wait a seconds and register again!");
                }
                catch (Exception)
                {
                    return BadRequest("Some error occur during sending email, please wait a seconds and register again!");
                }
                #endregion
            }
            catch (Exception e)
            {
                return BadRequest("Some error occur during sending email, please wait a seconds and register again!");
            }

            #endregion

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("User successfully created!");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return BadRequest("User Not Found");
            }
            var memberRole = "MEMBER";
            user.passwordResetToken = _services.CreateRandomToken(email, memberRole);
            user.resetTokenExpires = DateTime.Now.AddDays(1);
            await _context.SaveChangesAsync();

            #region Add Email Template
            try
            {
                var builder = new BodyBuilder();
                //Giao thuc IO Truyen file
                using (StreamReader SourceReader = System.IO.File.OpenText($"{_webHostEnvironment.WebRootPath}Templates/ResetPasswordTemplate.html"))
                {
                    builder.HtmlBody = SourceReader.ReadToEnd();
                }

                // replace chữ trong indexs
                string htmlBody = builder.HtmlBody.Replace("Welcome!", $"Welcome {user.Email}!")
                .Replace("We're excited to have you get started. First, you need to confirm your account. Just press the button below.", "RESET PASSWORD REQUEST!")
                .Replace("Confirm Account", "Reset Password")
                .Replace("#Token", user.passwordResetToken);
                string messagebody = string.Format("{0}", htmlBody);

                #region Send Verification Mail To User
                try
                {
                    var mailContent1 = new MailContent();
                    mailContent1.To = "wall-nguyen@mailinator.com"; //temp email
                    mailContent1.Subject = "Reset Password!";
                    mailContent1.Body = messagebody;
                    await _emailService.SendMail(mailContent1);
                }
                catch (System.ArgumentNullException)
                {
                    return BadRequest("Some error occur during sending email, please wait a seconds and register again!");
                }
                catch (Exception)
                {
                    return BadRequest("Some error occur during sending email, please wait a seconds and register again!");
                }
                #endregion
            }
            catch (Exception e)
            {
                return BadRequest("Some error occur during sending email, please wait a seconds and register again!");
            }

            #endregion


            return Ok(user.passwordResetToken);
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDTO resetPasswordDTO)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.passwordResetToken == resetPasswordDTO.Token);
            if (user == null || user.resetTokenExpires < DateTime.Now)
            {
                return BadRequest("Invalid Token.");
            }
            var password = resetPasswordDTO.Password;
            var confirmPassword = resetPasswordDTO.ConfirmPassword;
            if (password != confirmPassword)
            {
                return BadRequest("Password must match!");
            }
            if (password.Length < 6)
            {
                return BadRequest("Please enter at least 6 characters");
            }
            _services.CreatePasswordHash(resetPasswordDTO.Password, out byte[] passwordHash, out byte[] passwordSalt);

            user.passwordHash = passwordHash;
            user.passwordSalt = passwordSalt;
            user.passwordResetToken = null;
            user.resetTokenExpires = null;

            await _context.SaveChangesAsync();

            return Ok("Password successfully reset.");
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.verificationToken == token);
            if (user == null)
            {
                return BadRequest("User Not Found");
            }

            user.verifiedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            return Ok($"User {user.Email} verified at {user.verifiedAt}!");
        }

        [HttpGet]
        [Authorize(Roles = "MEMBER")]
        public ActionResult<User> GetUsers()
        {
            return Ok(_context.Users.ToList());
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512(passwordSalt))
            {
                var computeHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return computeHash.SequenceEqual(passwordHash);
            }

        }

    }
}
