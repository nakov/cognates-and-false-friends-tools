using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Caches in the file system all extracted titles and text snippets
    /// extracted from MSN (Live Search) for given {word, language} pair.
    /// </summary>
    class MSNContextCache
    {
        private const string MSN_CONTEXT_CACHE_DIR = @"..\..\..\MSN-Context-Cache";
        private const string CACHE_FILE_EXTENSION = ".txt";
        private const string SENTENCE_TYPE_TITLE = "title";
        private const string SENTENCE_TYPE_TEXT = "text";

        private static readonly Encoding FILE_ENCODING = Encoding.UTF8;

        public static void AddToCache(string word, string langCode,
            List<string> titleSentences, List<string> textSentences)
        {
            string cacheFileName = GetCacheFileName(word, langCode);
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
                    string sentence = tokens[1];
                    if (sentenceType == SENTENCE_TYPE_TEXT)
                    {
                        textSentences.Add(sentence);
                    }
                    else if (sentenceType == SENTENCE_TYPE_TITLE)
                    {
                        titleSentences.Add(sentence);
                    }
                    else
                    {
                        throw new Exception(
                            "MSN context cache: Invalid sentence: " + sentence);
                    }
                }
            }
        }

        private static string GetCacheFileName(string word, string langCode)
        {
            string fileName = MSN_CONTEXT_CACHE_DIR + "\\" + langCode + "\\" +
                Cyr2LatUtils.CyrillicToLatin(word) + CACHE_FILE_EXTENSION;
            return fileName;
        }

    }
}