using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Caches in the file system the information about the occurences
    /// of given word (or phrase) and laguage in Google.
    /// </summary>
    class GoogleOccurencesCache
    {
        const string GOOGLE_CACHE_FILE = @"..\..\..\Google-Cache\Google-Occurences-Cache.csv";
        const int INITIAL_CACHE_CAPACITY = 500000;
        const int FILE_READ_BUFFER_SIZE = 16 * 1024 * 1024;
        private static readonly Encoding FILE_ENCODING =
            Encoding.GetEncoding("windows-1251");

        private static Dictionary<string, long> cachedQueries =
            new Dictionary<string, long>(INITIAL_CACHE_CAPACITY);

        static GoogleOccurencesCache()
        {
            lock (cachedQueries)
            {
                try
                {
                    Console.WriteLine("Loading file: " + GOOGLE_CACHE_FILE);
                    StreamReader reader = new StreamReader(
                        GOOGLE_CACHE_FILE, FILE_ENCODING, false, FILE_READ_BUFFER_SIZE);
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
                            if (tokens.Length != 2)
                            {
                                throw new FormatException("The input file was not in the expected format!");
                            }
                            string query = tokens[0];
                            string countStr = tokens[1];
                            long count = long.Parse(countStr);
                            cachedQueries[query] = count;
                        }
                    }
                }
                catch (FileNotFoundException)
                {
                    // The cache file does not exists. Create it right now
                    StreamWriter writer = new StreamWriter(GOOGLE_CACHE_FILE, false,
                        FILE_ENCODING);
                    using (writer)
                    {
                        writer.WriteLine("Query;Count");
                    }
                    return;
                }
            }
        }

        public static void AddToCache(string query, long count)
        {
            lock (cachedQueries)
            {
                cachedQueries.Add(query, count);
                StreamWriter writer = new StreamWriter(
                    GOOGLE_CACHE_FILE, true, FILE_ENCODING);
                using (writer)
                {
                    writer.WriteLine("{0};{1}", query, count);
                }
            }
        }

        public static bool IsInCache(string query)
        {
            lock (cachedQueries)
            {
                bool isInCache = cachedQueries.ContainsKey(query);
                return isInCache;
            }
        }

        public static long GetFromCache(string query)
        {
            lock (cachedQueries)
            {
                long count = cachedQueries[query];
                return count;
            }
        }
    }
}
