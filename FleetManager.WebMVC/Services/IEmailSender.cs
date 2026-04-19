using System.Threading.Tasks;

namespace FleetManager.WebMVC.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}
