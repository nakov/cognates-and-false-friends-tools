using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Provides functionality for performing automated queries in Google
    /// for finding the number of occurences for given word and language.
    /// 
    /// </summary>
    public class GoogleOccurencesUtils
    {
        private const int MAX_ATTEMPTS = 15;
        private const int MIN_DELAY_MS = 500;
        private const int MAX_DELAY_MS = 5000;

        private const int MAX_THREADS_COUNT = 10;
        private static Queue<WordAndLang> queriesQueue;
        private static Thread[] workingThreads;

        public static long GetOccurences(string word, string langCode)
        {
            if (word.Contains(" "))
            {
                word = "\"" + word + "\"";
            }

            // Check the cache first
            string query = word + " lang:" + langCode;
            if (GoogleOccurencesCache.IsInCache(query))
            {
                long occurences = GoogleOccurencesCache.GetFromCache(query);
                return occurences;
            }

            Console.WriteLine("Looking Google occurences count of {0} ({1})", word, langCode);

            for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
            {
                try
                {
                    long occurences = TryToGetOccurences(word, langCode);
                    GoogleOccurencesCache.AddToCache(query, occurences);
                    return occurences;
                }
                catch (Exception)
                {
                    // Google search failed. Wait random time and try again
                    Console.WriteLine(
                        "Google search failed at attempt {0} of {1}.",
                        attempt + 1, MAX_ATTEMPTS);

                    Random rand = new Random();
                    int randomTime = rand.Next(MIN_DELAY_MS, MAX_DELAY_MS);
                    Thread.Sleep(randomTime);
                }
            }
            throw new Exception(String.Format("After {0} attempts Google failed to " +
                "return the number of occurences for the word {1}.", MAX_ATTEMPTS, word));
        }

        public static long AsyncCalcOccurences(string word, string langCode)
        {
            if (queriesQueue == null)
            {
                // Create the queries queue and start the working threads
                queriesQueue = new Queue<WordAndLang>();
                workingThreads = new Thread[MAX_THREADS_COUNT];
                for (int i = 0; i < MAX_THREADS_COUNT; i++)
                {
                    workingThreads[i] = new Thread(new ThreadStart(ProcessQueriesQueue));
                    workingThreads[i].Name = "Google.Query.Thread#" + i;
                    workingThreads[i].Start();
                }
            }

            if (word.Contains(" "))
            {
                word = "\"" + word + "\"";
            }

            // Check the cache first
            string query = word + " lang:" + langCode;
            if (GoogleOccurencesCache.IsInCache(query))
            {
                long occurences = GoogleOccurencesCache.GetFromCache(query);
                return occurences;
            }

            // Add the query to a queue for asynchronous processing
            lock (queriesQueue)
            {
                WordAndLang wordAndLang = new WordAndLang(word, langCode);
                queriesQueue.Enqueue(wordAndLang);
                Monitor.Pulse(queriesQueue);
            }

            return 0;
        }

        private static void ProcessQueriesQueue()
        {
            while (true)
            {
                WordAndLang query = null;
                lock (queriesQueue)
                {
                    if (queriesQueue.Count > 0)
                    {
                        query = queriesQueue.Dequeue();
                    }
                    else
                    {
                        Monitor.Wait(queriesQueue);
                    }
                }
                if (query != null)
                {
                    Console.WriteLine("Thread {0} - job {1} of {2}",
                        Thread.CurrentThread.Name, queriesQueue.Count, queriesQueue.Count);
                    GetOccurences(query.Word, query.Lang);
                }
            }
        }

        private static long TryToGetOccurences(string word, string langCode)
        {
            string wordURLEncoded = System.Web.HttpUtility.UrlEncode(word);
            string googleSearchUrl = String.Format(
                "http://www.google.bg/search?as_q={0}&lr=lang_{1}&num=100",
                wordURLEncoded,
                langCode);
            string googleSearchResults = HttpUtils.DownloadUrl(googleSearchUrl);
            if (googleSearchResults.IndexOf("/images/yellow_warning.gif") != -1)
            {
                // No result found in the requested language.
                // Google shows results in all languages
                return 0;
            }

            Match match = Regex.Match(googleSearchResults,
                " от (около )?<b>(?<count>.*?)</b> ");
            if (match.Success)
            {
                string occurencesStr = match.Groups["count"].Value;
                occurencesStr = occurencesStr.Replace(",", "");
                occurencesStr = occurencesStr.Replace("&nbsp;", "");
                long occurences = long.Parse(occurencesStr);
                return occurences;
            }
            else if (googleSearchResults.IndexOf("не бяха открити съответстващи документи") != -1)
            {
                return 0;
            }
            else
            {
                throw new Exception("Page returned by Google is in invalid format.");
            }
        }
    }
}