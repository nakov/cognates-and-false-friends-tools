using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Extracts local contexts for given word and language from MSN (Live) Search.
    /// Processes the first 1000 results for the given word and language.
    /// </summary>
    public class MSNContextUtils
    {
        public static void RetrieveMSNContextSentences(string word, string langCode,
            out List<string> titleSentences, out List<string> textSentences)
        {
            if (MSNContextCache.IsInCache(word, langCode))
            {
                MSNContextCache.LoadFromCache(word, langCode,
                    out titleSentences, out textSentences);
                Console.WriteLine("Loaded cached MSN context: {0} ({1})", word, langCode);
            }
            else
            {
                Console.WriteLine("Finding MSN context: {0} ({1})", word, langCode);

                // Get the MSN context as raw HTML string
                string msnContext = RetrieveMSNContextAsString(word, langCode);

                // Process search results titles
                titleSentences = new List<string>();
                Match match = Regex.Match(msnContext,
                    @"<li[^>]*?>\s*<h3>(.*?)</h3>",
                    RegexOptions.Singleline);
                while (match.Success)
                {
                    string title = match.Groups[1].Value;
                    string titleDecoded = HttpUtility.HtmlDecode(title);
                    string titleWithoutTags = HtmlParsingUtils.RemoveHtmlTags(titleDecoded);
                    titleSentences.Add(titleWithoutTags);
                    match = match.NextMatch();
                }

                // Process search results body text
                textSentences = new List<string>();
                match = Regex.Match(msnContext,
                    @"</h3>\s*<p>(.*?)</p>",
                    RegexOptions.Singleline);
                while (match.Success)
                {
                    string text = match.Groups[1].Value;
                    string textDecoded = HttpUtility.HtmlDecode(text);
                    string textWithoutTags = HtmlParsingUtils.RemoveHtmlTags(textDecoded);
                    textSentences.Add(textWithoutTags);
                    match = match.NextMatch();
                }

                MSNContextCache.AddToCache(word, langCode, titleSentences, textSentences);
            }
        }

        private static string RetrieveMSNContextAsString(string word, string langCode)
        {
            // Prepare the search URL
            string wordURLEncoded = System.Web.HttpUtility.UrlEncode(word);
            string msnBaseSearchUrl = String.Format(
                "http://search.live.com/results.aspx?q={0}+language:{1}&count=100",
                wordURLEncoded, langCode);

            // Search in MSN and append the results by portions of 100 at a time
            StringBuilder allMSNResults = new StringBuilder();
            int requestedStartResult = 1;
            while (true)
            {
                string msnSearchUrl = msnBaseSearchUrl + "&first=" + requestedStartResult;
                string msnSearchResults = HttpUtils.DownloadUrlWithRetry(msnSearchUrl);
                bool searchSuccessfull = msnSearchResults.StartsWith("HTTP/1.0 200");
                if (searchSuccessfull)
                {
                    if (msnSearchResults.IndexOf("<div id=\"no_results\">") != -1)
                    {
                        // No results are found by the query
                        break;
                    }
                    Match startResultMatch = Regex.Match(msnSearchResults,
                        "<span id=\"count\">(\\d+)");
                    if (!startResultMatch.Success)
                    {
                        throw new Exception("Can not find start result number identifier " +
                            "in the query results returned by MSN");
                    }
                    int returnedStartResult = Int32.Parse(startResultMatch.Groups[1].Value);
                    if (returnedStartResult != requestedStartResult)
                    {
                        // No more results (the requested start result is not found)
                        break;
                    }
                    allMSNResults.Append(msnSearchResults);
                    requestedStartResult += 100;
                }
                else
                {
                    throw new Exception("MSN returned unexpected response on " +
                        "the query: " + msnSearchUrl);
                }
            }

            string allResults = allMSNResults.ToString();
            string allResultsLowerCase = allResults.ToLowerInvariant();

            return allResultsLowerCase;
        }

    }
}
