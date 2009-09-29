using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Utility class for extraction of words from given sentence.
    /// </summary>
    public class WordExtractor
    {
        private WordExtractor()
        {
        }

        public static List<string> ExtractAndNormalizeWords(string sentence)
        {
            List<string> wordsList = new List<string>();
            string[] words = Regex.Split(sentence, @"[^à-ÿÀ-ß¸¨]+");
            foreach (string word in words)
            {
                if (word != "")
                {
                    string normalizedWord = NormalizeWord(word);
                    wordsList.Add(normalizedWord);
                }
            }
            return wordsList;
        }

        private static string NormalizeWord(string word)
        {
            string normalizedWord = word.ToLowerInvariant();
            return normalizedWord;
        }
    }
}
