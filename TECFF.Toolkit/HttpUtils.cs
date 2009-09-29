using System;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Threading;

namespace TECFF.Toolkit
{
    /// <summary>
    /// Provides utilities for sending HTTP requests and parsing the HTTP response.
    /// </summary>
    public class HttpUtils
    {
        private const int SOCKET_RECEIVE_TIMEOUT = 5 * 1000;
        private const int RECEIVE_BUF_SIZE = 65536;
        private const int MAX_ATTEMPTS = 15;
        private const int MIN_REGULAR_DELAY_MS = 0;
        private const int MAX_REGULAR_DELAY_MS = 0;
        private const int MIN_DELAY_ON_ERROR_MS = 1000;
        private const int MAX_DELAY_ON_ERROR_MS = 2000;

        private static bool SEND_COOKIE = true;
        private static string cookie = "PREF=ID=f5ec8110c31f45fc:TM=1244466883:LM=1244466883:S=3JFJNLkGRSc6R2GM; S=sorry=SSxR3eI2zyD_pkTGj36VvQ; GDSESS=ID=f5ec8110c31f45fc:EX=1244477697:S=C9B-xKqw2DxaO9BV; NID=23=F5JLzwNdQzVPgENv6bCrXzykF_aaCZcq_0mchV9d2xBrIu0lqWUWk7ZTjPnCroWoGhT_gwBDRPyU5Icpj4AsjKmxHE9w5q4_2w2Olss02KqSYam8c0SURpk2KIaMjKRE";

        private const string ACCEPT_HEADER = "image/gif, image/x-xbitmap, image/jpeg, image/pjpeg, application/x-shockwave-flash, application/vnd.ms-excel, application/vnd.ms-powerpoint, application/msword, application/xaml+xml, application/vnd.ms-xpsdocument, application/x-ms-xbap, application/x-ms-application, application/x-silverlight, */*";
        private const string USER_AGENT_HEADER = "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.2; SV1; .NET CLR 1.1.4322; InfoPath.1; .NET CLR 2.0.50727; .NET CLR 3.0.04506.30; .NET CLR 3.0.04506.648; .NET CLR 3.5.21022)";
        private const string CHARSET_HEADER = "utf-8";

        private static Random rand = new Random();

        public static string DownloadUrl(string url)
        {
            // Delay to avoid the "Search engine abuse protection (image captcha)"
            int randomSleepTime = rand.Next(MIN_REGULAR_DELAY_MS, MAX_REGULAR_DELAY_MS);
            Thread.Sleep(randomSleepTime);

            Uri uri = new Uri(url);
            string host = uri.Host;
            int port = uri.Port;
            String response = DownloadUrl(url, host, port);
            return response;
        }

        public static string DownloadUrl(string url, string host, int port)
        {
            TcpClient tcpClient = new TcpClient();
            tcpClient.Connect(host, port);
            tcpClient.ReceiveTimeout = SOCKET_RECEIVE_TIMEOUT;
            using (tcpClient)
            {
                NetworkStream ns = tcpClient.GetStream();
                using (ns)
                {
                    StreamWriter writer = new StreamWriter(ns);
                    string httpRequest = String.Format("GET {0} HTTP/1.0\n", url);
                    writer.Write(httpRequest);
                    string acceptHeader = String.Format("Accept: {0}\n", ACCEPT_HEADER);
                    writer.Write(acceptHeader);
                    string userAgentHeader = String.Format("User-Agent: {0}\n", USER_AGENT_HEADER);
                    writer.Write(userAgentHeader);
                    string charsetHeader = String.Format("Accept-Charset: {0}\n", CHARSET_HEADER);
                    writer.Write(charsetHeader);

                    if (SEND_COOKIE)
                    {
                        string cookieHeader = String.Format("Cookie: {0}\n", cookie);
                        writer.Write(cookieHeader);
                    }
                    writer.Write("\n");
                    writer.Flush();

                    byte[] buf = new byte[RECEIVE_BUF_SIZE];
                    MemoryStream responseAllBytes = new MemoryStream();
                    while (true)
                    {
                        int bytesRead = ns.Read(buf, 0, buf.Length);
                        if (bytesRead == 0)
                        {
                            // Server closed the connection
                            break;
                        }
                        responseAllBytes.Write(buf, 0, bytesRead);
                    }
                    string responseStr = Encoding.UTF8.GetString(responseAllBytes.ToArray());
                    return responseStr;
                }
            }
        }

        public static string DownloadUrlWithRetry(string url)
        {
            // Attempt to perform search engine query for MAX_ATTEMPTS times
            for (int attempt = 0; attempt < MAX_ATTEMPTS; attempt++)
            {
                try
                {
                    string urlContents = DownloadUrl(url);
                    return urlContents;
                }
                catch (Exception)
                {
                    // Search failed. Wait random time and try again
                    Console.WriteLine(
                        "Search failed at attempt {0} of {1}.", attempt + 1, MAX_ATTEMPTS);

                    int randomTime = rand.Next(MIN_DELAY_ON_ERROR_MS, MAX_DELAY_ON_ERROR_MS);
                    Thread.Sleep(randomTime);
                }
            }
            throw new Exception(
                String.Format("After {0} attempts search failed to get the URL {1}.",
                MAX_ATTEMPTS, url));
        }

        public static string Cookie
        {
            get { return cookie; }
            set { cookie = value; }
        }
    }
}