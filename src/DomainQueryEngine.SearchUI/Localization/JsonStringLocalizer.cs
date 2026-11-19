using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Localization;

namespace DomainQueryEngine.SearchUI.Localization
{
    /// <summary>
    /// Texts from flat key/value JSON files embedded under Resources/Locales ("tr.json", "en.json").
    /// All views share one set of keys, whatever type they ask for (<c>IStringLocalizer&lt;SharedResource&gt;</c>).
    /// </summary>
    public sealed class JsonStringLocalizerFactory : IStringLocalizerFactory
    {
        /// <summary>Language used when a key is missing in the requested one.</summary>
        public const string FallbackLanguage = "tr";

        private readonly ConcurrentDictionary<string, IReadOnlyDictionary<string, string>> _languages = new(StringComparer.OrdinalIgnoreCase);
        private readonly ILogger<JsonStringLocalizerFactory> _logger;
        private readonly JsonStringLocalizer _localizer;

        public JsonStringLocalizerFactory(ILogger<JsonStringLocalizerFactory> logger)
        {
            _logger = logger;
            _localizer = new JsonStringLocalizer(this);
        }

        public IStringLocalizer Create(Type resourceSource) => _localizer;

        public IStringLocalizer Create(string baseName, string location) => _localizer;

        /// <summary>The texts of one language ("tr", "en"); empty when there is no file for it.</summary>
        public IReadOnlyDictionary<string, string> Texts(string language)
            => _languages.GetOrAdd(language, Load);

        /// <summary>Languages that have a file, e.g. ["en", "tr"].</summary>
        public static IReadOnlyList<string> AvailableLanguages()
            => typeof(JsonStringLocalizerFactory).Assembly.GetManifestResourceNames()
                .Where(n => n.StartsWith("Locales.", StringComparison.Ordinal) && n.EndsWith(".json", StringComparison.Ordinal))
                .Select(n => n["Locales.".Length..^".json".Length])
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToList();

        private IReadOnlyDictionary<string, string> Load(string language)
        {
            using var stream = typeof(JsonStringLocalizerFactory).Assembly.GetManifestResourceStream($"Locales.{language}.json");
            if (stream == null)
                return new Dictionary<string, string>();

            return JsonSerializer.Deserialize<Dictionary<string, string>>(stream)
                   ?? new Dictionary<string, string>();
        }

        internal void ReportMissing(string key, string language)
            => _logger.LogWarning("Missing text '{Key}' for language '{Language}'.", key, language);
    }

    public sealed class JsonStringLocalizer : IStringLocalizer
    {
        private readonly JsonStringLocalizerFactory _factory;

        public JsonStringLocalizer(JsonStringLocalizerFactory factory)
        {
            _factory = factory;
        }

        public LocalizedString this[string name]
        {
            get
            {
                var value = Find(name);
                return new LocalizedString(name, value ?? name, resourceNotFound: value == null);
            }
        }

        /// <summary>
        /// Formats the text with the arguments. When the first argument is the number 1 and a
        /// "{name}.One" key exists, that singular form is used ("1 search" rather than "1 searches").
        /// </summary>
        public LocalizedString this[string name, params object[] arguments]
        {
            get
            {
                var value = (arguments.Length > 0 && arguments[0] is 1 ? FindOwn(name + ".One") : null) ?? Find(name);
                var text = value == null ? name : string.Format(CultureInfo.CurrentCulture, value, arguments);
                return new LocalizedString(name, text, resourceNotFound: value == null);
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures)
            => _factory.Texts(Language).Select(t => new LocalizedString(t.Key, t.Value));

        private static string Language => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        /// <summary>The text in the current language only, without fallback or a missing-key warning.</summary>
        private string? FindOwn(string name)
            => _factory.Texts(Language).TryGetValue(name, out var value) ? value : null;

        private string? Find(string name)
        {
            var language = Language;
            if (_factory.Texts(language).TryGetValue(name, out var value))
                return value;

            _factory.ReportMissing(name, language);
            return language != JsonStringLocalizerFactory.FallbackLanguage
                   && _factory.Texts(JsonStringLocalizerFactory.FallbackLanguage).TryGetValue(name, out var fallback)
                ? fallback
                : null;
        }
    }
}
