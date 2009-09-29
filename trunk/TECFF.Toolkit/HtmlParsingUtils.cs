using System.Text.RegularExpressions;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Utility class for removing HTML tags.
    /// </summary>
    class HtmlParsingUtils
    {
        public static string RemoveHtmlTags(string text)
        {
            string textWithoutTags = Regex.Replace(
                text, @"<.*?>", " ", RegexOptions.Singleline);
            return textWithoutTags;
        }
    }
}
