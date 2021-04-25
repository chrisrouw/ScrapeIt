using System;
using System.IO;
using System.Net;

namespace ScrapeIt
{
    internal class Program
    {
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

                    SaveTextToFile(content, fileName);
                }
            } while (!string.IsNullOrWhiteSpace(url));
        }

        // Images - can we download and store them?
        // Based folder:    http://tyannsheldonrouw.weebly.com/
        // Image:           uploads/1/6/0/5/16055074/mcrib1_orig.jpg
        // Full path:       http://tyannsheldonrouw.weebly.com/uploads/1/6/0/5/16055074/mcrib1_orig.jpg

        /// <summary>
        /// Download all content from the archives pages for a Weebly site
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
                    SaveTextToFile(content, fileName);
                }
            }
        }

        private static void SaveTextToFile(string content, string fileName)
        {
            if (string.IsNullOrWhiteSpace(content)) return;

            FileStream ostrm;
            StreamWriter writer;
            TextWriter oldOut = Console.Out;
            try
            {
                //var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), $"./{fileName}.html");
                var path = $"C:\\Temp\\WebContent\\{fileName}.html";

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
    }
}