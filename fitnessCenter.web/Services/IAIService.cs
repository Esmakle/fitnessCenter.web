using System.Threading.Tasks;

namespace fitnessCenter.web.Services
{
    // Arayüz aynı kalır, sadece ismini küçük 'i' ile IAiService.cs olarak düzelttim.
    public interface IAiService
    {
        // Tüm AI servislerinin uygulaması gereken temel metot.
        Task<string> GenerateAsync(string prompt);
    }
}