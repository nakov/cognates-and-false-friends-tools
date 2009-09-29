using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Extracts local contexts for given word and language from Google. Processes the
    /// titles and text snippets from the first 1000 results for given word and language.
    /// </summary>
    public class GoogleContextUtils
    {
        public static void RetrieveGoogleContextSentences(string word, string langCode,
            out List<string> titleSentences, out List<string> textSentences)
        {
            if (GoogleContextCache.IsInCache(word, langCode))
            {
                GoogleContextCache.LoadFromCache(word, langCode,
                    out titleSentences, out textSentences);
                Console.WriteLine("Loaded cached Google context: {0} ({1})", word, langCode);
            }
            else
            {
                Console.WriteLine("Finding Google context: {0} ({1})", word, langCode);

                // Get the Google context as RAW string
                string googleContext = RetrieveGoogleContextAsString(word, langCode);

                // Process search results titles
                titleSentences = new List<string>();
                Match match = Regex.Match(googleContext,
                    @"class=l[^>]*?>(.*?)</a>",
                    RegexOptions.Singleline);
                while (match.Success)
                {
                    string title = match.Groups[1].Value;
                    string titleDecoded = HttpUtility.HtmlDecode(title);
                    string titleWithoutTags = HtmlParsingUtils.RemoveHtmlTags(titleDecoded);
                    titleSentences.Add(titleWithoutTags);
                    match = match.NextMatch();
                }

                // Process search results body texts
                textSentences = new List<string>();

                match = Regex.Match(googleContext,
                    @"<div class=""s"">(<span class=f>.*?</a>)?(.*?)<br><cite>",
                    RegexOptions.Singleline);

                // If we have PDF/DOC/PPT file --> skip it's contents is in <span class=f> ... </a>

                while (match.Success)
                {
                    string text = match.Groups[2].Value;
                    string textDecoded = HttpUtility.HtmlDecode(text);
                    string textWithoutTags = HtmlParsingUtils.RemoveHtmlTags(textDecoded);
                    textSentences.Add(textWithoutTags);
                    match = match.NextMatch();
                }

                GoogleContextCache.AddToCache(word, langCode, titleSentences, textSentences);
            }
        }

        private static string RetrieveGoogleContextAsString(string word, string langCode)
        {
            // Add quotes if the word is phrase (contains spaces)
            if (word.Contains(" "))
            {
                word = "\"" + word + "\"";
            }

            // Prepare the search URL
            string wordURLEncoded = System.Web.HttpUtility.UrlEncode(word);
            string googleBaseSearchUrl = String.Format(
                "http://www.google.com/search?as_q={0}&lr=lang_{1}&ie=utf-8&num=100",
                wordURLEncoded, langCode);

            // Search in Google and append the results by portions of 100 at a time
            StringBuilder allGoogleResults = new StringBuilder();
            string googleSearchUrl = googleBaseSearchUrl;
            int page = 0;
            while (true)
            {
                string googleSearchResults = HttpUtils.DownloadUrlWithRetry(googleSearchUrl);
                bool searchSuccessfull = googleSearchResults.StartsWith("HTTP/1.0 200");
                if (searchSuccessfull)
                {
                    allGoogleResults.Append(googleSearchResults);
                    int indexNextPage = googleSearchResults.IndexOf("</span>Next</a>");
                    if (indexNextPage == -1)
                    {
                        // No next page
                        break;
                    }
                    page++;
                    googleSearchUrl = googleBaseSearchUrl + "&start=" + (page * 100);
                }
                else
                {
                    Console.Write("Google captcha protection. Please enter a cookie: ");
                    string cookie = Console.ReadLine();
                    if (cookie != "")
                    {
                        HttpUtils.Cookie = cookie;
                    }

                    // Search failed. Google returned unexpected HTTP response
                    //if (googleSearchResults.IndexOf(
                    //    "http://sorry.google.com") != -1)
                    //{
                    //    throw new Exception("Google displayed the automated request " +
                    //        "protection (captcha with image). Please use a cookie.");
                    //}
                    //else
                    //{
                    //    throw new Exception("Google returned unexpected response on " +
                    //        "the query: " + googleSearchUrl);
                    //}
                }
            }

            string allResults = allGoogleResults.ToString();
            string allResultsLowerCase = allResults.ToLowerInvariant();

            return allResultsLowerCase;
        }
    }
}
