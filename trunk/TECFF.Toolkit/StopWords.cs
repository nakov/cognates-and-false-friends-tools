using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Manages a repository of stop words (prepositions, pronouns, conjunctions, etc.)
    /// for multiple languages.
    /// </summary>
    public class StopWords
    {
        private const string STOP_WORDS_FILE_PREFIX = @"..\..\..\Resources\Stop-Words-";
        private const string STOP_WORDS_SUFFIX = @".csv";
        private static readonly Encoding FILE_ENCODING =
            Encoding.GetEncoding("windows-1251");

        private static Dictionary<string, Dictionary<string, bool>> stopWordsHT =
            new Dictionary<string, Dictionary<string, bool>>();

        private StopWords()
        {
        }

        private static Dictionary<string, bool> ReadStopWordsFile(string fileName)
        {
            Dictionary<string, bool> stopWords = new Dictionary<string, bool>();
            StreamReader reader = new StreamReader(fileName, FILE_ENCODING);
            using (reader)
            {
                // Skip the header line
                reader.ReadLine();

                // Process all lines to the end of the file
                while (true)
                {
                    string line = reader.ReadLine();
                    if ((line == null) || (line == ""))
                    {
                        // End of file reached
                        break;
                    }
                    string[] tokens = line.Split(';');
                    //string word = String.Intern(tokens[0].Trim());
                    string word = tokens[0].Trim();
                    stopWords[word] = true;
                }
            }
            return stopWords;
        }

        public static bool IsStopWord(string word, string langCode)
        {
            if (!stopWordsHT.ContainsKey(langCode))
            {
                string fileName = STOP_WORDS_FILE_PREFIX + langCode + STOP_WORDS_SUFFIX;
                Dictionary<string, bool> stopWords = ReadStopWordsFile(fileName);
                stopWordsHT[langCode] = stopWords;
            }

            Dictionary<string, bool> langStopWords = stopWordsHT[langCode];
            bool isStopWord = langStopWords.ContainsKey(word);
            return isStopWord;
        }
    }
}
