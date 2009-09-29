using System.Text;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Caches binary vectors used by SemSim and CrossSim algorithms.
    /// </summary>
    public class VectorsCache
    {
        private const string VECTORS_CONTEXT_CACHE_DIR = @".\Vectors-Cache";
        private const string CACHE_FILE_EXTENSION = ".bin";

        private static readonly Encoding FILE_ENCODING = Encoding.UTF8;

        static VectorsCache()
        {
            if (!Directory.Exists(VECTORS_CONTEXT_CACHE_DIR))
            {
                Directory.CreateDirectory(VECTORS_CONTEXT_CACHE_DIR);
            }
        }

        public static void AddToCache(string word, string langCode, double[] vector)
        {
            string cacheFileName = GetCacheFileName(word, langCode);
            FileStream output = new FileStream(cacheFileName, FileMode.Create);
            using (output)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(output, vector);
            }
        }

        public static bool IsInCache(string word, string langCode)
        {
            string cacheFileName = GetCacheFileName(word, langCode);
            bool isInCache = File.Exists(cacheFileName);
            return isInCache;
        }

        public static double[] LoadFromCache(string word, string langCode)
        {
            string cacheFileName = GetCacheFileName(word, langCode);
            FileStream input = new FileStream(cacheFileName, FileMode.Open);
            using (input)
            {
                BinaryFormatter formatter = new BinaryFormatter();
                double[] vector = (double[])formatter.Deserialize(input);
                return vector;
            }
        }

        private static string GetCacheFileName(string word, string langCode)
        {
            string fileName = VECTORS_CONTEXT_CACHE_DIR + "\\" + langCode + "-" +
                Cyr2LatUtils.CyrillicToLatin(word) + CACHE_FILE_EXTENSION;
            return fileName;
        }

    }
}
