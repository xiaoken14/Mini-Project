using System.Threading.Tasks;

namespace Mini_Project.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string email, string subject, string message);
    }
}
