using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace subtitle_tools
{
    class Program
    {
        const char SystemSplitter = '\\'; // Windows: \ , Unix: /
        const string SL = "pt"; // Source language
        const string TL = "en"; // Target language
        const string Source = @"D:\w\subtitle-tools\source"; // Source subtitle folder
        const string Target = @"D:\w\subtitle-tools\dest"; // Destination subtitle folder
        static readonly string GoogleTranslation = $"https://translate.google.com/translate_a/single?client=gtx&sl={SL}&tl={TL}&dt=t&q={{0}}";
        static readonly WebClient WebClient = new WebClient();

        static void Main(string[] args)
        {
            foreach (var file in Directory.GetFiles(Source))
            {
                //translateFile(file);
                timeOffsetFile(file, 3, 400);
            }
        }

        private static void timeOffsetFile(string file, int second, int millisecond)
        {
            string filename = file.Substring(file.LastIndexOf(SystemSplitter) + 1);
            using (var reader = new StreamReader(file))
            {
                using (var writer = new StreamWriter(Path.Combine(Target, filename)))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (int.TryParse(line, out int test))
                        {
                            writer.WriteLine(line);
                            /*
                            if (test <= 10)
                            {
                                continue;
                            }
                            */
                            Console.WriteLine($"Processing {file} - {line}...");
                        }
                        else if (line.IndexOf("-->") > -1)
                        {
                            var times = line.Split(" --> ", StringSplitOptions.None);
                            writer.WriteLine($"{offset(times[0])} --> {offset(times[1])}");
                        }
                        else
                        {
                            writer.WriteLine(line);
                        }
                    }
                }
            }

            // Local functions
            string offset(string timespan)
            {
                var ost = new TimeSpan(0, 0, 0, second, millisecond);
                var p = timespan.Split(':', ',').Select(s => int.Parse(s)).ToArray();
                var ot = new TimeSpan(0, p[0], p[1], p[2], p[3]);
                var nt = ot.Add(ost);
                var nts = nt.ToString().Replace('.', ',');
                return nts.Substring(0, nts.Length - 4);
            }
        }

        private static void translateFile(string file)
        {
            string filename = file.Substring(file.LastIndexOf(SystemSplitter) + 1);

            using (var reader = new StreamReader(file))
            {
                using (var writer = new StreamWriter(Path.Combine(Target, filename)))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();
                        if (int.TryParse(line, out int test))
                        {
                            writer.WriteLine(line);
                            Console.WriteLine($"Processing {file} - {line}...");
                        }
                        else if (line.IndexOf("-->") > -1)
                        {
                            writer.WriteLine(line);
                        }
                        else if (line.Equals(string.Empty))
                        {
                            writer.WriteLine(line);
                        }
                        else
                        {
                            writer.WriteLine(translateText(line));
                        }
                    }
                }
            }

            // Local functions
            string cleanText(string text)
            {
                return Regex.Replace(Regex.Replace(text, @"<[^>]*>", String.Empty), @"{[^}]*}", String.Empty);
            }

            string translateText(string text)
            {
                string cleaned = HttpUtility.UrlEncode(cleanText(text));
                string translated = WebClient.DownloadString(string.Format(GoogleTranslation, cleaned));
                var jsonArr = JsonConvert.DeserializeObject<JArray>(translated);
                return (string)jsonArr[0][0][0];
            }
        }
    }
}
