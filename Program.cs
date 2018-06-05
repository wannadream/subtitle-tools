using System;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace subtitle_tools
{
    class Program
    {
        const string sl = "pt";
        const string tl = "en";
        const string source = @"/home/dudey/Projects/subtitle_tools/source";
        const string target = @"/home/dudey/Projects/subtitle_tools/target";
        static readonly string googleTranslation = $"https://translate.google.com/translate_a/single?client=gtx&sl={sl}&tl={tl}&dt=t&q={{0}}";
        static readonly WebClient client = new WebClient();

        static void Main(string[] args)
        {
            foreach (var file in Directory.GetFiles(source))
            {
                string filename = file.Substring(file.LastIndexOf('/') + 1);
                using (var reader = new StreamReader(file))
                {
                    using (var writer = new StreamWriter(Path.Combine(target, filename)))
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
            }
        }

        private static string cleanText(string text)
        {
            return Regex.Replace(Regex.Replace(text, @"<[^>]*>", String.Empty), @"{[^}]*}", String.Empty);
        }

        private static string translateText(string text)
        {
            string cleaned = HttpUtility.UrlEncode(cleanText(text));
            string translated = client.DownloadString(string.Format(googleTranslation, cleaned));
            var jsonArr = JsonConvert.DeserializeObject<JArray>(translated);
            return (string)jsonArr[0][0][0];
        }
    }
}
