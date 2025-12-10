using System.Threading.Tasks;

namespace TalentoPlus.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string emailDestino, string asunto, string mensaje);
    }
}