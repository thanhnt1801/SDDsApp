using System.Net.Mail;
using WebApplicationClient.Models;

namespace WebApplicationClient.DTOs
{
    public class AuthorizedUserDTO : User
    {
        public string Token { get; set; }

        public AuthorizedUserDTO(User user)
        {
            Id = user.Id;
            Email = user.Email;
            AuthorizeRole = user.Role.Name;
            Token = user.verificationToken;
        }

        public AuthorizedUserDTO()
        {

        }
        public string AuthorizeRole { get; set; }
    }
}
