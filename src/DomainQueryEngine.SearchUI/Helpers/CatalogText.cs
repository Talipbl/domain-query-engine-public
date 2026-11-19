using DomainQueryEngine.Core.Catalog;

namespace DomainQueryEngine.SearchUI.Helpers
{
    /// <summary>
    /// Picks catalog texts (descriptions, FAQ) in the request's language; falls back to Turkish when the
    /// catalog has no English text.
    /// </summary>
    public static class CatalogText
    {
        private static bool IsEnglish => LanguagePreference.Current == "en";

        public static string? Description(TldEntry entry)
            => IsEnglish && !string.IsNullOrEmpty(entry.DescriptionEn) ? entry.DescriptionEn : entry.Description;

        public static string Question(FaqEntry faq)
            => IsEnglish && !string.IsNullOrEmpty(faq.QuestionEn) ? faq.QuestionEn : faq.Question;

        public static string Answer(FaqEntry faq)
            => IsEnglish && !string.IsNullOrEmpty(faq.AnswerEn) ? faq.AnswerEn : faq.Answer;
    }
}
