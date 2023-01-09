using System.Threading.Tasks;
using static UserService.Services.EmailService;

namespace UserService.Interfaces
{
    public interface IEmailService
    {
        Task SendMail(MailContent mailContent);
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}