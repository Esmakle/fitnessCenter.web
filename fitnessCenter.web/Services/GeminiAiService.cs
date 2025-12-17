using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace fitnessCenter.web.Services
{
    public sealed class GeminiAiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string ModelName = "gemini-2.5-flash"; // Google'ın önerdiği ve en hızlı/uygun model

        public GeminiAiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            // appsettings.json dosyasından "Gemini:ApiKey" değerini okur.
            _apiKey = configuration["Gemini:ApiKey"]
                   ?? throw new InvalidOperationException("Gemini:ApiKey bulunamadı.");

            // Base URL ayarlanabilir, ama burada tam URL ile istek atacağız.
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{ModelName}:generateContent";

            // Sistem talimatını prompt içine ekleyerek Gemini'ye gönderiyoruz.
            var fullPrompt = $@"Sen bir fitness koçusun. Kullanıcının hedefine yönelik kısa, net ve uygulanabilir bir plan/öneri hazırla.
Kullanıcı İsteği: {prompt}";

            var requestBody = new
            {
                contents = new[]
       {
        new { parts = new[] { new { text = fullPrompt } } }
    },
                // Generation ayarlarını doğru alana ekleyin
                generationConfig = new
                {
                    temperature = 0.5
                }
                // İpucu: Bu ayarı hiç göndermezseniz de (sadece contents bırakırsanız) çalışır.
            };

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestBody),
                Encoding.UTF8,
                "application/json"
            );

            try
            {
                // API Anahtarını URL parametresi ile gönderiyoruz.
                var response = await _httpClient.PostAsync($"{endpoint}?key={_apiKey}", jsonContent);

                var resultJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<GeminiResponse>(resultJson);
                    // Cevabı JSON yapısından alıp döndürüyoruz.
                    return result?.Candidates?[0]?.Content?.Parts?[0]?.Text ?? "Cevap boş döndü.";
                }
                else
                {
                    // Hata durumunda detaylı hata mesajı döndürülür.
                    return $"AI HATA: HTTP {(int)response.StatusCode}. Detay: {resultJson}";
                }
            }
            catch (Exception ex)
            {
                return "Bağlantı hatası: " + ex.Message;
            }
        }
    }

    // API Yanıtını İşlemek İçin Gerekli JSON Sınıfları
    // Bunları GeminiAiService.cs içine ekleyebilirsiniz.
    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<Candidate>? Candidates { get; set; }
    }
    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }
    }
    public class Content
    {
        [JsonPropertyName("parts")]
        public List<Part>? Parts { get; set; }
    }
    public class Part
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}