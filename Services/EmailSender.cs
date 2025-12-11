using System.Threading.Tasks;
using System;

namespace Mini_Project.Services
{
    public class EmailSender : IEmailSender
    {
        public EmailSender(string host, int port, string username, string password)
        {
            // This constructor is now a placeholder and doesn't need to do anything.
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            Console.WriteLine("--- New Email ---");
            Console.WriteLine($"To: {email}");
            Console.WriteLine($"Subject: {subject}");
            Console.WriteLine($"Body: {message}");
            Console.WriteLine("-----------------");
            return Task.CompletedTask;
        }
    }
}
