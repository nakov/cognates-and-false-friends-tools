using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Class for caching in the file system the reverse contexts of given
    /// word (instances of the class WordsAndCounts) .
    /// </summary>
    class ReversedContextCache
    {
        private const string REVERSED_CONTEXT_CACHE_DIR = @".\Reversed-Context-Words-Cache";
        private const string CACHE_FILE_EXTENSION = ".ser";

        static ReversedContextCache()
        {
            if (!Directory.Exists(REVERSED_CONTEXT_CACHE_DIR))
            {
                Directory.CreateDirectory(REVERSED_CONTEXT_CACHE_DIR);
            }
        }

        public static void AddToCache(string word, string langCode,
            WordsAndCounts contextWords)
        {
            string cacheFileName = GetCacheFileName(word, langCode);
            FileStream output = new FileStream(cacheFileName, FileMode.Create);
            using (output)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(output, contextWords);
            }
        }

        public static bool IsInCache(string word, string langCode)
        {
            string cacheFileName = GetCacheFileName(word, langCode);
            bool isInCache = File.Exists(cacheFileName);
            return isInCache;
        }

        public static WordsAndCounts LoadFromCache(string word, string langCode)
        {
            string cacheFileName = GetCacheFileName(word, langCode);
            FileStream input = new FileStream(cacheFileName, FileMode.Open);
            using (input)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                WordsAndCounts contextWords = (WordsAndCounts)formatter.Deserialize(input);
                return contextWords;
            }
        }

        private static string GetCacheFileName(string word, string langCode)
        {
            string fileName = REVERSED_CONTEXT_CACHE_DIR + "\\" + langCode + "-" +
                Cyr2LatUtils.CyrillicToLatin(word) + CACHE_FILE_EXTENSION;
            return fileName;
        }
    }
}
