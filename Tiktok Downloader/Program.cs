using System;
using System.Threading.Tasks;
using System.Drawing;
using Console = Colorful.Console;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net;
using System.ComponentModel;
using System.Threading;

namespace Tiktok_Downloader
{
    internal class Program
    {
        private static HttpClientHandler httpClientHandler = new HttpClientHandler()
        {
            AllowAutoRedirect = false,
            CookieContainer = new System.Net.CookieContainer(),
            UseCookies = false
        };
        private static HttpClient httpClient = new HttpClient() 
        {
            Timeout = TimeSpan.FromSeconds(10),
        };
        private static List<string> Urls = new List<string>();
        static async Task Main(string[] args)
        {
            Console.Title = "Tiktok Downloader";

            Console.WriteLine();

            Console.WriteLineFormatted("\tTik{0} Downloader", Color.FromArgb(99, 191, 197), Color.FromArgb(226, 28, 78), "tok");

            Console.WriteLine();

            Urls = File.ReadAllLines("Urls.txt").ToList();

            Urls.ForEach(async x => 
            {
                var urlwithout = Regex.Match(x, "https://.*?/(.*?)/").Groups[1].Value;

                var response = await GetInfo(x);

                if (response.success)
                {
                    Console.WriteLineFormatted("[+] {0} | {1}", Color.FromArgb(99, 191, 197), Color.White, urlwithout, "obtaining video metadata...");

                    await DownloadVideo($"https://tikmate.app/download/{response.token}/{response.id}.mp4?hd=1", response);
                }
            });

            await Task.Delay(-1);
        }
        private static async Task<Information> GetInfo(string url) 
        {
            while (true) 
            {
                (string state, bool success) Status = ("", false);

                var urlwithout = Regex.Match(url, "https://.*?/(.*?)/").Groups[1].Value;

                try
                {
                    using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.tikmate.app/api/lookup")
                    {
                        Content = new FormUrlEncodedContent(new Dictionary<string, string>() 
                        {
                            { "url", url}
                        })
                    };

                    httpRequest.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36 Edg/105.0.1343.53");

                    using var response = await httpClient.SendAsync(httpRequest);

                    var responseContent = await response.Content.ReadAsStringAsync();

                    if (responseContent.Contains("Video is not available or in privacy."))
                    {
                        Status.state = "NotFound";
                        Status.success = false;
                        return new Information() { success = false };
                    }
                    else if (responseContent.Contains("token"))
                    {
                        Status.state = "Ok";
                        Status.success = true;
                        return JsonSerializer.Deserialize<Information>(responseContent);
                    }
                    else
                    {
                        Status.state = "forbidden";
                        Status.success = false;
                        continue;
                    }
                }
                catch
                {
                    Status.state = "Error";
                    Status.success = false;
                    continue;
                }
                finally 
                {
                    if (Status.state == "forbidden")
                    {
                        Console.WriteLineFormatted("[!] {0} | Error {1}, retrying", Color.FromArgb(226, 28, 78), Color.White, urlwithout, "403 Forbidden");
                    }
                    else if (Status.state == "NotFound")
                    {
                        Console.WriteLineFormatted("[!] {0} | Error {1}, skipping", Color.FromArgb(226, 28, 78), Color.White, urlwithout, "404 Not found");
                    }
                    else if (Status.state == "Error") 
                    {
                        Console.WriteLineFormatted("[!] {0} | Error {1}, retrying", Color.FromArgb(226, 28, 78), Color.White, urlwithout, "Unkown");
                    }
                    else
                    {
                        Console.WriteLineFormatted("[+] {0} | {1}", Color.FromArgb(99, 191, 197), Color.White, urlwithout, "Information obtained successfully!");
                    }
                }
            }
        }
        private static async Task DownloadVideo(string Url, Information info) 
        {
            using var webClient = new WebClient();

            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadFileCompleted);
            //webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressCallback);

            try
            {
                 await webClient.DownloadFileTaskAsync(new Uri(Url), $"{AppDomain.CurrentDomain.BaseDirectory}\\Downloads\\{info.author_id}_{info.id}.mp4");
            }
            catch
            {
                return;
            }
        }
        //private static void DownloadProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        //{
        //    Console.Clear();

        //    Console.SetCursorPosition(0, 1);

        //    Console.WriteLineFormatted("\tTik{0} Downloader", Color.FromArgb(99, 191, 197), Color.FromArgb(226, 28, 78), "tok");

        //    Console.SetCursorPosition(0, 3);

        //    Console.WriteLineFormatted("[+] {0}% Complete | ({1}\\{2})", Color.FromArgb(99, 191, 197), Color.White, e.ProgressPercentage, e.BytesReceived, e.TotalBytesToReceive);
        //}
        private static void DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Console.WriteLineFormatted("[+] {0}", Color.FromArgb(99, 191, 197), Color.White, "Download finished!");
        }
    }
}
