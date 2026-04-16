using System.Text.Json;
using MelodyBanger.Models;

namespace MelodyBanger.Services
{
    public class LocalizationService
    {
        private readonly Dictionary<string, LocalizationData> _cache = new();
        private readonly string _localizationPath;
        private const string DefaultLang = "en";

        public LocalizationService(IWebHostEnvironment env)
        {
            _localizationPath = Path.Combine(env.ContentRootPath, "Localization");
        }
        public async Task<LocalizationData> GetLocalization(string lang)
        {
            if (_cache.TryGetValue(lang, out var cached))
                return cached;

            var data = await LoadLocalizationFromFileAsync(lang)
                ?? await LoadLocalizationFromFileAsync(DefaultLang)
                ?? throw new InvalidOperationException("Localization data is missing");

            _cache[lang] = data;
            return data;
        }
        private async Task<LocalizationData?> LoadLocalizationFromFileAsync(string lang)
        {
            var filePath = Path.Combine(_localizationPath, $"{lang}.json");
            if (!File.Exists(filePath)) return null;

            var json = await File.ReadAllTextAsync(filePath);
            return JsonSerializer.Deserialize<LocalizationData>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
    }
}
