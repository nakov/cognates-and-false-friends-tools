using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Caches in the file system all extracted titles and text snippets
    /// extracted from Google for given {word, language} pair.
    /// </summary>
    class GoogleContextCache
    {
        private const string GOOGLE_CONTEXT_CACHE_DIR = @"..\..\..\Google-Cache";
        private const string CACHE_FILE_EXTENSION = ".txt";
        private const string SENTENCE_TYPE_TITLE = "title";
        private const string SENTENCE_TYPE_TEXT = "text";

        private static readonly Encoding FILE_ENCODING = Encoding.UTF8;

        public static void AddToCache(string word, string langCode,
            List<string> titleSentences, List<string> textSentences)
        {
            string cacheFileName = GetCacheFileName(word, langCode);
            string cacheDir = (new FileInfo(cacheFileName)).DirectoryName;
            if (!File.Exists(cacheDir))
            {
                Directory.CreateDirectory(cacheDir);
            }

            StreamWriter writer = new StreamWriter(cacheFileName, true, FILE_ENCODING);
            using (writer)
            {
                foreach (string titleSentence in titleSentences)
                {
                    writer.WriteLine(SENTENCE_TYPE_TITLE + "\t" + titleSentence);
                }
                foreach (string textSentence in textSentences)
                {
                    writer.WriteLine(SENTENCE_TYPE_TEXT + "\t" + textSentence);
                }
            }
        }

        public static bool IsInCache(string word, string langCode)
        {
            string cacheFileName = GetCacheFileName(word, langCode);
            bool isInCache = File.Exists(cacheFileName);
            return isInCache;
        }

        public static void LoadFromCache(string word, string langCode,
            out List<string> titleSentences, out List<string> textSentences)
        {
            string cacheFileName = GetCacheFileName(word, langCode);
            StreamReader reader = new StreamReader(cacheFileName, FILE_ENCODING);
            using (reader)
            {
                titleSentences = new List<string>();
                textSentences = new List<string>();
                while (true)
                {
                    string line = reader.ReadLine();
                    if ((line == null) || (line == ""))
                    {
                        break;
                    }
                    string[] tokens = line.Split('\t');
                    string sentenceType = tokens[0];
                    if (sentenceType == SENTENCE_TYPE_TEXT)
                    {
                        string sentence = tokens[1];
                        textSentences.Add(sentence);
                    }
                    else if (sentenceType == SENTENCE_TYPE_TITLE)
                    {
                        string sentence = tokens[1];
                        titleSentences.Add(sentence);
                    }
                    else
                    {
                        // Just ignore the sentence
                        //throw new Exception(
                        //    "Google context cache: Invalid sentence: " + sentence);
                    }
                }
            }
        }

        private static string GetCacheFileName(string word, string langCode)
        {
            string fileName = GOOGLE_CONTEXT_CACHE_DIR + "\\" + langCode + "\\" +
                Cyr2LatUtils.CyrillicToLatin(word) + CACHE_FILE_EXTENSION;
            return fileName;
        }
    }
}
