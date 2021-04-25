using System;
using System.IO;
using System.Linq;
using System.Net;
using HtmlAgilityPack;

namespace ScrapeIt
{
    internal class Program
    {
        private static string localFolder = "C:\\Temp\\WebContent\\";

        private static void Main(string[] args)
        {
            Console.WriteLine("Enter option:");
            Console.WriteLine("1: Download from a URL");
            Console.WriteLine("2: Download all archives from Weebly blog");
            var option = Console.ReadLine();

            if (option == "1")
            {
                DownloadFromUrl();
            }
            else if (option == "2")
            {
                DownloadArchivesFromWeebly();
            }
        }

        /// <summary>
        /// Downloads the content for a given blog page on a Weebly site
        /// </summary>
        private static void DownloadFromUrl()
        {
            var webClient = new WebClient();
            string url;
            do
            {
                Console.WriteLine("Enter URL to save:");
                url = Console.ReadLine();

                if (!string.IsNullOrWhiteSpace(url))
                {
                    Console.WriteLine("Enter File Name:");
                    var fileName = Console.ReadLine();
                    var content = webClient.DownloadString(url);

                    content = DownloadImagesInContent(content, url);

                    SaveTextToFile(content, fileName);
                }
            } while (!string.IsNullOrWhiteSpace(url));
        }

        /// <summary>
        /// Download all content from the blog archives pages for a Weebly site
        /// </summary>
        private static void DownloadArchivesFromWeebly()
        {
            Console.WriteLine("Enter Weebly Archive URL (e.g. http://yoursite.weebly.com/blog/archives/):");
            var weeblyArchiveUrl = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(weeblyArchiveUrl)) return;

            if (!weeblyArchiveUrl.EndsWith("/"))
            {
                weeblyArchiveUrl += "/";
            }

            Console.WriteLine("Enter Start Year:");
            var startYear = Console.ReadLine();
            int start;
            int.TryParse(startYear, out start);

            Console.WriteLine("Enter End Year:");
            var endYear = Console.ReadLine();
            int end;
            int.TryParse(endYear, out end);

            if (start > end)
            {
                start = end;
            }

            var webClient = new WebClient();

            for (int year = start; year <= end; year++)
            {
                for (int month = 1; month <= 12; month++)
                {
                    var formattedMonth = month.ToString("D2");
                    var fileName = $"{year}-{formattedMonth}";
                    Console.WriteLine(fileName);
                    string url = $"{weeblyArchiveUrl}{formattedMonth}-{year}";
                    var content = webClient.DownloadString(url);

                    content = DownloadImagesInContent(content, url);
                    SaveTextToFile(content, fileName);
                }
            }
        }

        /// <summary>
        /// Creates an HTML file for the given content, using the given name
        /// Folder name is C:\Temp\WebContent, so make sure you have that folder created
        /// </summary>
        /// <param name="content">The contents of the file to save</param>
        /// <param name="fileName">The name of the file (without an extension)</param>
        private static void SaveTextToFile(string content, string fileName)
        {
            if (string.IsNullOrWhiteSpace(content)) return;

            FileStream ostrm;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;
            try
            {
                var path = $"{localFolder}{fileName}.html";

                ostrm = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);
                writer = new StreamWriter(ostrm);
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot open file for writing");
                Console.WriteLine(e.Message);
                return;
            }
            Console.SetOut(writer);
            Console.WriteLine(CleanContent(content));
            Console.SetOut(oldOut);
            writer.Close();
            ostrm.Close();
        }

        /// <summary>
        /// Weebly blog content - look for the "blogTable".
        /// Then remove content above this element and after the closing table tag
        /// </summary>
        /// <param name="content">Weebly blog page content</param>
        /// <returns>Just the blog entries for the page</returns>
        private static string CleanContent(string content)
        {
            var start = "id=\"blogTable\"";

            var startIndex = content.IndexOf(start);
            if (startIndex > 0)
            {
                content = $"<table {content.Substring(startIndex)}";

                startIndex = content.IndexOf("</table>");
                if (startIndex > 0)
                {
                    content = content.Substring(0, startIndex + 8);
                }
            }

            return content;
        }

        /// <summary>
        /// Find all IMG tags in the content and attempt to download and save the images
        /// </summary>
        /// <param name="content">Blog page content being downloaded</param>
        /// <param name="siteUrl">The URL of the website</param>
        private static string DownloadImagesInContent(string content, string siteUrl)
        {
            if (string.IsNullOrWhiteSpace(content)) return "";

            // Images - download and store them
            // Based folder:    http://sitename.weebly.com/
            // Image:           uploads/1/6/0/5/16055074/mcrib1_orig.jpg
            // Full path:       http://sitename.weebly.com/uploads/1/6/0/5/16055074/mcrib1_orig.jpg

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(content);
            document.DocumentNode.Descendants("img")
                .Where(e =>
                {
                    string src = e.GetAttributeValue("src", null) ?? "";
                    return !string.IsNullOrEmpty(src);
                })
                .ToList()
                .ForEach(x =>
                {
                    string currentSrcValue = x.GetAttributeValue("src", null);
                    Console.WriteLine(currentSrcValue);

                    // Remove all of the subfolders (/) and replace with dashes
                    var updatedSrcValue = currentSrcValue.Replace("/uploads/", "").Replace("/", "-");
                    DownloadImage(currentSrcValue, updatedSrcValue, siteUrl);

                    // Update the link in the HTML - want to keep the ref to uploads, but remove the initial /
                    var updatedSrcLink = currentSrcValue.Replace("/uploads/", "^^^").Replace("/", "-");
                    updatedSrcLink = updatedSrcLink.Replace("^^^", "uploads/");
                    content = content.Replace(currentSrcValue, updatedSrcLink);
                });

            return content;
        }

        private static void DownloadImage(string imageUrl, string fileName, string siteUrl)
        {
            if (string.IsNullOrWhiteSpace(imageUrl)) return;

            var imagesFolder = $"{localFolder}/uploads";
            if (!Directory.Exists(imagesFolder))
            {
                Directory.CreateDirectory(imagesFolder);
            }

            var uri = new Uri(siteUrl);
            string host = $"{uri.Scheme}://{uri.Host}";

            var fullImageUrl = host + imageUrl;
            Console.WriteLine(fullImageUrl);

            var webClient = new WebClient();
            webClient.DownloadFileAsync(new Uri(fullImageUrl), $"{imagesFolder}/{fileName}");
        }
    }
}